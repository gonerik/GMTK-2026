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

    public override void InstallBindings()
    {
        Container.BindInstance(matingConfig).AsSingle();
        Container.BindFactory<CellUnit, CellUnit.Factory>().FromComponentInNewPrefab(cellPrefab);
        Container.BindFactory<RedCell, RedCell.Factory>().FromComponentInNewPrefab(redCellPrefab);
        Container.BindInterfacesAndSelfTo<MatingService>().AsSingle().WithArguments(matingConfig,cellPrefab).NonLazy();
    }
}
