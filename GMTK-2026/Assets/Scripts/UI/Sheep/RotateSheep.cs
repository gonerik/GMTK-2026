using UnityEngine;

namespace UI.Sheep
{
    public class RotateSheep : MonoBehaviour
    {
        [Header("Angle Range")]
        [Tooltip("Minimum Z angle (degrees), relative to the object's starting rotation.")]
        public float minAngle = -30f;

        [Tooltip("Maximum Z angle (degrees), relative to the object's starting rotation.")]
        public float maxAngle = 30f;

        [Header("Motion")]
        [Tooltip("How fast the object swings back and forth (full cycles per second).")]
        public float speed = 1f;

        [Tooltip("Shape of the motion. Smooth = sine wave (eases at the ends). Linear = constant speed (bounces sharply at the ends).")]
        public MotionType motionType = MotionType.Smooth;

        [Tooltip("Offsets where in the cycle the object starts (0-1). Useful to desync multiple objects.")]
        [Range(0f, 1f)]
        public float phaseOffset = 0f;

        public enum MotionType { Smooth, Linear }

        private Quaternion originalRotation;
        private float timer;

        void Awake()
        {
            originalRotation = transform.localRotation;
            timer = phaseOffset / Mathf.Max(speed, 0.0001f);
        }

        void Update()
        {
            timer += Time.deltaTime * speed;

            // cycle runs 0 -> 1 repeatedly
            float cycle = timer % 1f;

            // wave oscillates -1 -> 1 -> -1, starting at 0 when cycle = 0
            float wave = motionType == MotionType.Smooth
                ? Mathf.Sin(cycle * Mathf.PI * 2f)
                : TriangleWave(cycle);

            // map [-1,1] to [minAngle, maxAngle] through 0 at wave = 0
            float angle = wave >= 0f
                ? Mathf.Lerp(0f, maxAngle, wave)
                : Mathf.Lerp(0f, minAngle, -wave);

            transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, angle);
        }

        // Triangle wave: -1 -> 1 -> -1 over one full cycle, starting at 0 when cycle = 0
        private float TriangleWave(float cycle)
        {
            float t = (cycle + 0.75f) % 1f;
            return (Mathf.Abs(t - 0.5f) * 4f) - 1f;
        }
    }
}