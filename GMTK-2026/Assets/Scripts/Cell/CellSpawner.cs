using System;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using MateStrategy;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Cell
{
    public class CellSpawner : MonoBehaviour
    {
        private const float MinSpawnInterval = 0.05f;

        [Header("Area")]
        [Tooltip("Spawn circle radius in world units, centred on this transform.")]
        [SerializeField, Min(0f)] private float radius = 3f;
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

        [Header("Debug")]
        [SerializeField] private Color gizmoColor = Color.green;

        [Inject] private CellUnit.Factory cellFactory;

        private int _aliveCount;

        private void Reset()
        {
            cellLayer = LayerMask.GetMask("Cell");
        }

        private void Start()
        {
            if (minSpacing > 0f && cellLayer.value == 0)
            {
                Debug.LogWarning("CellSpawner: Cell Layer is set to Nothing, so the spacing check is off.", this);
            }

            for (int i = 0; i < spawnOnStartCount; i++)
            {
                Spawn();
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

        public bool Spawn()
        {
            if (maxAlive > 0 && _aliveCount >= maxAlive) return false;
            if (!TryFindSpawnPoint(out var point)) return false;

            var cell = cellFactory.Create();
            cell.Initialize(MatingEnum.Default, CellSize.Small, DeviationEnum.Default, new Vector3(point.x, point.y, 0f));
            cell.OnDie += HandleSpawnedCellDied;
            _aliveCount++;
            return true;
        }

        private bool TryFindSpawnPoint(out Vector2 point)
        {
            Vector2 center = transform.position;

            // Cells created earlier this frame have moved but their colliders haven't synced yet.
            if (minSpacing > 0f) Physics2D.SyncTransforms();

            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                point = center + Random.insideUnitCircle * radius;
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
                Debug.Log("CellSpawner: spawn skipped (cap reached or no clear point found).", this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
