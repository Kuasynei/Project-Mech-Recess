using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZonePool : MonoBehaviour {
    public static ZonePool current;
    public GameObject pooledObj;
    public int pooledAmount = 2;
    public bool grow = false;

    List<GameObject> pooledObjs;

    void Awake()
    {
        current = this;
    }

    // Use this for initialization
    void Start()
    {
        pooledObjs = new List<GameObject>();
        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = Instantiate(pooledObj, new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), transform.rotation) as GameObject;
            obj.SetActive(false);
            pooledObjs.Add(obj);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjs.Count; i++)
        {
            if (!pooledObjs[i].activeInHierarchy)
            {
                return pooledObjs[i];
            }
        }

        if (grow)
        {
            GameObject obj = (GameObject)Instantiate(pooledObj, new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), transform.rotation);
            pooledObjs.Add(obj);

            return obj;
        }

        return null;
    }
}
