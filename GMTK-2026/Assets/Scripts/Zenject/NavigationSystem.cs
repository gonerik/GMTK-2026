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
        public Dictionary<ITarget, ITarget> targets = new Dictionary<ITarget, ITarget>();
        
        private List<ITarget> _targets = new List<ITarget>();
        private CancellationTokenSource _cts;

        // Dish rims live on this layer. Vision reaches further than the gap between dishes, so without a
        // line-of-sight check a cell would chase prey in another dish and push into its own rim forever.
        private const string WallLayer = "PetriDish";
        private readonly RaycastHit2D[] _wallHits = new RaycastHit2D[1];
        private ContactFilter2D _walls;

        public void Initialize()
        {
            // Triggers are skipped: each dish's trigger circle sits on the same layer as its rim.
            _walls = new ContactFilter2D { useTriggers = false };
            _walls.SetLayerMask(LayerMask.GetMask(WallLayer));

            _cts = new CancellationTokenSource();
            UpdateLoop(_cts.Token).Forget();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public void RegisterTarget(ITarget target)
        {
            if (!_targets.Contains(target))
                _targets.Add(target);
        }

        public void UnregisterTarget(ITarget target)
        {
            _targets.Remove(target);
            
            // Remove from targets dictionary
            List<ITarget> keysToRemove = new List<ITarget>();
            foreach (var pair in targets)
            {
                if (pair.Value == target || pair.Key == target)
                {
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                targets.Remove(key);
            }
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
            if (_targets.Count < 2)
            {
                targets.Clear();
                return;
            }

            // Capture data on main thread
            var currentTargets = new List<ITarget>(_targets);
            int targetCount = currentTargets.Count;
            
            NativeArray<Vector2> targetPositions = new NativeArray<Vector2>(targetCount, Allocator.Persistent);
            NativeArray<float> detectionRanges = new NativeArray<float>(targetCount, Allocator.Persistent);
            NativeArray<int> targetIndices = new NativeArray<int>(targetCount, Allocator.Persistent);

            NativeArray<bool> canTargetMatrix = new NativeArray<bool>(targetCount * targetCount, Allocator.Persistent);

            for (int i = 0; i < targetCount; i++)
            {
                var target = currentTargets[i];
                targetPositions[i] = target.GetTargetPosition();
                detectionRanges[i] = target.DetectionRange;
                targetIndices[i] = -1;
            }

            for (int i = 0; i < targetCount; i++)
            {
                var t1 = currentTargets[i];
                for (int j = 0; j < targetCount; j++)
                {
                    var t2 = currentTargets[j];
                    if (t1 == t2 || targetPositions[j] == Vector2.zero)
                    {
                        canTargetMatrix[i * targetCount + j] = false;
                        continue;
                    }
                    
                    canTargetMatrix[i * targetCount + j] = t1.CanTarget(t2) && CanReach(targetPositions[i], targetPositions[j], detectionRanges[i]);
                }
            }

            // Offload to background thread
            await UniTask.RunOnThreadPool(() =>
            {
                var job = new CalculateTargetsJob
                {
                    TargetPositions = targetPositions,
                    DetectionRanges = detectionRanges,
                    CanTargetMatrix = canTargetMatrix,
                    TargetCount = targetCount,
                    TargetIndices = targetIndices
                };
                job.Schedule(targetCount, 64).Complete();
            }, cancellationToken: token);

            // Back to main thread to update dictionary
            Dictionary<ITarget, ITarget> newTargets = new Dictionary<ITarget, ITarget>();
            for (int i = 0; i < targetCount; i++)
            {
                int targetIndex = targetIndices[i];
                if (targetIndex != -1)
                {
                    var source = currentTargets[i];
                    var target = currentTargets[targetIndex];

                    // Check if they are still registered
                    if (_targets.Contains(source) && _targets.Contains(target))
                    {
                        newTargets[source] = target;
                    }
                }
            }
            targets = newTargets;

            targetPositions.Dispose();
            detectionRanges.Dispose();
            canTargetMatrix.Dispose();
            targetIndices.Dispose();
        }

        // A cell only chases what it can reach: in range, with no dish wall in between. The job rejects
        // out-of-range pairs too; checking range here first keeps the linecast to the pairs that need it.
        private bool CanReach(Vector2 from, Vector2 to, float range)
        {
            if ((to - from).sqrMagnitude > range * range)
            {
                return false;
            }

            return Physics2D.Linecast(from, to, _walls, _wallHits) == 0;
        }

        [BurstCompile]
        private struct CalculateTargetsJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector2> TargetPositions;
            [ReadOnly] public NativeArray<float> DetectionRanges;
            [ReadOnly] public NativeArray<bool> CanTargetMatrix;
            public int TargetCount;
            public NativeArray<int> TargetIndices;

            public void Execute(int index)
            {
                Vector2 targetPos = TargetPositions[index];
                if (targetPos == Vector2.zero) return;

                float detectionRange = DetectionRanges[index];
                if (detectionRange <= 0) return;

                float detectionRangeSq = detectionRange * detectionRange;

                int closestIndex = -1;
                float minDistanceSq = float.MaxValue;

                for (int j = 0; j < TargetCount; j++)
                {
                    if (!CanTargetMatrix[index * TargetCount + j]) continue;

                    float distSq = (TargetPositions[j] - targetPos).sqrMagnitude;
                    if (distSq <= detectionRangeSq && distSq < minDistanceSq)
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