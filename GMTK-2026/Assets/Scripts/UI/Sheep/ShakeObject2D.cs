using UnityEngine;

/// <summary>
/// Continuously shakes a 2D object in place using Perlin noise for smooth, chaotic randomness.
/// Shake runs indefinitely until StopShake() is called or the component is disabled.
/// </summary>
public class ShakeObject2D : MonoBehaviour
{
    [Header("Shake Settings")]
    [Tooltip("Maximum distance the object can move from its origin, in each axis.")]
    public float shakeMagnitude = 0.3f;

    [Tooltip("How fast the shake oscillates. Higher = twitchier/more chaotic.")]
    public float shakeSpeed = 25f;

    [Tooltip("Roughness of the noise. Higher values = jitterier, less smooth motion.")]
    [Range(0.1f, 10f)]
    public float chaosFactor = 2f;

    [Header("Rotation Shake (optional)")]
    [Tooltip("Enable random rotational shake alongside positional shake.")]
    public bool shakeRotation = false;

    [Tooltip("Max rotation offset in degrees.")]
    public float rotationMagnitude = 5f;

    [Header("Behavior")]
    [Tooltip("Start shaking automatically when the object loads.")]
    public bool shakeOnStart = true;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isShaking;
    private float noiseSeedX;
    private float noiseSeedY;
    private float noiseSeedZ;

    void Awake()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        // Random seeds so multiple shaking objects don't move in sync
        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedY = Random.Range(0f, 1000f);
        noiseSeedZ = Random.Range(0f, 1000f);
    }

    void Start()
    {
        if (shakeOnStart)
            Shake();
    }

    void Update()
    {
        if (!isShaking) return;

        float time = Time.time * shakeSpeed;

        // Perlin noise gives smooth-but-random motion; chaosFactor scales sampling rate for jitteriness
        float offsetX = (Mathf.PerlinNoise(noiseSeedX, time * chaosFactor) * 2f - 1f) * shakeMagnitude;
        float offsetY = (Mathf.PerlinNoise(noiseSeedY, time * chaosFactor) * 2f - 1f) * shakeMagnitude;

        transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

        if (shakeRotation)
        {
            float offsetZ = (Mathf.PerlinNoise(noiseSeedZ, time * chaosFactor) * 2f - 1f) * rotationMagnitude;
            transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, offsetZ);
        }
    }

    /// <summary>Starts continuous shaking (captures current position as the origin).</summary>
    public void Shake()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        isShaking = true;
    }

    /// <summary>Stops shaking and snaps back to the original position/rotation.</summary>
    public void StopShake()
    {
        isShaking = false;
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }

    /// <summary>Toggles shaking on/off.</summary>
    public void ToggleShake()
    {
        if (isShaking) StopShake();
        else Shake();
    }
}