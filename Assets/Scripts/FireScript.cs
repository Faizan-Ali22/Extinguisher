using UnityEngine;
using DigitalRuby.PyroParticles;

/// <summary>
/// Controls fire health, visual feedback, and extinguishing mechanics.
/// Each fire has HP that decreases when water is applied.
/// Fire visually shrinks as HP drops and is fully extinguished at 0.
/// Attach to each WallOfFire object in the scene.
/// </summary>
public class FireScript : MonoBehaviour
{
    [Header("Fire Health")]
    [Tooltip("Starting health of this fire")]
    public float maxHP = 100f;

    [Tooltip("Rate at which unattended fire grows back (HP/sec). Creates urgency.")]
    public float spreadRate = 3f;

    [Tooltip("Maximum HP the fire can grow to if left unattended")]
    public float maxSpreadHP = 130f;

    [Header("Score")]
    [Tooltip("Base points awarded when this fire is fully extinguished")]
    public int scoreValue = 10;

    // ---- Runtime State ----
    private float currentHP;
    private bool isExtinguished;
    private ParticleSystem[] fireParticles;
    private float[] originalEmissionRates;
    private Vector3[] originalScales;
    private FireBaseScript fireBase;
    private bool isBeingSprayed;

    /// <summary>True once the fire has been fully put out.</summary>
    public bool IsExtinguished => isExtinguished;

    /// <summary>Current HP as a 0-1 ratio of maxHP.</summary>
    public float HPRatio => Mathf.Clamp01(currentHP / maxHP);

    void Start()
    {
        currentHP = maxHP;
        isExtinguished = false;

        // Cache all child particle systems and their original values
        fireParticles = GetComponentsInChildren<ParticleSystem>();
        originalScales = new Vector3[fireParticles.Length];
        originalEmissionRates = new float[fireParticles.Length];

        for (int i = 0; i < fireParticles.Length; i++)
        {
            originalScales[i] = fireParticles[i].transform.localScale;
            originalEmissionRates[i] = fireParticles[i].emission.rateOverTimeMultiplier;
        }

        fireBase = GetComponent<FireBaseScript>();

        // Ensure collider is large enough for spray detection (mobile-friendly)
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            // Scale up collider so SphereCast can find it even at small fire scale
            if (box.size.x < 5f)
                box.size = new Vector3(6f, 6f, 6f);
            box.isTrigger = true;
        }

        // Register with GameManager for fire tracking
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterFire(this);
    }

    void Update()
    {
        if (isExtinguished) return;

        // Fire slowly regrows when NOT being sprayed — creates urgency
        if (!isBeingSprayed && currentHP < maxSpreadHP)
        {
            currentHP = Mathf.Min(currentHP + spreadRate * Time.deltaTime, maxSpreadHP);
        }

        // Reset spray flag each frame; ApplyWaterDamage re-sets it
        isBeingSprayed = false;

        UpdateVisuals();
    }

    /// <summary>
    /// Called every frame by the player's spray system when water hits this fire.
    /// Damage is already scaled by Time.deltaTime on the caller side.
    /// </summary>
    public void ApplyWaterDamage(float damage)
    {
        if (isExtinguished) return;

        isBeingSprayed = true;
        currentHP -= damage;
        currentHP = Mathf.Max(0f, currentHP);

        if (currentHP <= 0f)
        {
            Extinguish();
        }
    }

    /// <summary>
    /// Smoothly scale and fade fire particles based on remaining HP.
    /// </summary>
    private void UpdateVisuals()
    {
        float ratio = Mathf.Clamp01(currentHP / maxHP);

        for (int i = 0; i < fireParticles.Length; i++)
        {
            if (fireParticles[i] == null) continue;

            // Scale particles down as HP decreases
            fireParticles[i].transform.localScale =
                originalScales[i] * Mathf.Lerp(0.1f, 1f, ratio);

            // Reduce emission rate proportionally
            var emission = fireParticles[i].emission;
            emission.rateOverTimeMultiplier =
                originalEmissionRates[i] * Mathf.Lerp(0.05f, 1f, ratio);
        }
    }

    /// <summary>
    /// Fully extinguish the fire: award score, stop particles, clean up.
    /// </summary>
    private void Extinguish()
    {
        isExtinguished = true;
        currentHP = 0f;

        // Notify GameManager (combo scoring handled there)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnFireExtinguished(scoreValue);
        }

        // Stop all particle systems gracefully (let existing particles fade)
        foreach (var ps in fireParticles)
        {
            if (ps != null)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // Use PyroParticles' own graceful shutdown if available
        if (fireBase != null)
        {
            fireBase.Stop();
        }
        else
        {
            // Fallback: destroy after particles have faded
            Destroy(gameObject, 3f);
        }
    }
}
