using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles player movement (joystick + WASD) and water spray (hold button + Space).
/// Movement uses Rigidbody physics so walls and colliders properly block the player.
/// Water detection uses SphereCast for reliable, mobile-friendly fire targeting.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed = 5f;
    public Joystick movementJoystick;
    public float rotationSpeed = 12f;

    [Header("Water Spray — References")]
    public Button shootingButton;
    public Transform hoseNozzle;
    public GameObject waterPrefab;

    [Header("Water Spray — Tuning")]
    public float shootingForce = 10f;       // Kept for serialization compat
    public float shootingCooldown = 0.5f;   // Kept for serialization compat
    public int maxWaterInstances = 3;       // Kept for serialization compat

    public float sprayRange = 20f;
    public float sprayDamagePerSecond = 55f;
    public float sprayRadius = 2.5f;

    [Header("Water Visual Adjustment")]
    [Tooltip("Euler rotation offset for the water particle relative to the nozzle.")]
    public Vector3 waterRotationOffset = new Vector3(-90f, 0f, 0f);

    [Header("References")]
    public GameObject firePrefab; // Kept for scene serialization compatibility

    // ---- Runtime ----
    private bool isSpraying;
    private float lastStopTime;
    private ParticleSystem waterEffect;
    private Rigidbody rb;
    private Vector3 moveInput;
    private Transform hoseRoot; // Keep track of the hose to force it to follow

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("[PlayerController] No Rigidbody on player!");
            return;
        }

        // Configure Rigidbody for smooth ground movement
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        SetupWaterEffect();
        SetupSprayButton();
    }

    // ================================================================
    //  WATER EFFECT SETUP
    // ================================================================

    void SetupWaterEffect()
    {
        // 1. Ensure the hose nozzle is forcibly attached to the player root.
        // If it was parented to an animated mesh bone that isn't moving with the root,
        // it gets left behind. We parent it to the main player transform to be safe.
        if (hoseNozzle != null)
        {
            hoseRoot = hoseNozzle.root == hoseNozzle ? hoseNozzle : hoseNozzle.parent;
            // Reparent to the player directly to ensure it moves with the rigid body
            if (hoseNozzle.parent != this.transform)
            {
                // Try to bring the parent object (like GardenHose) if it's there
                Transform topHoseLevel = hoseNozzle;
                while (topHoseLevel.parent != null && topHoseLevel.parent != this.transform && topHoseLevel.parent.name.Contains("Hose"))
                {
                    topHoseLevel = topHoseLevel.parent;
                }
                topHoseLevel.SetParent(this.transform, true);
            }
            
            waterEffect = hoseNozzle.GetComponent<ParticleSystem>();
            if (waterEffect == null)
                waterEffect = hoseNozzle.GetComponentInChildren<ParticleSystem>();
        }

        // 2. Fallback if no particle system was found in the scene
        if (waterEffect == null && waterPrefab != null)
        {
            Transform parent = hoseNozzle != null ? hoseNozzle : transform;
            GameObject waterObj = Instantiate(waterPrefab, parent);
            waterObj.transform.localPosition = Vector3.zero;
            waterObj.transform.localRotation = Quaternion.Euler(waterRotationOffset);
            waterObj.transform.localScale = Vector3.one;

            Rigidbody waterRb = waterObj.GetComponent<Rigidbody>();
            if (waterRb != null) Destroy(waterRb);
            foreach (Collider col in waterObj.GetComponents<Collider>())
                Destroy(col);

            waterEffect = waterObj.GetComponent<ParticleSystem>();
        }

        // 3. Configure the particle system to NOT trail behind
        if (waterEffect != null)
        {
            var main = waterEffect.main;
            main.playOnAwake = false;
            
            // CRITICAL FIX: Change to Local space!
            // This forces the water stream to stay rigidly in front of the player
            // instead of leaving a trail of particles behind them as they move.
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            
            waterEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Debug.Log("[PlayerController] Water effect ready — using " + waterEffect.gameObject.name);
        }
    }

    // ================================================================
    //  SPRAY BUTTON SETUP
    // ================================================================

    void SetupSprayButton()
    {
        if (shootingButton == null) return;
        EventTrigger trigger = shootingButton.GetComponent<EventTrigger>();
        if (trigger == null) trigger = shootingButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener(_ => StartSpraying());
        trigger.triggers.Add(down);

        EventTrigger.Entry up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener(_ => StopSpraying());
        trigger.triggers.Add(up);
    }

    // ================================================================
    //  UPDATE
    // ================================================================

    void Update()
    {
        ReadInput();

        if (Input.GetKeyDown(KeyCode.Space)) StartSpraying();
        if (Input.GetKeyUp(KeyCode.Space)) StopSpraying();

        if (isSpraying) DetectAndDamageFire();
    }

    // ================================================================
    //  LATE UPDATE - Force Follow 
    // ================================================================
    
    void LateUpdate()
    {
        // Absolute fallback: If the hose still somehow gets left behind due to 
        // animation overrides, we forcibly snap it to the player's position.
        if (hoseNozzle != null && hoseNozzle.parent != this.transform)
        {
            hoseNozzle.position = transform.position + transform.forward + Vector3.up;
            hoseNozzle.rotation = transform.rotation * Quaternion.Euler(waterRotationOffset);
        }
    }

    void ReadInput()
    {
        float h = 0f, v = 0f;
        if (movementJoystick != null)
        {
            h = movementJoystick.Horizontal;
            v = movementJoystick.Vertical;
        }
        if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f)
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }
        moveInput = new Vector3(h, 0f, v);
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        Vector3 targetVel = moveInput * movementSpeed;
        rb.linearVelocity = new Vector3(targetVel.x, rb.linearVelocity.y, targetVel.z);

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void DetectAndDamageFire()
    {
        Vector3 origin = hoseNozzle != null ? hoseNozzle.position : transform.position;
        Vector3 dir = transform.forward; 

        RaycastHit[] hits = Physics.SphereCastAll(origin, sprayRadius, dir, sprayRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        float dmg = sprayDamagePerSecond * Time.deltaTime;

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.CompareTag("Fire")) continue;
            FireScript fire = hit.collider.GetComponent<FireScript>();
            if (fire != null && !fire.IsExtinguished) fire.ApplyWaterDamage(dmg);
        }
    }

    public void StartSpraying()
    {
        if (isSpraying) return;
        isSpraying = true;
        if (waterEffect != null) waterEffect.Play(true);
    }

    public void StopSpraying()
    {
        if (!isSpraying) return;
        isSpraying = false;
        lastStopTime = Time.time;
        if (waterEffect != null) waterEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void ShootWater()
    {
        if (Time.time - lastStopTime < 0.15f) return;
        if (!isSpraying)
        {
            StartSpraying();
            CancelInvoke(nameof(StopSpraying));
            Invoke(nameof(StopSpraying), 0.4f);
        }
    }

    public void waterButton() => ShootWater();
}
