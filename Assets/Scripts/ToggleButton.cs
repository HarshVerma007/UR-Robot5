using UnityEngine;
using UnityEngine.UI; // For handling the Button component

public class ToggleObject : MonoBehaviour
{
    public GameObject targetObject; // The object to enable/disable
    public Button toggleButton;     // The button that will toggle the object's state

    private bool isObjectActive = true; // Keeps track of the current state

    void Start()
    {
        // Add a listener to the button to call ToggleObjectState when clicked
        toggleButton.onClick.AddListener(ToggleObjectState);

        // Initialize the state based on the object's active state
        isObjectActive = targetObject.activeSelf;
    }

    // Method to toggle the object's state
    void ToggleObjectState()
    {
        // Toggle the state
        isObjectActive = !isObjectActive;
        
        // Set the object's active state
        targetObject.SetActive(isObjectActive);
    }
}
