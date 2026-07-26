using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using FMODUnity;
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
        private const string MutateSoundID = "event:/Cell mutates";

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

            if (mate1.GetView().CellSize == CellSize.Small && mate2.GetView().CellSize == CellSize.Acid && mate1.GetMatingEnum() != MatingEnum.Horny)
            {
                mate1.IsMating = true;
                mate2.IsMating = true;
                CreateAndInitializeCell(mate1.GetMatingEnum()+1, mate1.CellSize, mate1.GetTargetPosition());
                mate1.Destroy();
                mate2.Destroy();
                return;
            }
            if(mate2.GetView().CellSize == CellSize.Small && mate1.GetView().CellSize == CellSize.Acid && mate2.GetMatingEnum() != MatingEnum.Horny)
            {
                mate1.IsMating = true;
                mate2.IsMating = true;
                CreateAndInitializeCell(mate2.GetMatingEnum()+1, mate2.CellSize, mate2.GetTargetPosition());
                mate1.Destroy();
                mate2.Destroy();
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

            if (mate1.GetMatingEnum() == MatingEnum.Agressive || mate2.GetMatingEnum() == MatingEnum.Agressive)
            {
                resultEnum = MatingEnum.Horny;
            }

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
                FMODUnity.RuntimeManager.PlayOneShot(MutateSoundID);
                cellFactory.Create().Initialize(resultEnum, newSize, DeviationEnum.Blue, spawnPos);
            }
            else if (mutationProbability <= yellowMutationChance)
            {
                FMODUnity.RuntimeManager.PlayOneShot(MutateSoundID);
                cellFactory.Create().Initialize(resultEnum, newSize, DeviationEnum.Yellow, spawnPos);
            }
            else if (mutationProbability <= redMutationChance)
            {
                FMODUnity.RuntimeManager.PlayOneShot(MutateSoundID);
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
            Vector2 probabilityMatrix  = new Vector2();
            probabilityMatrix.x = matingConfig.GetMatingChanceModifier(parent1);
            probabilityMatrix.y = matingConfig.GetMatingChanceModifier(parent2);
            probabilityMatrix.Normalize();
            probabilityMatrix *= 100;
            int randomIndex = UnityEngine.Random.Range(0, 100);
            if (randomIndex <= probabilityMatrix.x)
            {
                return parent1;
            }
            return parent2;
        }
    }
}