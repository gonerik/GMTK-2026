using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Interfaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Zenject;

namespace DefaultNamespace.Zenject
{
    public class NavigationSystem : IInitializable, IDisposable
    {
        public Dictionary<IConsumer, IEatable> targets = new Dictionary<IConsumer, IEatable>();
        
        private List<IEatable> _eatables = new List<IEatable>();
        private List<IConsumer> _consumers = new List<IConsumer>();
        private CancellationTokenSource _cts;

        public void Initialize()
        {
            _cts = new CancellationTokenSource();
            UpdateLoop(_cts.Token).Forget();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public void RegisterEatable(IEatable eatable)
        {
            if (!_eatables.Contains(eatable))
                _eatables.Add(eatable);
        }

        public void UnregisterEatable(IEatable eatable)
        {
            _eatables.Remove(eatable);
        }

        public void RegisterConsumer(IConsumer consumer)
        {
            if (!_consumers.Contains(consumer))
                _consumers.Add(consumer);
        }

        public void UnregisterConsumer(IConsumer consumer)
        {
            _consumers.Remove(consumer);
            targets.Remove(consumer);
        }

        private async UniTaskVoid UpdateLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UpdateTargets(token);
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            }
        }

        private async UniTask UpdateTargets(CancellationToken token)
        {
            if (_consumers.Count == 0 || _eatables.Count == 0)
            {
                targets.Clear();
                return;
            }

            // Capture data on main thread
            int consumerCount = _consumers.Count;
            int eatableCount = _eatables.Count;
            
            NativeArray<Vector2> consumerPositions = new NativeArray<Vector2>(consumerCount, Allocator.Persistent);
            NativeArray<Vector2> eatablePositions = new NativeArray<Vector2>(eatableCount, Allocator.Persistent);
            NativeArray<int> targetIndices = new NativeArray<int>(consumerCount, Allocator.Persistent);

            // We still need to check CanBeEaten. 
            // Since it uses Predicates, we can't do it inside Burst.
            // But we can pre-calculate a bitmask or a 2D array of "who can eat whom".
            NativeArray<bool> canEatMatrix = new NativeArray<bool>(consumerCount * eatableCount, Allocator.Persistent);

            for (int j = 0; j < eatableCount; j++)
            {
                var eatable = _eatables[j];
                if (eatable is MonoBehaviour emb && emb != null)
                {
                    eatablePositions[j] = emb.transform.position;
                }
                else
                {
                    eatablePositions[j] = Vector2.zero;
                }
            }

            for (int i = 0; i < consumerCount; i++)
            {
                var consumer = _consumers[i];
                if (consumer is not MonoBehaviour cmb || cmb == null)
                {
                    consumerPositions[i] = Vector2.zero;
                    continue;
                }
                consumerPositions[i] = cmb.transform.position;
                targetIndices[i] = -1;

                for (int j = 0; j < eatableCount; j++)
                {
                    var eatable = _eatables[j];
                    if (eatable == (IEatable)consumer || eatablePositions[j] == Vector2.zero)
                    {
                        canEatMatrix[i * eatableCount + j] = false;
                        continue;
                    }
                    
                    canEatMatrix[i * eatableCount + j] = eatable.CanBeEaten(consumer);
                }
            }

            // Offload to background thread
            await UniTask.RunOnThreadPool(() =>
            {
                var job = new CalculateTargetsJob
                {
                    ConsumerPositions = consumerPositions,
                    EatablePositions = eatablePositions,
                    CanEatMatrix = canEatMatrix,
                    EatableCount = eatableCount,
                    TargetIndices = targetIndices
                };
                job.Schedule(consumerCount, 64).Complete();
            }, cancellationToken: token);

            // Back to main thread to update dictionary
            Dictionary<IConsumer, IEatable> newTargets = new Dictionary<IConsumer, IEatable>();
            for (int i = 0; i < consumerCount; i++)
            {
                int targetIndex = targetIndices[i];
                if (targetIndex != -1)
                {
                    newTargets[_consumers[i]] = _eatables[targetIndex];
                }
            }
            targets = newTargets;

            consumerPositions.Dispose();
            eatablePositions.Dispose();
            canEatMatrix.Dispose();
            targetIndices.Dispose();
        }

        [BurstCompile]
        private struct CalculateTargetsJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector2> ConsumerPositions;
            [ReadOnly] public NativeArray<Vector2> EatablePositions;
            [ReadOnly] public NativeArray<bool> CanEatMatrix;
            public int EatableCount;
            public NativeArray<int> TargetIndices;

            public void Execute(int index)
            {
                Vector2 consumerPos = ConsumerPositions[index];
                if (consumerPos == Vector2.zero) return;

                int closestIndex = -1;
                float minDistanceSq = float.MaxValue;

                for (int j = 0; j < EatableCount; j++)
                {
                    if (!CanEatMatrix[index * EatableCount + j]) continue;

                    float distSq = (EatablePositions[j] - consumerPos).sqrMagnitude;
                    if (distSq < minDistanceSq)
                    {
                        minDistanceSq = distSq;
                        closestIndex = j;
                    }
                }

                TargetIndices[index] = closestIndex;
            }
        }
    }
}