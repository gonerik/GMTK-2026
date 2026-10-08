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
    [Header("Horny Brood")]
    [Tooltip("Chance (%) that a Horny mating's Brood holds a second copy of the partner. The first copy is guaranteed.")]
    [SerializeField, Range(0, 100)] private int secondPartnerCopyChance = 50;
    [Tooltip("Chance (%) that a Horny mating's Brood holds a copy of the Horny parent.")]
    [SerializeField, Range(0, 100)] private int selfCopyChance = 50;
    public float MatingDuration => matingDuration;
    public float MutationChanceGrowthRate => mutationChanceGrowthRate;
    public int SecondPartnerCopyChance => secondPartnerCopyChance;
    public int SelfCopyChance => selfCopyChance;

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
