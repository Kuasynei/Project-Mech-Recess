using UnityEngine;
using System.Collections;

public class ZonePenalty : MonoBehaviour {
    public float timeBeforePenalty = 30.0f;

    private float originalTime;
    private GameObject player;
    private mechBehaviour script;

	void Awake() 
    {
        originalTime = timeBeforePenalty;
	}

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        script = player.GetComponent<mechBehaviour>();
    }
	
	void Update() 
    {
        StartCountdown();

        if (timeBeforePenalty < 0.0f)
        {
            script.canBoost = false;

            ResetCountdown();
        }
	}

    void OnDisable()
    {
        ResetCountdown();
    }

    void StartCountdown()
    {
        timeBeforePenalty -= Time.deltaTime;
    }

    void ResetCountdown()
    {
        timeBeforePenalty = originalTime;
    }
}
