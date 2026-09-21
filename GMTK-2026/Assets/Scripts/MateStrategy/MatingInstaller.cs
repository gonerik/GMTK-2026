using DefaultNamespace;
using MateStrategy;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "MatingInstaller", menuName = "Installers/MatingInstaller")]
public class MatingInstaller : ScriptableObjectInstaller<MatingInstaller>
{
    [SerializeField] private MatingConfig matingConfig;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject redCellPrefab;
    [Tooltip("Agressive Acid: promotes Ordinary Small cells to Agressive.")]
    [SerializeField] private GameObject acidPrefab;
    [Tooltip("Horny Acid: promotes Agressive Small cells to Horny.")]
    [SerializeField] private GameObject hornyAcidPrefab;

    public override void InstallBindings()
    {
        Container.BindInstance(matingConfig).AsSingle();
        Container.BindFactory<CellUnit, CellUnit.Factory>().FromComponentInNewPrefab(cellPrefab);
        Container.BindFactory<RedCell, RedCell.Factory>().FromComponentInNewPrefab(redCellPrefab);
        Container.BindFactory<Acid, Acid.AgressiveFactory>().FromComponentInNewPrefab(acidPrefab);

        // A null prefab would throw at install time and stop the scene loading, so an unassigned slot
        // degrades to "no Horny Acid" instead. MatingService injects this factory as optional.
        if (hornyAcidPrefab != null)
        {
            Container.BindFactory<Acid, Acid.HornyFactory>().FromComponentInNewPrefab(hornyAcidPrefab);
        }
        else
        {
            Debug.LogWarning("MatingInstaller: 'Horny Acid Prefab' is unassigned on MatingInstaller.asset. " +
                             "Horny Acid will never spawn, so Agressive cells cannot be promoted to Horny.", this);
        }

        Container.BindInterfacesAndSelfTo<MatingService>().AsSingle().WithArguments(matingConfig,cellPrefab).NonLazy();
    }
}
