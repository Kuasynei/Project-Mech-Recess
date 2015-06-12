using UnityEngine;
using System.Collections;

public class ZonePenalty : MonoBehaviour {
    public float timeBeforePenalty = 30.0f;

    private float originalTime;
    private string penaltyWarning;
    private string lossOfBoost;
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
        lossOfBoost = "You lost your boost!!!";
    }
	
	void Update() 
    {
        penaltyWarning = "You will lose your boost in: " + (int)timeBeforePenalty + "s";
        StartCountdown();

        if (timeBeforePenalty < 11.0f)
        {
            Debug.LogWarning(penaltyWarning);
        }

        if (timeBeforePenalty < 0.0f)
        {
            script.canBoost = false;

            Debug.LogError(lossOfBoost);

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
