using UnityEngine;
using UnityEngine.UI; // Include this namespace if you're using UI elements

public class PetInteraction : MonoBehaviour
{
    private int touchCount = 0;
    private bool isBeingPetted = false;

    // Variables to keep track of the pet's love for the player
    [HideInInspector]
    public float loveValue = 0f; // Current love value
    public float loveIncreaseRate = 1f; // Love increase per second while petting
    public float loveDecreaseRate = 0.5f; // Love decrease per second when not petting (optional)
    public float maxLoveValue = 100f; // Maximum love value
    public float minLoveValue = 0f; // Minimum love value

    // Optional: Reference to a UI Text or Slider to display the love value
    public Text loveValueText; // Assign in the Inspector if using Text
    // public Slider loveValueSlider; // Assign in the Inspector if using Slider

    void Update()
    {
        // Update the love value based on whether the pet is being petted
        if (isBeingPetted)
        {
            loveValue += loveIncreaseRate * Time.deltaTime;
            if (loveValue > maxLoveValue)
                loveValue = maxLoveValue;
        }
        else
        {
            // Optionally decrease love value when not being petted
            loveValue -= loveDecreaseRate * Time.deltaTime;
            if (loveValue < minLoveValue)
                loveValue = minLoveValue;
        }

        // Update the love value display
        UpdateLoveDisplay();

        // Debug log the current love value
        Debug.Log("Pet's love value: " + loveValue);
    }

    // These methods are now public so they can be called from HandCollider
    public void OnPetTouched()
    {
        touchCount++;
        if (touchCount == 1)
        {
            isBeingPetted = true;
            // Pet reaction when petted
            Debug.Log("Pet is being petted!");

            // Optional: Add pet's reaction here (e.g., play animation, sound)
        }
    }

    public void OnPetReleased()
    {
        touchCount--;
        if (touchCount <= 0)
        {
            touchCount = 0;
            isBeingPetted = false;
            // Pet reaction when petting stops
            Debug.Log("Pet is no longer being petted.");

            // Optional: Add pet's reaction here (e.g., stop animation, sound)
        }
    }

    private void UpdateLoveDisplay()
    {
        // Update the UI Text if assigned
        if (loveValueText != null)
        {
            loveValueText.text = "Pet's Love: " + Mathf.RoundToInt(loveValue);
        }

        // Update the UI Slider if assigned
        // if (loveValueSlider != null)
        // {
        //     loveValueSlider.value = loveValue;
        // }
    }
}
