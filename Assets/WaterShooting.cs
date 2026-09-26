using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterShooting : MonoBehaviour
{
    public ParticleSystem waterStream;
    public float shootingForce = 10f;
    public int scoreValue = 10;  // Set the score value for hitting a house on fire

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))  // Change "Fire1" to the input you want (e.g., left mouse button)
        {
            ShootWater();
        }
    }

    void ShootWater()
    {
        // Play the water particle system
        waterStream.Play();

        // Create a ray from the camera to the screen point where the user clicked
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            // Instantiate the water prefab at the click position
            GameObject water = Instantiate(waterStream.gameObject, hit.point, Quaternion.identity);

            // Access the Rigidbody component of the water prefab
            Rigidbody waterRb = water.GetComponent<Rigidbody>();

            // Apply force to simulate the water shooting
            waterRb.AddForce(ray.direction * shootingForce, ForceMode.Impulse);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is a house
        if (collision.gameObject.CompareTag("House"))
        {
            // Check if the house has a HouseController script
            HouseController houseController = collision.gameObject.GetComponent<HouseController>();

            if (houseController != null)
            {
                // Check if the house is on fire
                if (houseController.IsOnFire())
                {
                    Debug.Log("House is on fire. Extinguishing...");

                    // Extinguish the fire
                    houseController.ExtinguishFire();

                    // Increase the score using the AddScore method in GameManager
                    GameManager.Instance.AddScore(scoreValue);
                }
                else
                {
                    Debug.Log("House is not on fire.");
                }
            }
            else
            {
                Debug.Log("HouseController not found.");
            }
        }

        // Destroy the water regardless of the collision
        Destroy(gameObject);
    }
}
