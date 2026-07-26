using AYellowpaper.SerializedCollections;
using MateStrategy;
using UnityEngine;

[CreateAssetMenu(fileName = "MatingConfig", menuName = "Configs/MatingConfig")]
public class MatingConfig : ScriptableObject
{
    [SerializeField] private float matingDuration = 1.0f;
    [Tooltip("Time between increasing the mutation chance in seconds." )]
    [SerializeField] private float mutationChanceGrowthRate = 5f;
    [SerializeField] private SerializedDictionary<MatingEnum, int> matingChaceModifier;
    public float MatingDuration => matingDuration;
    public float MutationChanceGrowthRate => mutationChanceGrowthRate;
    public int GetMatingChanceModifier(MatingEnum matingEnum) => matingChaceModifier[matingEnum];
}
