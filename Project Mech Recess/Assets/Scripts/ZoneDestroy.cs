using UnityEngine;
using System.Collections;

public class ZoneDestroy : MonoBehaviour {

    void Destroy()
    {
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void OnTriggerExit(Collider c)
    {
        if (c.tag == "Player")
        {
            Destroy();
        }
    }
}
