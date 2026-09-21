using UnityEngine;

[CreateAssetMenu(fileName = "MatingConfig", menuName = "Configs/MatingConfig")]
public class MatingConfig : ScriptableObject
{
    [SerializeField] private float matingDuration = 1.0f;
    [Tooltip("Time between increasing the mutation chance in seconds." )]
    [SerializeField] private float mutationChanceGrowthRate = 5f;
    public float MatingDuration => matingDuration;
    public float MutationChanceGrowthRate => mutationChanceGrowthRate;
}
