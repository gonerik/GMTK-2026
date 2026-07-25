using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using Interfaces;
using UnityEngine;
using Zenject;

namespace MateStrategy
{
    public class MatingService
    {
        private MatingConfig matingConfig;
        
        [Inject] private CellUnit.Factory cellFactory;
        [Inject] private RedCell.Factory redCellFactory;
        [Inject] private MatingProbabilities matingProbabilities;

        private int mutationChance;
        private static readonly int HornyAdditionalSpawnChance = 50;

        [Inject]
        public MatingService(MatingConfig matingConfig, GameObject cellPrefab)
        {
            this.matingConfig = matingConfig;
            StartMutationGrowth().Forget();
        }

        private async UniTaskVoid StartMutationGrowth()
        {
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(matingConfig.MutationChanceGrowthRate));
                mutationChance++;
            }
        }

        public void Mate(IMate mate1, IMate mate2)
        {
            if (mate1 == null || mate2 == null)
            {
                return;
            }

            if (mate1.GetView().CellSize != mate2.GetView().CellSize)
            {
                return;
            }

            if (mate1.IsMating || mate2.IsMating)
            {
                return;
            }

            if (mate1.CellSize == CellSize.Large || mate2.CellSize == CellSize.Large)
            {
                return;
            }

            mate1.IsMating = true;
            mate2.IsMating = true;
            
            

            MatingEnum resultEnum = DetermineResultingEnum(mate1.GetMatingEnum(), mate2.GetMatingEnum());
            CellSize newSize = mate1.CellSize + 1;
            Vector3 spawnPos = (mate1.GetTargetPosition() + mate2.GetTargetPosition()) / 2f;

            if (mate1.GetMatingEnum() == MatingEnum.Horny)
            {
                int additionalSpawnProbability = UnityEngine.Random.Range(0, 100);
                if (additionalSpawnProbability <= HornyAdditionalSpawnChance)
                {
                    MatingEnum resultEnumHorny1 = DetermineResultingEnum(mate1.GetMatingEnum(), mate2.GetMatingEnum());
                    Vector3 spawnOffset = UnityEngine.Random.insideUnitCircle * 1f;
                    HandleMutationAndCreation(resultEnumHorny1, mate1.CellSize, spawnPos + spawnOffset);
                }
            }
            if(mate2.GetMatingEnum() == MatingEnum.Horny)
            {
                int additionalSpawnProbability = UnityEngine.Random.Range(0, 100);
                if (additionalSpawnProbability <= HornyAdditionalSpawnChance)
                {
                    MatingEnum resultEnumHorny2 = DetermineResultingEnum(mate1.GetMatingEnum(), mate2.GetMatingEnum());
                    Vector3 spawnOffset = UnityEngine.Random.insideUnitCircle * 1f;
                    HandleMutationAndCreation(resultEnumHorny2, mate2.CellSize, spawnPos + spawnOffset);
                }
            }

            if (mate1.GetMatingEnum() == MatingEnum.Default && mate2.GetMatingEnum() == MatingEnum.Default)
            {
                CreateAndInitializeCell(resultEnum, newSize, spawnPos);
            }
            else
            {
                HandleMutationAndCreation(resultEnum, newSize, spawnPos);
            }
            mate1.Destroy();
            mate2.Destroy();
        }

        private void HandleMutationAndCreation(MatingEnum resultEnum, CellSize newSize, Vector3 spawnPos)
        {
            int mutationProbability = UnityEngine.Random.Range(0, 100);
            int yellowMutationChance = matingProbabilities.GetProbability(DeviationEnum.Yellow) + mutationChance;
            int redMutationChance = matingProbabilities.GetProbability(DeviationEnum.Red) + mutationChance;
            int blueMutationChance = matingProbabilities.GetProbability(DeviationEnum.Blue) + mutationChance;
            
            if (mutationProbability <= blueMutationChance)
            {
                cellFactory.Create().Initialize(resultEnum, newSize, DeviationEnum.Blue, spawnPos);
            }
            else if (mutationProbability <= yellowMutationChance)
            {
                cellFactory.Create().Initialize(resultEnum, newSize, DeviationEnum.Yellow, spawnPos);
            }
            else if (mutationProbability <= redMutationChance)
            {
                redCellFactory.Create().Initialize(resultEnum, newSize, spawnPos);
            }
            else
            {
                CreateAndInitializeCell(resultEnum, newSize, spawnPos);
            }
        }

        private void CreateAndInitializeCell(MatingEnum resultEnum, CellSize newSize, Vector3 spawnPos)
        {
            cellFactory.Create().Initialize(resultEnum, newSize,  DeviationEnum.Default, spawnPos);
        }

        private MatingEnum DetermineResultingEnum(MatingEnum parent1, MatingEnum parent2)
        {
            int randomIndex = UnityEngine.Random.Range(0, 100);
            if (randomIndex <= 50)
            {
                return parent1;
            }
            return parent2;
        }
    }
}