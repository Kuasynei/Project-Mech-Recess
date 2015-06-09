using UnityEngine;
using System.Collections;

public class ZoneGeneration : MonoBehaviour {
    public float timeToActive = 0.5f;

    private float originalTime;
	// Use this for initialization
	void Start () {
        originalTime = timeToActive;
	}
	
	// Update is called once per frame
	void Update () {
        timeToActive -= Time.deltaTime;
        if (timeToActive < 0)
        {
            Generate();
            timeToActive = originalTime;
        }
	}

    void Generate()
    {
        GameObject obj = ZonePool.current.GetPooledObject();

        if (obj == null) return;

        obj.transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        obj.transform.rotation = transform.rotation;
        obj.SetActive(true);
    }
}
