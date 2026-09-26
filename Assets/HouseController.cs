using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseController : MonoBehaviour
{

    // Reference to the fire prefab
    public GameObject firePrefab;

    private bool isOnFire = false;

    public bool IsOnFire()
    {
        return isOnFire;
    }

    public void IgniteFire()
    {
        isOnFire = true;
        // Add code to visually show the house on fire, e.g., play a fire particle effect.
    }

    public void ExtinguishFire()
    {
        isOnFire = false;
        // Add code to visually extinguish the fire, e.g., stop the fire particle effect.
    }
}


