using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovment : MonoBehaviour
{
    public FixedJoystick joy;
    private float xinput, yinput;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
      rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        xinput = joy.Horizontal;
        yinput = joy.Vertical;
        rb.linearVelocity= new Vector3 (xinput, yinput, 0f);
    }
}
