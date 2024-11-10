using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Switch : MonoBehaviour
{
    // Start is called before the first frame update
    public Image On;
    public Image Off;
    

    void Start()
    {
        // Set initial states (optional based on your requirements)
        On.gameObject.SetActive(true);
        Off.gameObject.SetActive(false);
        
    }

    // Update is called once per frame (optional, can remove if not needed)
    void Update()
    {
        // If you want to add real-time behavior based on 'index', use this.
        // Currently, the methods ON() and OFF() handle toggling, so this might be redundant.
    }

    public void ON()
    {
        
        Off.gameObject.SetActive(true);  // Display the 'Off' image
        On.gameObject.SetActive(false);  // Hide the 'On' image
    }

    public void OFF()
    {
        
        On.gameObject.SetActive(true);   // Display the 'On' image
        Off.gameObject.SetActive(false); // Hide the 'Off' image
    }
}
