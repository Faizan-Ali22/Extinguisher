using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 5f;
    public Joystick movementJoystick; // Reference to the Mobile Joystick Pack's Joystick component for movement
    public Button shootingButton; // Reference to the Unity UI Button component for shooting
    public Transform hoseNozzle; // Reference to the transform of the hose nozzle on your character
    public GameObject waterPrefab; // Reference to the water prefab
    public float shootingForce = 10f;
    public float shootingCooldown = 0.5f; // Adjust the cooldown time
    public int maxWaterInstances = 3; // Adjust the maximum number of active water instances

    public GameObject firePrefab;

    private float lastShootTime;
    private int currentWaterInstances = 0;
    private bool isShooting = false;

    void Update()
    {
        HandleMovement();
        /*HandleShooting();*/
    }

    void HandleMovement()
    {
        // Get input for character movement
        float horizontalInput = movementJoystick.Horizontal;
        float verticalInput = movementJoystick.Vertical;

        // Calculate movement vector
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput) * movementSpeed * Time.deltaTime;

        // Move the character
        transform.Translate(movement);
    }

    void HandleShooting()
    {
        // Check if the shooting button is pressed and cooldown is expired
        if (shootingButton != null && shootingButton.onClick != null && Time.time - lastShootTime > shootingCooldown)
        {
            isShooting = true;
            lastShootTime = Time.time;
        }
        else
        {
            isShooting = false;
        }

        // Shoot water continuously while the button is held down
        if (isShooting && currentWaterInstances < maxWaterInstances)
        {
            ShootWater();
        }
    }

    public void ShootWater()
    {
        
        // Check if the water prefab is assigned and the maximum instances limit is not reached
        if (waterPrefab != null && currentWaterInstances < maxWaterInstances)
        {
            // Instantiate water prefab at the nozzle position
            GameObject water = Instantiate(waterPrefab, hoseNozzle.position, hoseNozzle.rotation);

            // Apply force to the water
            Rigidbody waterRb = water.GetComponent<Rigidbody>();
            waterRb.AddForce(hoseNozzle.forward * shootingForce, ForceMode.Impulse);

            // Destroy the water after a certain duration (adjust as needed)
            Destroy(water, 2f);

        }
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Fire"))
        {
            Destroy(col.gameObject);
        }
    }

    
    public void waterButton()
    {
        HandleShooting();
    }
}
