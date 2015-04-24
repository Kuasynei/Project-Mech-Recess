using UnityEngine;
using System.Collections;

public class mechBehaviour : MonoBehaviour {

    //BASE STAT VARIABLES (Edit these in the inspector)
	public float acceleration = 5f;
    public float topSpeed = 10f;
    public float jumpPower = 10f;
    public float boostPower = 10f;
    public float boostCooldown = 1f;
    public int boostNumber = 2;
    public float reticleMaxDistance = 100f;
    public bool onGround = false;

    //Private Stat Variables
    private float landRecovery = 0.2f;
    private float boostCooldown_Var = 0f;
    private int boostNumber_Var = 0;
    private float transparency = 1;

    //Input Variables
	private float hAxesInput;
	private float vAxesInput;
    private float jumpAxes;
    private float fire1Axes;

    //Public Components
    public GameObject mainCamera;
    public GameObject reticleObj;
    public GameObject boostLight;

    //Private Components
	private Rigidbody RB;

	void Awake () {
		RB = GetComponent<Rigidbody>();

	}
	
	void Start () {
        reticleObj = Instantiate(reticleObj, transform.position, Quaternion.identity) as GameObject;
	}

	void Update()
    {
        ////Input
        hAxesInput = Input.GetAxis("Horizontal");
        vAxesInput = Input.GetAxis("Vertical");
        jumpAxes = Input.GetAxis("Jump");
        fire1Axes = Input.GetAxis("Fire1");
        //\\//\\//\\

        ////Horizontal Movement
        RB.AddForce(new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z) * acceleration * vAxesInput);
        RB.AddForce(new Vector3(mainCamera.transform.right.x, 0, mainCamera.transform.right.z) * acceleration * hAxesInput);
        //\\//\\//\\

        ////Speed Limit
        {
            Vector3 tempXZ = new Vector3(RB.velocity.x, 0f, RB.velocity.z);
            Vector3 tempY = new Vector3(0f, RB.velocity.y, 0f);

            if (tempXZ.magnitude > topSpeed && boostCooldown_Var > -boostCooldown + 0.5f)
            {
                RB.velocity = tempXZ.normalized * topSpeed + tempY;
            }
                
                
        }
        //\\//\\//\\

        ////Reticle
        //Getting the ray from the center of the camera, forwards.
        Vector3 defaultPoint = mainCamera.transform.position + mainCamera.transform.forward * reticleMaxDistance;

        Ray cameraRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit[] cameraRayHit = Physics.RaycastAll(cameraRay, reticleMaxDistance);

        //Firing a new ray from the player, to the first point of collision from the camera ray.
        Ray playerRay = new Ray();
        RaycastHit[] playerRayHit;

        float nearest = Mathf.Infinity;
        if (cameraRayHit.Length > 0)
        {
            foreach (RaycastHit cameraHitInfo in cameraRayHit)
            {
                if (cameraHitInfo.distance < nearest)
                {
                    nearest = cameraHitInfo.distance;
                    if (cameraHitInfo.collider.tag != "Player")
                        playerRay = new Ray(transform.position, (transform.position - cameraHitInfo.point).normalized * -reticleMaxDistance);
                }
            }
        }
        else
            playerRay = new Ray(transform.position, (transform.position - defaultPoint).normalized * -reticleMaxDistance);

        playerRayHit = Physics.RaycastAll(playerRay, reticleMaxDistance);

        //If player ray hits terrain move the reticle to the point of collision. If it hits nothing move it to default point.
        nearest = Mathf.Infinity;
        if (playerRayHit.Length > 0)
        {
            foreach (RaycastHit playerRayHitInfo in playerRayHit)
            {
                if (playerRayHitInfo.distance < nearest)
                {
                    nearest = playerRayHitInfo.distance;
                    if (playerRayHitInfo.collider.tag != "Player")
                        reticleObj.transform.position = playerRayHitInfo.point;
                }
            }
        }
        else
            reticleObj.transform.position = defaultPoint;
        

        //\\//\\//\\

        ////Jump
        //Ground Detection
        RaycastHit groundHit;
        if (Physics.Raycast(transform.position, transform.up * -1, out groundHit, transform.lossyScale.y + 0.4f))
            onGround = true;
        else
            onGround = false;

        //If on the ground, not recovering, and the jump buttons were pressed.
        if (jumpAxes != 0 && onGround && landRecovery <= 0f)
            RB.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);

        //If not on the ground increase landing recovery to 0.2, if on the ground decrease landing recovery over time.
        if (!onGround) 
            landRecovery = 0.2f;
        else
        {
            landRecovery -= Time.deltaTime;
            boostNumber_Var = boostNumber;
        }
        //\\//\\//\\

        ////Click Boost
        //If boost is on cooldown
        if (boostCooldown_Var < 0f)
            boostCooldown_Var += Time.deltaTime;

        if (boostNumber_Var > 0)
            boostLight.GetComponent<Light>().color = new Color(1f, 0.3f, 0f);

        if (boostNumber_Var == 0)
            boostLight.GetComponent<Light>().color = new Color(1f, 0.1f, 0f);

        if (boostCooldown_Var < -boostCooldown+0.5f)
        {
            GameObject tempLight = Instantiate(boostLight, transform.position - transform.forward, Quaternion.identity) as GameObject;
            Destroy(tempLight, 0.2f);
        }

        //If player presses fire1 and boost is off cooldown.
        if (fire1Axes != 0 && boostCooldown_Var >= 0 && boostNumber_Var > 0)
        {
            RB.AddForce(transform.up + new Ray(transform.position, (cameraRay.direction * reticleMaxDistance) - transform.position).direction * boostPower, ForceMode.Impulse);
            boostCooldown_Var = -boostCooldown;
            boostNumber_Var--;
        }
        //\\//\\//\\

        ////Player Transparency
        if (Vector3.Distance(transform.position, mainCamera.transform.position) <= 3f && transparency > 0)
            transparency -= Time.deltaTime*2;
        else if (transparency < 1)
            transparency += Time.deltaTime*2;

        GetComponent<MeshRenderer>().material.color = new Vector4(1f, 1f, 1f, transparency);
        //\\//\\//\\
    }
}
