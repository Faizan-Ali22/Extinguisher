using UnityEngine;

/// <summary>
/// Tracks the fire state for a house.
/// When its associated fire is extinguished, the house is marked "saved".
/// Provides hooks for visual feedback (material swap, particle burst, etc.)
/// </summary>
public class HouseController : MonoBehaviour
{
    [Tooltip("Reference to the fire GameObject associated with this house")]
    public GameObject firePrefab;

    private FireScript fireScript;
    private bool wasSaved;

    /// <summary>True once this house's fire has been fully extinguished.</summary>
    public bool IsSaved => wasSaved;

    void Start()
    {
        // Try to find FireScript on the referenced fire object
        if (firePrefab != null)
        {
            fireScript = firePrefab.GetComponent<FireScript>();
        }

        // Fallback: check children of this house
        if (fireScript == null)
        {
            fireScript = GetComponentInChildren<FireScript>();
        }
    }

    void Update()
    {
        // Detect the moment fire is extinguished
        if (fireScript != null && !wasSaved && fireScript.IsExtinguished)
        {
            wasSaved = true;
            OnHouseSaved();
        }
    }

    /// <summary>
    /// Called once when the house's fire is put out.
    /// Override or extend this for visual/audio feedback.
    /// </summary>
    void OnHouseSaved()
    {
        Debug.Log($"<color=green>{gameObject.name} has been saved!</color>");

        // TODO: Add your visual feedback here, for example:
        // - Change house material to a "saved" color
        // - Play a celebration particle effect
        // - Show a floating "SAVED!" text
    }

    /// <summary>Returns true if this house currently has an active fire.</summary>
    public bool IsOnFire()
    {
        return fireScript != null && !fireScript.IsExtinguished;
    }
    /// <summary>Extinguishes the fire immediately.</summary>
    public void ExtinguishFire()
    {
        if (fireScript != null && !fireScript.IsExtinguished)
        {
            fireScript.ApplyWaterDamage(fireScript.maxHP);
        }
    }
}