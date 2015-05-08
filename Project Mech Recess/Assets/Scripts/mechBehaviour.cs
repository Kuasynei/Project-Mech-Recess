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

    //Private Stat Variables
    private float landRecovery = 0.2f;
    private float boostCooldown_Var = 0f;
    private int boostNumber_Var = 0;
    private float transparency = 1;
    private bool onGround = false;
    private bool onWall = false;
    private bool inWallRun = false;
    private float wallJumpTurnSpeed = 1f;

    //Input Variables
	private float hAxesInput;
	private float vAxesInput;
    private float jumpAxes;
    private float fire1Axes;
    private bool shiftTechInput;

    //Public Components
    public GameObject mainCamera;
    public GameObject reticleObj;
    public GameObject boostLight;
    public GameObject windSystem;

    //Private Components
	private Rigidbody RB;
    private GameObject speedWind;

	void Awake () {
		RB = GetComponent<Rigidbody>();

	}
	
	void Start () {
        reticleObj = Instantiate(reticleObj, transform.position, Quaternion.identity) as GameObject;
        speedWind = Instantiate(windSystem, transform.position, Quaternion.identity) as GameObject;
        windSystem = Instantiate(windSystem, transform.position, Quaternion.identity) as GameObject;
        windSystem.transform.rotation = Quaternion.Euler(-90, 0, 0);
	}

    void Update()
    {
        ////Input
        hAxesInput = Input.GetAxis("Horizontal");
        vAxesInput = Input.GetAxis("Vertical");
        jumpAxes = Input.GetAxis("Jump");
        fire1Axes = Input.GetAxis("Fire1");
        shiftTechInput = Input.GetKey(KeyCode.LeftShift);

        ////Cursor Relock
        if (Cursor.lockState != CursorLockMode.Locked && fire1Axes != 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        ////Wall Slide
        //Limiting horizontal movement when in a wall slide.
        float wallDampener = 1;
        if (onWall)
        {
            wallDampener = 0.6f;
            if (RB.velocity.y < -3.5f)
            {
                RB.velocity = new Vector3(RB.velocity.x, -3.5f, RB.velocity.z);
            }
        }

        //Wall jump turn limiter
        if (wallJumpTurnSpeed < 1)
            wallJumpTurnSpeed += Time.deltaTime/2;
        else
            wallJumpTurnSpeed = 1;

        ////Horizontal Movement
        RB.AddForce(new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z) * acceleration * vAxesInput * wallDampener * wallJumpTurnSpeed * (boostCooldown_Var + 1));
        RB.AddForce(new Vector3(mainCamera.transform.right.x, 0, mainCamera.transform.right.z) * acceleration * hAxesInput * wallDampener * wallJumpTurnSpeed * (boostCooldown_Var + 1));

        ////Speed Limit
        {
            Vector3 tempXZ = new Vector3(RB.velocity.x, 0f, RB.velocity.z);
            Vector3 tempY = new Vector3(0f, RB.velocity.y, 0f);

            //Limit only horizontal speed if the player's boost not being used.
            if (tempXZ.magnitude > topSpeed && boostCooldown_Var <= boostCooldown - 0.5f)
            {
                RB.velocity = tempXZ.normalized * topSpeed + tempY;
            }
            else if (boostCooldown_Var > 0)
            {
                RB.velocity = new Vector3(Mathf.Clamp(tempXZ.x, -20, 20), Mathf.Clamp(tempY.y, -20, 20), Mathf.Clamp(tempXZ.z, -20, 20));
                Debug.Log(tempY);
            }

        }

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

        ////Jumps
        //Ground Detection
        RaycastHit groundHit;
        if (Physics.Raycast(transform.position, -transform.up, out groundHit, transform.lossyScale.y + 0.4f))
        {
            onGround = true;
            onWall = false;
            inWallRun = false;
            wallJumpTurnSpeed = 1;
        }
        else
            onGround = false;

        //If on the ground, not recovering, and the jump buttons were pressed.
        if (jumpAxes != 0 && onGround && landRecovery <= 0f)
            RB.AddForce(0f, jumpPower * Time.deltaTime, 0f, ForceMode.Impulse);
        
        //Wall Jump is in OnTriggerStay

        //If not on the ground or on a wall increase landing recovery to 0.2, if on the ground or a wall decrease landing recovery over time.
        if (!onGround && !onWall)
            landRecovery = 0.1f;
        else
        {
            landRecovery -= Time.deltaTime;
            boostNumber_Var = boostNumber; //Available boosts refill to max
        }

        ////Glide & Fast Fall
        if (RB.velocity.y < -0.2f && (vAxesInput != 0 || hAxesInput != 0))
        {
            RB.AddForce(0f, 10f, 0f);
        }
        else if (RB.velocity.y < 0f)
            RB.AddForce(0f, -20f, 0f);


        ////Click Boost
        //If boost is on cooldown
        if (boostCooldown_Var > 0f)
            boostCooldown_Var -= Time.deltaTime;

        if (boostNumber_Var > 0)
            boostLight.GetComponent<Light>().color = new Color(1f, 0.3f, 0f);

        if (boostNumber_Var == 0)
            boostLight.GetComponent<Light>().color = new Color(1f, 0.1f, 0f);

        //If boost is currently being performed.
        if (boostCooldown_Var > boostCooldown - 0.5f)
        {
            GameObject tempLight = Instantiate(boostLight, transform.position - transform.forward, Quaternion.identity) as GameObject;
            Destroy(tempLight, 0.2f);
        }

        //If player presses fire1 and boost is off cooldown.
        if (fire1Axes != 0 && boostCooldown_Var <= 0 && boostNumber_Var > 0)
        {
            RB.AddForce(cameraRay.direction * boostPower * (wallJumpTurnSpeed) + transform.up * 1.2f, ForceMode.Impulse);
            boostCooldown_Var = boostCooldown;
            boostNumber_Var--;
        }

        ////Player Transparency
        if (Vector3.Distance(transform.position, mainCamera.transform.position) <= 3f && transparency > 0)
            transparency -= Time.deltaTime * 2;
        else if (transparency < 1)
            transparency += Time.deltaTime * 2;

        GetComponent<MeshRenderer>().material.color = new Vector4(1f, 1f, 1f, transparency);

        ////Wind FX
        //Wind that appears below player to help with landing
        RaycastHit verticalAlignHit;
        if (Physics.Raycast(transform.position, -transform.up, out verticalAlignHit, 50f))
            windSystem.transform.position = verticalAlignHit.point;

        //Wind to indicate exceeding regular top speed
        speedWind.transform.position = transform.position;
        speedWind.transform.LookAt(RB.velocity+transform.position, transform.up);
        speedWind.GetComponent<ParticleSystem>().emissionRate = (RB.velocity.magnitude - (topSpeed+5f))*20;
    }

    //Handling Wall Collisions and Wall State
    void OnTriggerStay(Collider other)
    {
        //If player is touching wall and not on the ground.
        if (onWall == false && other.tag == "Slide Enabled Wall" && !onGround)
        {
            onWall = true;
        }

        //If player presses jump, is on the wall, and is not recovering from a jump.
        if (jumpAxes != 0 && onWall && landRecovery <= 0f)
        {
            RB.AddForce(other.transform.forward*100f + transform.up*Mathf.Clamp(30 - RB.velocity.y*8f, 5, 100), ForceMode.Impulse);
            wallJumpTurnSpeed = 0;
            landRecovery = 0.1f;
        }

        //If player is holding Shift while on a wall begin checking for wall run.
        if (shiftTechInput)
            inWallRun = true;
        else
            inWallRun = false;

        //Check if on wall, and check if player is moving.
        if (inWallRun && onWall && (Mathf.Abs(RB.velocity.x) > 1|| Mathf.Abs(RB.velocity.z) > 1))
        {
            //Slow their fall speed
            if (RB.velocity.y < -0.2)
            {
                RB.AddForce(transform.up * 80);
            }

            //Prevent the player from stopping while in a wall run by checking if their speed is less than topspeed+2f
            if (Mathf.Abs(RB.velocity.x) < topSpeed+2f || Mathf.Abs(RB.velocity.z) < topSpeed+2f)
            {
                //Apply force if below top speed
                RB.velocity = new Vector3(RB.velocity.x*1.1f, RB.velocity.y, RB.velocity.z*1.1f);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        //If player leaves wall
        if (other.tag == "Slide Enabled Wall")
        {
            onWall = false;
            inWallRun = false;
        }
            
    }

}
