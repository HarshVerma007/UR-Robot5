using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DynamicRope : MonoBehaviour
{
    // AR/VR Functionality Fields
    public GameObject baseObject;
    public GameObject jointPrefab;
    public LineRenderer lineRenderer;
    public float initialDistance = 0f;
    public float lineExtensionStep = 0.1f;
    public Vector3 currentDirection = Vector3.forward;

    public TextMeshProUGUI rotationText;
    public TextMeshProUGUI lengthText;
    public Slider positionSlider;

    private List<GameObject> joints = new List<GameObject>();
    private List<Vector3> previousPositions = new List<Vector3>();
    private int jointCount = 0;

    private float currentXRotation = 0f;
    private const float maxRotation = 360f;
    private const float rotationStep = 90f;

    private GameObject selectedJoint;
    private float currentLength = 0f;
    private const float maxLength = 10f;
    private int displayLengthCounter = 0;

    private int initialJointCount;
    private int selectedJointIndex = -1;
    private float currentSliderValue = 0f;
    private List<Vector3> initialPositions = new List<Vector3>();

    [SerializeField] private GameObject uiElementPrefab;
    [SerializeField] private Transform panelContainer;
    private List<GameObject> jointUIs = new List<GameObject>();
    private List<float> originalYPositions = new List<float>();

    // New fields for trajectory motion
    public TMP_InputField[] jointAngleInputs;
    public TMP_InputField timeInput;
    public Toggle coordinatedMotionToggle;

    private List<float> targetAngles = new List<float>();
    private float animationDuration = 0f;
    private List<Quaternion> initialRotations = new List<Quaternion>(); // To store initial rotations for reset

   private void Start()
    {
        // Existing start logic
        if (baseObject != null)
        {
            joints.Add(baseObject);
            initialJointCount = joints.Count;
            initialPositions.Add(baseObject.transform.position);
            originalYPositions.Add(baseObject.transform.position.y);
            initialRotations.Add(baseObject.transform.rotation); // Store the initial rotation for reset
        }
        else
        {
            Debug.LogError("Base Object is not assigned!");
            return;
        }

        UpdateLineRenderer();
        UpdateRotationText();
        UpdateLengthText();

        if (positionSlider != null)
        {
            positionSlider.onValueChanged.AddListener(OnPositionSliderChanged);
        }

        currentSliderValue = positionSlider.value;
    }


    // Method to add a new UI element with all attached scripts and components
    private GameObject AddNewUIElement()
    {
        // Instantiate the new UI element as a child of the panel container
        GameObject newElement = Instantiate(uiElementPrefab, panelContainer);

        // Ensure the new element is set as a child of the panelContainer with local positioning preserved
        newElement.transform.SetParent(panelContainer, false);

        // Copy RectTransform properties from the original prefab to the new UI element
        RectTransform newElementRect = newElement.GetComponent<RectTransform>();
        RectTransform originalElementRect = uiElementPrefab.GetComponent<RectTransform>();

        newElementRect.anchorMin = originalElementRect.anchorMin;
        newElementRect.anchorMax = originalElementRect.anchorMax;
        newElementRect.pivot = originalElementRect.pivot;
        newElementRect.sizeDelta = originalElementRect.sizeDelta;
        newElementRect.localPosition = originalElementRect.localPosition; // Copy position
        newElementRect.localScale = originalElementRect.localScale; // Copy scale

        return newElement; // Return the newly created UI element with all scripts and components intact
    }

    // Modified AddJoint method to save the initial rotation for reset functionality
    public void AddJoint()
    {
        if (jointPrefab == null)
        {
            Debug.LogError("Joint Prefab is not assigned!");
            return;
        }

        Vector3 lastPosition = joints[joints.Count - 1].transform.position;
        Vector3 newPosition = lastPosition + (currentDirection * initialDistance);

        GameObject newJoint = Instantiate(jointPrefab, newPosition, Quaternion.identity);
        joints.Add(newJoint);
        initialPositions.Add(newPosition); // Add the initial position
        initialRotations.Add(newJoint.transform.rotation); // Add the initial rotation
        jointCount++;

        // Create a new UI element for the joint
        GameObject newJointUI = AddNewUIElement();
        jointUIs.Add(newJointUI);
        originalYPositions.Add(newPosition.y); // Store the initial Y position

        UpdateLineRenderer();
    }


    // Increase line length and update the display
    public void IncreaseLineLength()
    {
        GameObject jointToMove = selectedJoint ?? joints[joints.Count - 1];
        if (jointToMove != null && jointCount > 0)
        {
            Vector3 lastPosition = jointToMove.transform.position;
            previousPositions.Add(lastPosition);

            Vector3 direction = (lastPosition - joints[joints.Count - 2].transform.position).normalized;
            jointToMove.transform.position += direction * lineExtensionStep;

            currentLength += lineExtensionStep;
            currentLength = Mathf.Clamp(currentLength, 0, maxLength);
            displayLengthCounter = Mathf.Min(displayLengthCounter + 1, (int)maxLength);

            UpdateLineRenderer();
            UpdateLengthText();
        }
    }

    // Decrease line length and adjust the display
    public void DecreaseLineLength()
    {
        GameObject jointToMove = selectedJoint ?? joints[joints.Count - 1];
        if (jointToMove != null && previousPositions.Count > 0)
        {
            Vector3 previousPosition = previousPositions[previousPositions.Count - 1];
            jointToMove.transform.position = previousPosition;
            previousPositions.RemoveAt(previousPositions.Count - 1);

            currentLength -= lineExtensionStep;
            currentLength = Mathf.Clamp(currentLength, 0, maxLength);
            displayLengthCounter = Mathf.Max(displayLengthCounter - 1, 0);

            UpdateLineRenderer();
            UpdateLengthText();
        }
    }

    private void UpdateLengthText()
    {
        if (lengthText != null)
        {
            lengthText.text = displayLengthCounter.ToString();
        }
        else
        {
            Debug.LogError("Length Text UI is not assigned!");
        }
    }

    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = joints.Count;

        for (int i = 0; i < joints.Count; i++)
        {
            lineRenderer.SetPosition(i, joints[i].transform.position);
        }
    }

    // Rotate the joint around the x-axis (alpha or twist)
    public void RotateJointX()
    {
        GameObject jointToRotate = selectedJoint ?? joints[joints.Count - 1];
        if (jointToRotate != null && jointCount > 0)
        {
            currentXRotation = (currentXRotation + rotationStep) % maxRotation;
            jointToRotate.transform.rotation = Quaternion.Euler(currentXRotation, 0, 0);
            currentDirection = jointToRotate.transform.forward;

            UpdateRotationText();
            UpdateLineRenderer();
        }
    }

    // Reverse the rotation around the x-axis
    public void ReverseRotateJointX()
    {
        GameObject jointToRotate = selectedJoint ?? joints[joints.Count - 1];
        if (jointToRotate != null && jointCount > 0)
        {
            currentXRotation = (currentXRotation - rotationStep) % maxRotation;
            if (currentXRotation < 0) currentXRotation += maxRotation;
            jointToRotate.transform.rotation = Quaternion.Euler(currentXRotation, 0, 0);
            currentDirection = jointToRotate.transform.forward;

            UpdateRotationText();
            UpdateLineRenderer();
        }
    }

    private void UpdateRotationText()
    {
        if (rotationText != null)
        {
            rotationText.text = Mathf.FloorToInt(currentXRotation).ToString();
        }
        else
        {
            Debug.LogError("Rotation Text UI is not assigned!");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && joints.Contains(hit.transform.gameObject))
            {
                SelectJoint(hit.transform.gameObject);
            }
        }
    }

    private void OnPositionSliderChanged(float value)
{
    if (selectedJoint == null || selectedJointIndex < 0) return;

    // Calculate the movement offset based on the slider value and initial position
    float movementOffset = value - currentSliderValue;
    currentSliderValue = value; // Update current slider value

    // Move the selected joint based on the slider value offset
    Vector3 selectedJointPosition = joints[selectedJointIndex].transform.position;
    joints[selectedJointIndex].transform.position = new Vector3(
        selectedJointPosition.x,
        originalYPositions[selectedJointIndex] + value, // Offset by initial Y position
        selectedJointPosition.z
    );

    // Update the position of all subsequent joints relative to the selected joint
    for (int i = selectedJointIndex + 1; i < joints.Count; i++)
    {
        Vector3 jointPosition = joints[i].transform.position;
        joints[i].transform.position = new Vector3(
            jointPosition.x,
            originalYPositions[i] + value, // Use the initial Y position for each joint
            jointPosition.z
        );
    }

    // Update the line renderer to reflect the new positions
    UpdateLineRenderer();
    Debug.Log("Position Slider Value: " + value);
}

    
    // Select a joint by clicking and update its color
    private void SelectJoint(GameObject joint)
    {
        if (selectedJoint == joint)
        {
            selectedJoint.GetComponent<Renderer>().material.color = Color.white;
            selectedJoint = null;
            selectedJointIndex = -1;
        }
        else
        {
            if (selectedJoint != null)
            {
                selectedJoint.GetComponent<Renderer>().material.color = Color.white;
            }

            selectedJoint = joint;
            selectedJointIndex = joints.IndexOf(joint);

            selectedJoint.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    // New method to parse input values
    private void ParseInputValues()
    {
        targetAngles.Clear();
        foreach (TMP_InputField input in jointAngleInputs)
        {
            if (float.TryParse(input.text, out float angle))
            {
                targetAngles.Add(angle);
            }
            else
            {
                targetAngles.Add(0f); // Default to 0 if no input
            }
        }

        if (float.TryParse(timeInput.text, out float duration))
        {
            animationDuration = Mathf.Max(duration, 0.1f); // Minimum duration of 0.1s
        }
        else
        {
            animationDuration = 1f; // Default duration if no input
        }
    }

   // Method to start the trajectory animation
public void OnDoneButtonPressed()
{
    ParseInputValues();
    
    if (coordinatedMotionToggle.isOn)
    {
        StartCoroutine(CoordinatedMotionAnimation());
    }
    else
    {
        StartCoroutine(NonCoordinatedMotionAnimation());
    }
}

// Coordinated motion animation: all joints move simultaneously with arm-like motion
private IEnumerator CoordinatedMotionAnimation()
{
    float elapsedTime = 0f;

    // Store initial rotations and positions of each joint
    List<Quaternion> initialRotations = new List<Quaternion>();
    List<Vector3> initialPositions = new List<Vector3>();
    for (int i = 0; i < joints.Count; i++)
    {
        initialRotations.Add(joints[i].transform.rotation);
        initialPositions.Add(joints[i].transform.position);
    }

    while (elapsedTime < animationDuration)
    {
        float t = elapsedTime / animationDuration;
        
        // Calculate new positions based on cascading rotations
        for (int i = 0; i < joints.Count && i < targetAngles.Count; i++)
        {
            Quaternion targetRotation = Quaternion.Euler(targetAngles[i], 0, 0);
            joints[i].transform.rotation = Quaternion.Slerp(initialRotations[i], targetRotation, t);
            
            // Calculate the new position for each joint in the chain based on the previous joint's rotation
            if (i > 0)
            {
                Vector3 direction = joints[i - 1].transform.forward;
                joints[i].transform.position = joints[i - 1].transform.position + direction * initialDistance;
            }
        }

        elapsedTime += Time.deltaTime;
        UpdateLineRenderer();
        yield return null;
    }

    // Finalize rotation and position for all joints
    for (int i = 0; i < joints.Count && i < targetAngles.Count; i++)
    {
        joints[i].transform.rotation = Quaternion.Euler(targetAngles[i], 0, 0);
        
        if (i > 0)
        {
            Vector3 direction = joints[i - 1].transform.forward;
            joints[i].transform.position = joints[i - 1].transform.position + direction * initialDistance;
        }
    }
    UpdateLineRenderer();
}

// Non-coordinated motion animation: joints move in sequence with arm-like motion
private IEnumerator NonCoordinatedMotionAnimation()
{
    for (int i = 0; i < joints.Count && i < targetAngles.Count; i++)
    {
        Quaternion initialRotation = joints[i].transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(targetAngles[i], 0, 0);
        
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            joints[i].transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

            // Update positions of all joints in the chain based on the new rotation
            if (i > 0)
            {
                for (int j = i; j < joints.Count; j++)
                {
                    Vector3 direction = joints[j - 1].transform.forward;
                    joints[j].transform.position = joints[j - 1].transform.position + direction * initialDistance;
                }
            }

            elapsedTime += Time.deltaTime;
            UpdateLineRenderer();
            yield return null;
        }

        joints[i].transform.rotation = targetRotation; // Finalize rotation
        if (i > 0)
        {
            Vector3 direction = joints[i - 1].transform.forward;
            joints[i].transform.position = joints[i - 1].transform.position + direction * initialDistance;
        }
        UpdateLineRenderer();
    }
}


    // Method to reset trajectory values
    public void ResetTrajectory()
    {
        // Clear all joint angle input fields
        foreach (TMP_InputField inputField in jointAngleInputs)
        {
            inputField.text = ""; // Clear input field
        }

        timeInput.text = ""; // Clear time input field
        targetAngles.Clear(); // Clear target angles list

        // Reset all joints to their initial positions and rotations
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].transform.position = initialPositions[i];
            joints[i].transform.rotation = initialRotations[i];
        }

        UpdateLineRenderer(); // Update LineRenderer to reflect reset positions
        Debug.Log("Trajectory reset to initial state.");
    }
}
