using AYellowpaper.SerializedCollections;
using DefaultNamespace;
using MateStrategy;
using UnityEngine;
using Zenject;

namespace Energy
{
    // Every Food number in one place: the board's Food, what Seeding spends, and the Food Payout a cell's
    // death returns in the Food dish. The initialisers pre-fill a freshly created asset; once it exists, the
    // asset holds the live values.
    [CreateAssetMenu(fileName = "FoodConfig", menuName = "Configs/FoodConfig")]
    public class FoodConfig : ScriptableObjectInstaller
    {
        [Header("Board")]
        [SerializeField, Min(0)] private int startingFood = 100;
        [SerializeField, Min(1)] private int maxFood = 100;

        [Header("Seeding")]
        [Tooltip("Food spent on each seeded cell. A spawn is skipped while the board holds less.")]
        [SerializeField, Min(0)] private int foodPerSeededCell = 2;

        // A cell's Food Payout is the sum of these three, like its Energy Payout in CellLifetimeConfig. The
        // starting values copy the Energy ones, except Red: Food Payouts are gains only.
        [Header("Payout")]
        [SerializeField] private SerializedDictionary<MatingEnum, int> strainFoodPayout = new SerializedDictionary<MatingEnum, int>
        {
            { MatingEnum.Default, 0 },
            { MatingEnum.Agressive, 5 },
            { MatingEnum.Horny, 1 },
        };
        [SerializeField] private SerializedDictionary<CellSize, int> stageFoodPayout = new SerializedDictionary<CellSize, int>
        {
            { CellSize.Small, 1 },
            { CellSize.Medium, 2 },
            { CellSize.Large, 3 },
        };
        [SerializeField] private SerializedDictionary<DeviationEnum, int> deviationFoodPayout = new SerializedDictionary<DeviationEnum, int>
        {
            { DeviationEnum.Default, 0 },
            { DeviationEnum.Red, 0 },
            { DeviationEnum.Yellow, 20 },
            { DeviationEnum.Blue, 5 },
        };

        public int StartingFood => startingFood;
        public int MaxFood => maxFood;
        public int FoodPerSeededCell => foodPerSeededCell;

        public override void InstallBindings()
        {
            Container.BindInstance(this).AsSingle();
        }

        // Never negative: a loss always costs Energy, so the Food dish can't drain Food.
        public int CalculateFoodPayout(MatingEnum strain, CellSize stage, DeviationEnum deviation)
        {
            int payout = strainFoodPayout[strain] + stageFoodPayout[stage] + deviationFoodPayout[deviation];
            return Mathf.Max(0, payout);
        }
    }
}
