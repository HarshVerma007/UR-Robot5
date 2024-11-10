using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class DynamicURRobotController : MonoBehaviour
{
    public List<GameObject> joints; // Automatically populate this with dynamic joints
    public TMP_InputField[] angleInputs; // Set references in inspector or dynamically link if they’re created at runtime
    public TMP_InputField timeInput;
    public Button evaluateButton;
    public bool coordinatedMotion;

    private void Start()
    {
        // Assign button listener
        evaluateButton.onClick.AddListener(StartMotion);
        
        // Find all cloned joints and add them to the list (adjust tag if needed)
        joints = new List<GameObject>(GameObject.FindGameObjectsWithTag("CloneJoint"));
    }

    public void StartMotion()
    {
        float time = float.Parse(timeInput.text);
        List<float> targetAngles = new List<float>();
        
        // Parse angle inputs for each joint
        for (int i = 0; i < angleInputs.Length; i++)
        {
            targetAngles.Add(float.Parse(angleInputs[i].text));
        }

        // Start motion coroutine with specified angles and time
        StartCoroutine(RotateJoints(targetAngles, time));
    }

    private IEnumerator RotateJoints(List<float> targetAngles, float duration)
    {
        float elapsedTime = 0f;
        List<Quaternion> initialRotations = new List<Quaternion>();

        // Store initial rotations for each joint
        foreach (var joint in joints)
        {
            initialRotations.Add(joint.transform.rotation);
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Interpolate each joint's rotation
            for (int i = 0; i < joints.Count; i++)
            {
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngles[i]);
                joints[i].transform.rotation = Quaternion.Slerp(initialRotations[i], targetRotation, t);
            }

            yield return null;
        }

        // Ensure all joints reach exact target angles at the end
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].transform.rotation = Quaternion.Euler(0, 0, targetAngles[i]);
        }
    }
}
