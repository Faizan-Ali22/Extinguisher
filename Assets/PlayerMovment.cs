using UnityEngine;

/// <summary>
/// Alternative joystick movement using Rigidbody velocity.
/// Uses FixedUpdate for proper physics integration.
/// Note: If PlayerController is handling movement, this script
/// should be disabled to avoid conflicts.
/// </summary>
public class PlayerMovment : MonoBehaviour
{
    public FixedJoystick joy;
    public Rigidbody rb;
    public float moveSpeed = 5f;

    private float xinput, yinput;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    // Physics movement belongs in FixedUpdate, not Update
    void FixedUpdate()
    {
        if (joy == null || rb == null) return;

        xinput = joy.Horizontal;
        yinput = joy.Vertical;

        rb.linearVelocity = new Vector3(xinput * moveSpeed, rb.linearVelocity.y, yinput * moveSpeed);
    }
}
