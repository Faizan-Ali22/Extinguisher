using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles player movement (joystick) and water spray (hold button).
/// Water detection uses SphereCast for reliable, mobile-friendly fire targeting.
/// The water particle effect is purely visual; fire damage is applied via raycasting.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed = 5f;
    public Joystick movementJoystick;

    [Header("Water Spray — References")]
    public Button shootingButton;
    public Transform hoseNozzle;
    public GameObject waterPrefab;

    [Header("Water Spray — Tuning")]
    public float shootingForce = 10f;       // Kept for serialization compat
    public float shootingCooldown = 0.5f;   // Kept for serialization compat
    public int maxWaterInstances = 3;       // Kept for serialization compat

    [Tooltip("How far the water spray reaches")]
    public float sprayRange = 20f;

    [Tooltip("Damage dealt to fire per second while spraying")]
    public float sprayDamagePerSecond = 55f;

    [Tooltip("SphereCast radius — larger = more forgiving aim (good for mobile)")]
    public float sprayRadius = 2.5f;

    [Header("References")]
    public GameObject firePrefab; // Kept for scene serialization compatibility

    // ---- Runtime ----
    private bool isSpraying;
    private float lastStopTime;
    private ParticleSystem waterEffect;
    private Vector3 lastMoveDir = Vector3.forward;

    void Start()
    {
        SetupWaterEffect();
        SetupSprayButton();
    }

    /// <summary>
    /// Instantiate the water particle system once as a child of the nozzle.
    /// Strip physics components — we use raycasting, not collider overlap.
    /// </summary>
    void SetupWaterEffect()
    {
        if (waterPrefab == null || hoseNozzle == null) return;

        GameObject waterObj = Instantiate(waterPrefab, hoseNozzle);
        waterObj.transform.localPosition = Vector3.zero;
        waterObj.transform.localRotation = Quaternion.identity;

        // Remove physics components (not needed — spray is visual only)
        Rigidbody rb = waterObj.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        foreach (Collider col in waterObj.GetComponents<Collider>())
            Destroy(col);

        waterEffect = waterObj.GetComponent<ParticleSystem>();
        if (waterEffect != null)
            waterEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    /// <summary>
    /// Wire the spray button for hold-to-spray using PointerDown / PointerUp events.
    /// This gives much better feel than OnClick for a continuous spray mechanic.
    /// </summary>
    void SetupSprayButton()
    {
        if (shootingButton == null) return;

        EventTrigger trigger = shootingButton.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = shootingButton.gameObject.AddComponent<EventTrigger>();

        // Pointer Down → Start spraying
        EventTrigger.Entry down = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        down.callback.AddListener(_ => StartSpraying());
        trigger.triggers.Add(down);

        // Pointer Up → Stop spraying
        EventTrigger.Entry up = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        up.callback.AddListener(_ => StopSpraying());
        trigger.triggers.Add(up);
    }

    // ================================================================
    //  UPDATE LOOP
    // ================================================================

    void Update()
    {
        HandleMovement();

        if (isSpraying)
        {
            DetectAndDamageFire();
        }
    }

    // ================================================================
    //  MOVEMENT
    // ================================================================

    void HandleMovement()
    {
        if (movementJoystick == null) return;

        float h = movementJoystick.Horizontal;
        float v = movementJoystick.Vertical;

        Vector3 move = new Vector3(h, 0f, v) * movementSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // Rotate player to face movement direction (smooth)
        if (move.sqrMagnitude > 0.0001f)
        {
            lastMoveDir = move.normalized;
            Quaternion target = Quaternion.LookRotation(lastMoveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, target, 12f * Time.deltaTime);
        }
    }

    // ================================================================
    //  WATER SPRAY — FIRE DETECTION
    // ================================================================

    /// <summary>
    /// Casts a thick ray (SphereCast) from the hose nozzle forward.
    /// Any fire hit receives continuous water damage.
    /// QueryTriggerInteraction.Collide ensures we detect trigger colliders.
    /// </summary>
    void DetectAndDamageFire()
    {
        if (hoseNozzle == null) return;

        Vector3 origin = hoseNozzle.position;
        Vector3 dir = hoseNozzle.forward;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin, sprayRadius, dir, sprayRange,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide);

        float dmg = sprayDamagePerSecond * Time.deltaTime;

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.CompareTag("Fire")) continue;

            FireScript fire = hit.collider.GetComponent<FireScript>();
            if (fire != null && !fire.IsExtinguished)
            {
                fire.ApplyWaterDamage(dmg);
            }
        }

        // Editor debug line
        Debug.DrawRay(origin, dir * sprayRange, Color.cyan);
    }

    // ================================================================
    //  SPRAY START / STOP
    // ================================================================

    public void StartSpraying()
    {
        isSpraying = true;
        if (waterEffect != null)
            waterEffect.Play(true);
    }

    public void StopSpraying()
    {
        isSpraying = false;
        lastStopTime = Time.time;
        if (waterEffect != null)
            waterEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    /// <summary>
    /// Legacy method — the Button OnClick in the scene calls this.
    /// Fires a short burst so the existing wiring still works.
    /// </summary>
    public void ShootWater()
    {
        // Skip if StopSpraying just ran (EventTrigger PointerUp fires before OnClick)
        if (Time.time - lastStopTime < 0.15f) return;

        if (!isSpraying)
        {
            StartSpraying();
            CancelInvoke(nameof(StopSpraying));
            Invoke(nameof(StopSpraying), 0.4f);
        }
    }

    /// <summary>Legacy method kept for backward compatibility.</summary>
    public void waterButton()
    {
        ShootWater();
    }
}
