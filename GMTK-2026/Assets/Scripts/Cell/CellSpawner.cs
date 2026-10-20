using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using Energy;
using MateStrategy;
using UI.Reel;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Cell
{
    public class CellSpawner : MonoBehaviour
    {
        private const float MinSpawnInterval = 0.05f;

        // Where Seeding lands while its dish is the Selected dish.
        [Serializable]
        private class SeedingArea
        {
            [Tooltip("The dish button that selects this dish.")]
            public PetriDishCameraEnum dish;
            [Tooltip("Centre of the spawn circle, e.g. the dish's PetriDish transform.")]
            public Transform center;
            [Tooltip("Spawn circle radius in world units. Keep it inside the dish.")]
            [Min(0f)] public float radius = 8f;
        }

        [Header("Area")]
        [Tooltip("Seeding lands in the Selected dish's area. The first area is used until the player selects another dish. A dish with no area, such as the Fast dish, leaves Seeding where it was.")]
        [SerializeField] private List<SeedingArea> seedingAreas = new List<SeedingArea>();
        [Tooltip("Minimum clear distance from any existing cell. 0 disables the check.")]
        [SerializeField, Min(0f)] private float minSpacing = 0.5f;
        [Tooltip("Layers checked by the spacing test.")]
        [SerializeField] private LayerMask cellLayer;
        [Tooltip("Random points tried before a spawn is skipped.")]
        [SerializeField, Min(1)] private int maxPlacementAttempts = 10;

        [Header("Timing")]
        [SerializeField] private bool spawnOnTimer = true;
        [Tooltip("Seconds before the first timed spawn.")]
        [SerializeField, Min(0f)] private float initialDelay = 0f;
        [Tooltip("Seconds between timed spawns.")]
        [SerializeField, Min(0f)] private float spawnInterval = 5f;
        [Tooltip("Cells spawned at once in Start.")]
        [SerializeField, Min(0)] private int spawnOnStartCount = 0;

        [Header("Limits")]
        [Tooltip("Max cells from this spawner alive at once. 0 = unlimited.")]
        [SerializeField, Min(0)] private int maxAlive = 0;

        [Header("Seeding")]
        [Tooltip("Strain seeded until the reel picks one. Keep it Default (Ordinary), the only strain Discovered from the start.")]
        [SerializeField] private MatingEnum defaultSeedStrain = MatingEnum.Default;

        [Header("Predators")]
        [Tooltip("Chance (%) that a seeded cell arrives with a Predator beside it. The Predator costs no Food; the Start burst never rolls.")]
        [SerializeField, Range(0, 100)] private int predatorChance = 5;

        [Header("Debug")]
        [SerializeField] private Color gizmoColor = Color.green;

        [Inject] private CellUnit.Factory cellFactory;
        [Inject] private SignalBus signalBus;
        [Inject] private FoodService foodService;
        [Inject] private FoodConfig foodConfig;
        [Inject] private MatingService matingService;

        private int _aliveCount;
        private MatingEnum _seedStrain;
        private SeedingArea _area;

        private void Reset()
        {
            cellLayer = LayerMask.GetMask("Cell");
        }

        private void Awake()
        {
            _seedStrain = defaultSeedStrain;
            _area = seedingAreas.Count > 0 ? seedingAreas[0] : null;
        }

        private void OnEnable()
        {
            signalBus.Subscribe<ReelSelectorController.SeedStrainChangedSignal>(HandleSeedStrainChanged);
            signalBus.Subscribe<CameraSwitchButton.DishSelectedSignal>(HandleDishSelected);
        }

        private void OnDisable()
        {
            signalBus.Unsubscribe<ReelSelectorController.SeedStrainChangedSignal>(HandleSeedStrainChanged);
            signalBus.Unsubscribe<CameraSwitchButton.DishSelectedSignal>(HandleDishSelected);
        }

        private void Start()
        {
            if (minSpacing > 0f && cellLayer.value == 0)
            {
                Debug.LogWarning("CellSpawner: Cell Layer is set to Nothing, so the spacing check is off.", this);
            }

            if (_area == null || _area.center == null)
            {
                Debug.LogWarning("CellSpawner: the first Seeding Area is missing or has no Center, so nothing is seeded.", this);
            }

            // The opening seed never brings a Predator.
            for (int i = 0; i < spawnOnStartCount; i++)
            {
                Spawn(mayBringPredator: false);
            }

            if (spawnOnTimer)
            {
                SpawnLoop().Forget();
            }
        }

        private async UniTaskVoid SpawnLoop()
        {
            var token = this.GetCancellationTokenOnDestroy();
            await UniTask.Delay(TimeSpan.FromSeconds(initialDelay), cancellationToken: token);

            while (true)
            {
                if (enabled) Spawn();
                await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(spawnInterval, MinSpawnInterval)), cancellationToken: token);
            }
        }

        public bool Spawn(bool mayBringPredator = true)
        {
            if (maxAlive > 0 && _aliveCount >= maxAlive) return false;
            if (!TryFindSpawnPoint(out var point)) return false;
            // Paid only once there is somewhere to put the cell, so a crowded tick costs nothing.
            if (!foodService.TrySpend(foodConfig.FoodPerSeededCell)) return false;

            var cell = cellFactory.Create();
            cell.Initialize(_seedStrain, CellSize.Small, DeviationEnum.Default, new Vector3(point.x, point.y, 0f));
            cell.OnDie += HandleSpawnedCellDied;
            _aliveCount++;

            if (mayBringPredator && Random.Range(0, 100) < predatorChance)
            {
                BringPredator(point);
            }
            return true;
        }

        // A Predator arrives beside the seeded cell (ADR-0008): free, uncounted by maxAlive, and skipped when
        // no clear point lies far enough from it. The explicit distance matters because the seeded cell's
        // collider is still scaled to zero this frame, so the spacing check can't see the body it grows into.
        private void BringPredator(Vector2 seededAt)
        {
            if (TryFindSpawnPoint(out var point, seededAt, matingService.PredatorClearance(CellSize.Small)))
            {
                matingService.SpawnPredator(_seedStrain, CellSize.Small, new Vector3(point.x, point.y, 0f));
            }
        }

        private bool TryFindSpawnPoint(out Vector2 point, Vector2? avoid = null, float avoidDistance = 0f)
        {
            if (_area == null || _area.center == null)
            {
                point = default;
                return false;
            }

            Vector2 center = _area.center.position;
            float radius = _area.radius;

            // Cells created earlier this frame have moved but their colliders haven't synced yet.
            if (minSpacing > 0f) Physics2D.SyncTransforms();

            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                point = center + Random.insideUnitCircle * radius;
                if (avoid.HasValue && Vector2.Distance(point, avoid.Value) < avoidDistance) continue;
                if (minSpacing <= 0f || Physics2D.OverlapCircle(point, minSpacing, cellLayer) == null) return true;
            }

            point = default;
            return false;
        }

        private void HandleSpawnedCellDied(CellUnit cell)
        {
            cell.OnDie -= HandleSpawnedCellDied;
            _aliveCount--;
        }

        private void HandleSeedStrainChanged(ReelSelectorController.SeedStrainChangedSignal signal)
        {
            _seedStrain = signal.Strain;
        }

        // Seeding follows the Selected dish. A dish with no area (the Fast dish) leaves it where it was.
        private void HandleDishSelected(CameraSwitchButton.DishSelectedSignal signal)
        {
            SeedingArea area = seedingAreas.Find(a => a.dish == signal.Dish);
            if (area != null)
            {
                _area = area;
            }
        }

        [ContextMenu("Spawn Cell")]
        private void SpawnFromContextMenu()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("CellSpawner: Spawn Cell only works in Play mode.", this);
                return;
            }

            if (!Spawn())
            {
                Debug.Log("CellSpawner: spawn skipped (cap reached, no seeding area, no clear point found, or not enough food).", this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            foreach (SeedingArea area in seedingAreas)
            {
                if (area?.center != null) Gizmos.DrawWireSphere(area.center.position, area.radius);
            }
        }
    }
}
