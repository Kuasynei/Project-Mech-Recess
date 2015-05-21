using UnityEngine;
using System.Collections;

public class gateScript : MonoBehaviour {
    // Time in seconds since set as un-triggered
    public float timeSince = 10.0f;

    // Reset value for timeSince
    private float beginTime;

	// Use this for initialization
	void Start () 
    {
        beginTime = timeSince;
	}
	
	// Update is called once per frame
	void Update () 
    {
        Debug.Log("Time since exit: " + timeSince);
        if (GetComponent<Collider>().isTrigger == false)
        {
            timeSince -= Time.deltaTime;

            if (timeSince <= 0.0f)
            {
                GetComponent<Collider>().isTrigger = true;

                timeSince = 0.0f;
            }
        }
	}

    // Calls setTriggered on exit of trigger
    void OnTriggerExit(Collider c)
    {
        if (c.tag == "Player")
        {
            setTriggered();
        }
    }

    // Sets collider not triggered
    void setTriggered()
    {
        if (GetComponent<Collider>().isTrigger == true)
        {
            GetComponent<Collider>().isTrigger = false;

            timeSince = beginTime;
        }
    }
}
