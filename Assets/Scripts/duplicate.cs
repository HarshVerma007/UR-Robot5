using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SimpleJointController : MonoBehaviour
{
    public GameObject baseObject;
    public GameObject jointPrefab;
    public LineRenderer lineRenderer;
    public float lineExtensionStep = 0.1f;
    public Vector3 currentDirection = Vector3.forward;

    public TextMeshProUGUI rotationText;
    public TextMeshProUGUI lengthText;
    public Slider positionSlider;
    public Slider rotationSlider;

    [Header("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.up;  // Adjustable rotation axis in the Inspector

    private List<GameObject> joints = new List<GameObject>();
    private List<Vector3> initialPositions = new List<Vector3>();

    private float currentXRotation = 0f;
    private const float rotationStep = 90f;
    private float currentLength = 0f;
    private const float maxLength = 10f;
    private const float maxRotation = 360f;

    private GameObject selectedJoint;
    private Color originalColor;
    private int selectedJointIndex = -1;
    private float currentSliderValue = 0f;
    private float currentRotationSliderValue = 0f;
    public GameObject uiPanelPrefab;
    public Transform uiContainer;
    private List<GameObject> uiPanels = new List<GameObject>();
    public float rotationSpeedMultiplier = 0.1f; // Default multiplier to slow down rotation

    [Header("Rotation Control")]
    public TMP_InputField[] angleInputFields; // 6 input fields for each joint
    public TMP_InputField timeInputField;
    public Toggle coordinatedToggle;
    public Button doneButton;


    private void Start()
    {
        if (baseObject != null)
        {
            joints.Add(baseObject);
            initialPositions.Add(baseObject.transform.position);
            CreateUIPanelForJoint(baseObject);
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

            // Other initialization code...
        if (rotationSlider != null)
        {
            rotationSlider.value = 0;  // Set the default value to 0 (0 degrees).
            rotationSlider.onValueChanged.AddListener(OnRotationSliderChanged);
        }
        // Assign the Done button to trigger the rotation action
        if (doneButton != null)
        {
            doneButton.onClick.AddListener(OnDoneButtonPressed);
        }  
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (joints.Contains(hit.transform.gameObject))
                {
                    ToggleJointSelection(hit.transform.gameObject);
                }
            }
        }
    }

    private void ToggleJointSelection(GameObject joint)
    {
        if (selectedJoint == joint)
        {
            ResetSelectedJoint();
        }
        else
        {
            if (selectedJoint != null)
            {
                ResetSelectedJoint();
            }

            selectedJoint = joint;
            Renderer jointRenderer = joint.GetComponent<Renderer>();

            if (jointRenderer != null)
            {
                originalColor = jointRenderer.material.color;
                jointRenderer.material.color = Color.green;
            }

            selectedJointIndex = joints.IndexOf(selectedJoint);

            positionSlider.value = 0;
            rotationSlider.value = 0;
        }
    }

    private void ResetSelectedJoint()
    {
        if (selectedJoint != null)
        {
            Renderer jointRenderer = selectedJoint.GetComponent<Renderer>();
            if (jointRenderer != null)
            {
                jointRenderer.material.color = originalColor;
            }
            selectedJoint = null;
            selectedJointIndex = -1;
        }

        positionSlider.value = 0;
        rotationSlider.value = 0;
    }

    private void OnPositionSliderChanged(float value)
    {
        if (selectedJoint == null || selectedJointIndex < 0) return;

        float positionChange = value - currentSliderValue;
        currentSliderValue = value;

        for (int i = selectedJointIndex; i < joints.Count; i++)
        {
            joints[i].transform.position += new Vector3(0, positionChange, 0);
            initialPositions[i] = joints[i].transform.position;
        }

        UpdateLineRenderer();
    }

private void OnRotationSliderChanged(float value)
{
    if (selectedJoint == null || selectedJointIndex < 0) return;

    // Calculate rotation angle based on slider value to stay within 0-360 range
    float rotationAngle = Mathf.Repeat(value * maxRotation * rotationSpeedMultiplier, 360f);

    // Calculate the rotation based on the current slider value
    Quaternion rotation = Quaternion.Euler(rotationAxis * rotationAngle);

    // Apply the rotation to the selected joint
    selectedJoint.transform.rotation = rotation;

    // Update the positions and LineRenderer based on the current selection and rotation
    UpdateJointPositionsAndLineRenderer();

}

private void UpdateJointPositionsAndLineRenderer()
{
    if (selectedJoint == null) return;

    // Get the rotation of the selected joint
    Quaternion jointRotation = selectedJoint.transform.rotation;

    // Loop through each joint starting from the selected joint
    for (int i = selectedJointIndex + 1; i < joints.Count; i++)
    {
        // Calculate the new position based on the selected joint's rotation
        Vector3 previousJointPosition = joints[i - 1].transform.position;
        Vector3 relativePosition = joints[i].transform.position - previousJointPosition;

        // Apply the selected joint's rotation only to the forward joints
        Vector3 rotatedPosition = jointRotation * relativePosition + previousJointPosition;

        // Update the current joint's position
        joints[i].transform.position = rotatedPosition;

        // Update the LineRenderer position at the current joint
        lineRenderer.SetPosition(i, rotatedPosition);
    }

    // Keep LineRenderer length constant by ensuring the number of positions matches joints count
    AdjustLineRendererLength();
}

private void AdjustLineRendererLength()
{
    // Ensure the LineRenderer has the same count as joints to avoid shrinking
    lineRenderer.positionCount = joints.Count;

    // Update each position in the LineRenderer to match joint positions
    for (int i = 0; i < joints.Count; i++)
    {
        lineRenderer.SetPosition(i, joints[i].transform.position);
    }
}


private void UpdateLineRenderer()
{
    // Update the positions of all joints for the LineRenderer
    lineRenderer.positionCount = joints.Count;

    // Set the positions of the LineRenderer to match the current joint positions
    for (int i = 0; i < joints.Count; i++)
    {
        lineRenderer.SetPosition(i, joints[i].transform.position);
    }
}


    public void RotateJointX()
    {
        GameObject jointToRotate = selectedJoint ?? joints[joints.Count - 1];
        if (jointToRotate != null && joints.Count > 0)
        {
            currentXRotation = (currentXRotation + rotationStep) % maxRotation;
            jointToRotate.transform.rotation = Quaternion.Euler(currentXRotation, 0, 0);
            currentDirection = jointToRotate.transform.forward;

            UpdateRotationText();
            UpdateLineRenderer();
        }
    }

    public void ReverseRotateJointX()
    {
        GameObject jointToRotate = selectedJoint ?? joints[joints.Count - 1];
        if (jointToRotate != null && joints.Count > 0)
        {
            currentXRotation = (currentXRotation - rotationStep + maxRotation) % maxRotation;
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

    public void AddJoint()
    {
        if (jointPrefab == null)
        {
            Debug.LogError("Joint Prefab is not assigned!");
            return;
        }

        Vector3 lastPosition = joints[joints.Count - 1].transform.position;
        Vector3 newPosition = lastPosition + (currentDirection * lineExtensionStep);

        GameObject newJoint = Instantiate(jointPrefab, newPosition, Quaternion.identity);
        joints.Add(newJoint);
        initialPositions.Add(newPosition);
        CreateUIPanelForJoint(newJoint);

        UpdateLineRenderer();
    }

    public void IncreaseLineLength()
    {
        if (currentLength < maxLength)
        {
            GameObject jointToMove = joints[joints.Count - 1];
            Vector3 lastPosition = jointToMove.transform.position;
            Vector3 direction = (lastPosition - joints[joints.Count - 2].transform.position).normalized;
            jointToMove.transform.position += direction * lineExtensionStep;

            currentLength += lineExtensionStep;
            initialPositions[joints.Count - 1] = jointToMove.transform.position;
            UpdateLineRenderer();
            UpdateLengthText();
        }
    }

    public void DecreaseLineLength()
    {
        if (joints.Count > 1 && currentLength > 0)
        {
            GameObject jointToMove = joints[joints.Count - 1];
            Vector3 previousPosition = initialPositions[joints.Count - 1];
            jointToMove.transform.position = previousPosition;

            currentLength -= lineExtensionStep;
            UpdateLineRenderer();
            UpdateLengthText();
        }
    }

    private void UpdateLengthText()
    {
        if (lengthText != null)
        {
            lengthText.text = $"{currentLength:F1}";
        }
        else
        {
            Debug.LogError("Length Text UI is not assigned!");
        }
    }

    // private void UpdateLineRenderer()
    // {
    //     lineRenderer.positionCount = joints.Count;

    //     for (int i = 0; i < joints.Count; i++)
    //     {
    //         lineRenderer.SetPosition(i, joints[i].transform.position);
    //     }
    // }

    private void CreateUIPanelForJoint(GameObject joint)
    {
        if (uiPanelPrefab == null || uiContainer == null)
        {
            Debug.LogError("UI Panel Prefab or Container is not assigned!");
            return;
        }

        GameObject newUIPanel = Instantiate(uiPanelPrefab, uiContainer);
        uiPanels.Add(newUIPanel);

        TextMeshProUGUI jointLabel = newUIPanel.GetComponentInChildren<TextMeshProUGUI>();
        jointLabel.text = $"Joint {joints.Count}";
    }
    private void OnDoneButtonPressed()
    {
        float[] angles = new float[6];
        float rotationTime;

        // Read angle values for each joint from input fields
        for (int i = 0; i < Mathf.Min(angleInputFields.Length, joints.Count); i++)
        {
            if (float.TryParse(angleInputFields[i].text, out float angle))
            {
                angles[i] = angle;
            }
            else
            {
                angles[i] = 0f;
            }
        }

        // Read the rotation time from the input field
        if (!float.TryParse(timeInputField.text, out rotationTime) || rotationTime <= 0)
        {
            Debug.LogError("Invalid rotation time! Using default time of 1 second.");
            rotationTime = 1f;
        }

        // Start rotation based on the coordinated mode
        if (coordinatedToggle.isOn)
        {
            StartCoroutine(CoordinatedRotationRoutine(angles, rotationTime));
        }
        else
        {
            StartCoroutine(NonCoordinatedRotationRoutine(angles, rotationTime));
        }
    }

    private IEnumerator CoordinatedRotationRoutine(float[] angles, float time)
    {
        for (int i = 0; i < Mathf.Min(angles.Length, joints.Count); i++)
        {
            Quaternion targetRotation = Quaternion.Euler(rotationAxis * angles[i]);
            Quaternion initialRotation = joints[i].transform.rotation;
            float elapsedTime = 0f;

            while (elapsedTime < time)
            {
                joints[i].transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, elapsedTime / time);
                elapsedTime += Time.deltaTime;
                UpdateLineRenderer();
                yield return null;
            }

            joints[i].transform.rotation = targetRotation;
            UpdateLineRenderer();
        }
    }

    private IEnumerator NonCoordinatedRotationRoutine(float[] angles, float time)
    {
        float elapsedTime = 0f;
        Quaternion[] initialRotations = new Quaternion[joints.Count];
        Quaternion[] targetRotations = new Quaternion[joints.Count];

        // Set initial and target rotations for each joint
        for (int i = 0; i < Mathf.Min(angles.Length, joints.Count); i++)
        {
            initialRotations[i] = joints[i].transform.rotation;
            targetRotations[i] = Quaternion.Euler(rotationAxis * angles[i]);
        }

        // Rotate all joints simultaneously over the specified time
        while (elapsedTime < time)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                joints[i].transform.rotation = Quaternion.Lerp(initialRotations[i], targetRotations[i], elapsedTime / time);
            }

            elapsedTime += Time.deltaTime;
            UpdateLineRenderer();
            yield return null;
        }

        // Ensure all joints reach their target rotation
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].transform.rotation = targetRotations[i];
        }

        UpdateLineRenderer();
    }


}
