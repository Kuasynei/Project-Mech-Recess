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
<<<<<<< HEAD
        //Debug.Log("Time since exit: " + timeSince);
=======
        Debug.Log("Time since exit: " + timeSince);
>>>>>>> origin/Wall-Stick-Fix-&-Wall-States
        if (GetComponent<Collider>().isTrigger == false)
        {
            timeSince -= Time.deltaTime;

            if (timeSince <= 0.0f)
            {
                GetComponent<Collider>().isTrigger = true;

                timeSince = 0.0f;
<<<<<<< HEAD

                GetComponentInChildren<MeshRenderer>().material.color = new Vector4(1f, 1f, 1f, 0.5f);
                
=======
>>>>>>> origin/Wall-Stick-Fix-&-Wall-States
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
<<<<<<< HEAD

            GetComponentInChildren<MeshRenderer>().material.color = new Vector4(1f, 1f, 1f, 0f);
=======
>>>>>>> origin/Wall-Stick-Fix-&-Wall-States
        }
    }
}
