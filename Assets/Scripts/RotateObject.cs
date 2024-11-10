using UnityEngine;

public class RotateObject : MonoBehaviour
{
    // Set the step rotation (90 degrees per press)
    public float rotationStep = 90f;

    // Current rotation angle
    private float currentRotation = 0f;

    // Maximum and minimum rotation limits (in degrees)
    private float maxRotation = 360f;
    private float minRotation = 0f;

    void Start()
    {
        // Initialize currentRotation to match the object's initial X rotation
        currentRotation = Mathf.Round(transform.eulerAngles.x);
    }

    // Method for Up button (rotate upwards by 90 degrees)
    public void UpRotation()
    {
        // Increase the rotation and clamp it to maxRotation
        currentRotation = (currentRotation + rotationStep) % maxRotation;
        ApplyRotation();
    }

    // Method for Down button (rotate downwards by 90 degrees)
    public void DownRotation()
    {
        // Decrease the rotation and wrap it if necessary
        currentRotation -= rotationStep;
        if (currentRotation < minRotation)
        {
            currentRotation = maxRotation - rotationStep; // Wrap around when going below 0
        }
        ApplyRotation();
    }

    // Apply the calculated rotation to the object
    private void ApplyRotation()
    {
        // Apply rotation around the X-axis
        transform.rotation = Quaternion.Euler(currentRotation, transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
