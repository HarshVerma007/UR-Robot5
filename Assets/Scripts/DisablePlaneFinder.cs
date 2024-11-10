using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class DisablePlaneFinder : MonoBehaviour
{
    public PlaneFinderBehaviour planeFinder;  // Reference to the Vuforia PlaneFinderBehaviour
    public GameObject objectToPlace;          // The object that you want to place
    private bool objectPlaced = false;        // Flag to check if the object is placed

    void Start()
    {
        if (planeFinder == null)
        {
            planeFinder = FindObjectOfType<PlaneFinderBehaviour>();
        }

        // Add a callback to the event when an object is placed
        planeFinder.OnInteractiveHitTest.AddListener(OnObjectPlaced);
    }

    // This method will be called when the object is placed
    void OnObjectPlaced(HitTestResult result)
    {
        if (!objectPlaced)
        {
            // Place the object at the hit test result position
            objectToPlace.transform.position = result.Position;
            objectToPlace.SetActive(true);

            // Disable the Plane Finder to stop further detections
            planeFinder.gameObject.SetActive(false);

            // Set object placed flag to true
            objectPlaced = true;

            Debug.Log("Object placed and Plane Finder disabled.");
        }
    }
}
