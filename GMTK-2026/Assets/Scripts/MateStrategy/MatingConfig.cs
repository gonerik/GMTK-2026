using DefaultNamespace;
using UnityEngine;

[CreateAssetMenu(fileName = "MatingConfig", menuName = "Configs/MatingConfig")]
public class MatingConfig : ScriptableObject
{
    [SerializeField] private float matingDuration = 1.0f;
    [Tooltip("Time between increasing the mutation chance in seconds." )]
    [SerializeField] private float mutationChanceGrowthRate = 5f;
    [Header("Devouring")]
    [Tooltip("How many Smalls a devoured Medium is worth, and how many Mediums a devoured Large is worth.")]
    [SerializeField] private int stageNourishmentRatio = 3;
    [Tooltip("How many meals of its own stage an Agressive cell needs to grow a stage, or to Burst when Large.")]
    [SerializeField] private int mealsToGrow = 3;
    [Tooltip("Seconds of life a meal gives back to its devourer, whatever the prey's stage, never past its full lifespan.")]
    [SerializeField, Min(0f)] private float lifeRestoredPerMeal = 3f;
    [Header("Horny Brood")]
    [Tooltip("Starting chance (%) that a Horny mating's Brood holds a second copy of the partner. The first copy is guaranteed. The chance returns here whenever the second copy appears.")]
    [SerializeField, Range(0, 100)] private int secondPartnerCopyChance = 50;
    [Tooltip("Percentage points the second-copy chance gains each Horny mating that misses it, up to 100.")]
    [SerializeField, Range(0, 100)] private int secondPartnerCopyChanceGrowth = 10;
    [Tooltip("Starting chance (%) that a Horny mating's Brood holds a copy of the Horny parent. The chance returns here whenever the copy appears.")]
    [SerializeField, Range(0, 100)] private int selfCopyChance = 50;
    [Tooltip("Percentage points the self-copy chance gains each Horny mating that misses it, up to 100.")]
    [SerializeField, Range(0, 100)] private int selfCopyChanceGrowth = 10;
    [Header("Predators")]
    [Tooltip("Clear edge-to-edge gap between a Predator and the new cells it arrives beside.")]
    [SerializeField, Min(0f)] private float predatorArrivalGap = 1f;
    [Tooltip("Directions tried for a Predator's arrival before it is skipped because dish walls leave no room.")]
    [SerializeField, Min(1)] private int predatorArrivalAttempts = 8;
    public float MatingDuration => matingDuration;
    public float MutationChanceGrowthRate => mutationChanceGrowthRate;
    public float LifeRestoredPerMeal => lifeRestoredPerMeal;
    public int SecondPartnerCopyChance => secondPartnerCopyChance;
    public int SecondPartnerCopyChanceGrowth => secondPartnerCopyChanceGrowth;
    public int SelfCopyChance => selfCopyChance;
    public int SelfCopyChanceGrowth => selfCopyChanceGrowth;
    public float PredatorArrivalGap => predatorArrivalGap;
    public int PredatorArrivalAttempts => predatorArrivalAttempts;

    // What a devoured cell of this stage adds to its devourer's Nourishment.
    public int GetNourishment(CellSize cellSize)
    {
        switch (cellSize)
        {
            case CellSize.Small:
                return 1;
            case CellSize.Medium:
                return stageNourishmentRatio;
            case CellSize.Large:
                return stageNourishmentRatio * stageNourishmentRatio;
            default:
                return 0;
        }
    }

    // The Nourishment an Agressive cell of this stage needs to grow a stage, or to Burst when Large.
    public int GetNourishmentToGrow(CellSize cellSize) => mealsToGrow * GetNourishment(cellSize);
}
