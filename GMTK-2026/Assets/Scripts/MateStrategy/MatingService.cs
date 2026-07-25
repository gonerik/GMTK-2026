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
        private GameObject cellPrefab;
        
        [Inject] private Cell.Factory cellFactory;
        [Inject] private RedCell.Factory redCellFactory;

        private int mutationChance;

        [Inject]
        public MatingService(MatingConfig matingConfig, GameObject cellPrefab)
        {
            this.matingConfig = matingConfig;
            this.cellPrefab = cellPrefab;
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

            mate1.IsMating = true;
            mate2.IsMating = true;
            
            

            MatingEnum resultEnum = DetermineResultingEnum(mate1.GetMatingEnum(), mate2.GetMatingEnum());
            CellSize newSize = mate1.CellSize + 1;
            Vector3 spawnPos = (mate1.GetTargetPosition() + mate2.GetTargetPosition()) / 2f;
            
            int mutationProbability = UnityEngine.Random.Range(0, 100);
            if (mutationProbability <= mutationChance)
            {
                redCellFactory.Create().Initialize(resultEnum, newSize, spawnPos);
            }
            else
            {
                cellFactory.Create().Initialize(resultEnum, newSize, spawnPos);
            }
            

            mate1.Destroy();
            mate2.Destroy();
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