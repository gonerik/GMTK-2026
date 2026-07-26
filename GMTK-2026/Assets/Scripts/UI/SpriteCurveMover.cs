using System;
using UnityEngine;
using Zenject;

namespace UI
{
    public class SpriteCurveMover : UnityEngine.MonoBehaviour
    {
        [SerializeField] private Transform goalPosition;
        private Vector2 goalVector;

        [Inject] private GlobalTimer timer;

        private void Start()
        {
            goalVector = goalPosition.position;
            goalVector.y = transform.position.y;
        }

        void Update()
        {
            transform.position = Vector2.Lerp(transform.position, goalVector, timer.ProgressTime);
        }
    }
}