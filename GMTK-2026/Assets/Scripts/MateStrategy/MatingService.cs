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
            CellSize newSize = IncrementSize(mate1.CellSize);
            Vector3 spawnPos = (mate1.GetTargetPosition() + mate2.GetTargetPosition()) / 2f;

            cellFactory.Create().Initialize(resultEnum, newSize, spawnPos);

            mate1.Destroy();
            mate2.Destroy();
        }

        private MatingEnum DetermineResultingEnum(MatingEnum parent1, MatingEnum parent2)
        {
            List<MatingEnum> pool = new List<MatingEnum>();

            for (int i = 0; i < 50; i++) pool.Add(parent1);
            for (int i = 0; i < 50; i++) pool.Add(parent2);
            for (int i = 0; i < mutationChance; i++) pool.Add(MatingEnum.Red);

            int randomIndex = UnityEngine.Random.Range(0, pool.Count);
            return pool[randomIndex];
        }

        private CellSize IncrementSize(CellSize currentSize)
        {
            switch (currentSize)
            {
                case CellSize.Acid: return CellSize.Small;
                case CellSize.Small: return CellSize.Medium;
                case CellSize.Medium: return CellSize.Large;
                case CellSize.Large: return CellSize.Large; // Max size
                default: return currentSize;
            }
        }
    }
}