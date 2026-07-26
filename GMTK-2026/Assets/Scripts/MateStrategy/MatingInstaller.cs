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
    [SerializeField] private GameObject acidPrefab;

    public override void InstallBindings()
    {
        Container.BindInstance(matingConfig).AsSingle();
        Container.BindFactory<CellUnit, CellUnit.Factory>().FromComponentInNewPrefab(cellPrefab);
        Container.BindFactory<RedCell, RedCell.Factory>().FromComponentInNewPrefab(redCellPrefab);
        Container.BindFactory<Acid, Acid.Factory>().FromComponentInNewPrefab(acidPrefab);
        Container.BindInterfacesAndSelfTo<MatingService>().AsSingle().WithArguments(matingConfig,cellPrefab).NonLazy();
    }
}
