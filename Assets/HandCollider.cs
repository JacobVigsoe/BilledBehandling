using UnityEngine;

public class HandCollider : MonoBehaviour
{
    public GameObject petObject; // Reference to the pet GameObject

    private PetInteraction petInteraction;

    void Start()
    {
        if (petObject != null)
        {
            petInteraction = petObject.GetComponent<PetInteraction>();
            if (petInteraction == null)
            {
                Debug.LogError("PetInteraction script not found on the pet object.");
            }
        }
        else
        {
            Debug.LogError("Pet object not assigned in HandCollider script.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == petObject)
        {
            // The hand collider has entered the pet's collider
            petInteraction.OnPetTouched();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == petObject)
        {
            // The hand collider has exited the pet's collider
            petInteraction.OnPetReleased();
        }
    }
}
