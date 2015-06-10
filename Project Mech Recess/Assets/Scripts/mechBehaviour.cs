using UnityEngine;
using System.Collections;

public class mechBehaviour : MonoBehaviour {

    //BASE STAT VARIABLES (Edit these in the inspector)
	public float acceleration = 5f;
    public float topSpeed = 10f;
    public float jumpPower = 10f;
    public float turnSpeed = 10f;

    public float boostPower = 10f; 				//Power of the boost
    public float boostCooldown = 1f; 			//Max time before boost can be used again.
    public int boostNumber = 2; 				//Maximum number of boosts.

    public float reticleMaxDistance = 100f; 	//If the reticle doesn't hit any colliders.

    public bool controlsEnabled = true;
    public bool canBoost = true;
    public bool isChaser = false;

    //Private Stat Variables
    private float boostCooldown_Var = 0f;	 	//Player's current time available before boost is available.
    private int boostNumber_Var = 0;		 	//Player's current available boosts.
	
	private float landRecovery = 0.1f;			//Allow the system to register the player landing.
	private bool onGround = false;
	private bool onWall = false;
	private bool inWallRun = false;
	private float wallJumpTurnSpeed = 1f;		//Prevent player from jumping back onto a wall infinitely to climb it.
	
    private float isAttacking = 0f;

    //Input Variables
	private float hAxesInput;
	private float vAxesInput;
    private float jumpAxes;
    private float fire1Axes;
    private float fire2Axes;
    private bool shiftTechInput;

    //Public Components
    public GameObject mainCamera;
    public GameObject reticleObj;
    public GameObject boostLight;
    public GameObject windParticles;
    public GameObject empSphereParticles;
    public GameObject empBlastParticles;

    public GameObject playerModel;
    public GameObject attackHurtbox;
    public Material chaserMaterial;
    public Material runnerMaterial;

    //Private Components
	private Rigidbody RB;
    private GameObject speedWind;

	void Awake () {
		RB = GetComponent<Rigidbody>();
	}
	
	void Start () {
        reticleObj = Instantiate(reticleObj, transform.position, Quaternion.identity) as GameObject;

        speedWind = Instantiate(windParticles, transform.position, Quaternion.identity) as GameObject;
		speedWind.transform.parent = transform;

        windParticles = Instantiate(windParticles, transform.position, Quaternion.identity) as GameObject;
        windParticles.transform.rotation = Quaternion.Euler(-90, 0, 0);
		windParticles.transform.parent = transform;

        empSphereParticles = Instantiate(empSphereParticles, transform.position, Quaternion.identity) as GameObject;
        empSphereParticles.GetComponent<ParticleSystem>().Stop();
		empSphereParticles.transform.parent = transform;

        empBlastParticles = Instantiate(empBlastParticles, transform.position, Quaternion.identity) as GameObject;
        empBlastParticles.GetComponent<ParticleSystem>().Stop();
		empBlastParticles.transform.parent = transform;

        attackHurtbox = Instantiate(attackHurtbox, transform.position, Quaternion.Euler(90, 0, 0)) as GameObject;
		attackHurtbox.transform.parent = transform;
		attackHurtbox.GetComponent<CapsuleCollider> ().enabled = false;

		//Changing parent of the player model
		playerModel = Instantiate (playerModel, transform.position-transform.up*1.7f, transform.rotation) as GameObject;
		playerModel.transform.parent = transform;

        
	}

    void Update()
    {
        ////Cursor Relock
        if (Cursor.lockState != CursorLockMode.Locked && fire1Axes != 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        ////Input
        if (controlsEnabled)
        {
            hAxesInput = Input.GetAxis("Horizontal");
            vAxesInput = Input.GetAxis("Vertical");
            jumpAxes = Input.GetAxis("Jump");
            fire1Axes = Input.GetAxis("Fire1");
            fire2Axes = Input.GetAxis("Fire2");
            shiftTechInput = Input.GetKey(KeyCode.LeftShift);
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
			wallJumpTurnSpeed += Time.deltaTime/3;
		else
			wallJumpTurnSpeed = 1;
		
		////Horizontal Movement
		RB.AddForce(new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z) * (acceleration * Time.deltaTime * 100f)
		            * vAxesInput * wallDampener * wallJumpTurnSpeed * (boostCooldown_Var + 1));
		RB.AddForce(new Vector3(mainCamera.transform.right.x, 0, mainCamera.transform.right.z) * (acceleration * Time.deltaTime * 100f)
		            * hAxesInput * wallDampener * wallJumpTurnSpeed * (boostCooldown_Var + 1));
		
		////Rotate Towards Movement
		if (new Vector3(RB.velocity.normalized.x, 0, RB.velocity.normalized.z) != new Vector3(0,0,0))
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(new Vector3(RB.velocity.normalized.x, 0, RB.velocity.normalized.z)), 
			                                              Mathf.Clamp(RB.velocity.magnitude, 3f, 20f));
		}
		
		////Speed Limit
		{
			Vector3 tempXZ = new Vector3(RB.velocity.x, 0f, RB.velocity.z);
			Vector3 tempY = new Vector3(0f, RB.velocity.y, 0f);
			
			//Limit only horizontal speed if the player's boost not being used.
			if (tempXZ.magnitude > topSpeed && boostCooldown_Var <= boostCooldown - 0.5f)
			{
				RB.velocity = tempXZ.normalized * topSpeed + tempY;
			}
			else if (boostCooldown_Var > 0)//Hard Limit
			{
                RB.velocity = new Vector3(0, Mathf.Clamp(tempY.y, -12, 15), 0) + Vector3.ClampMagnitude(new Vector3(tempXZ.x, 0, tempXZ.z), 40f);
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

        ////Chaser Attack
        //If player presses the right mouse button, trigger attack.
        if (fire2Axes != 0 && isChaser && isAttacking <= 0 && isChaser)
        {
            attackHurtbox.GetComponent<CapsuleCollider>().enabled = true;
            empSphereParticles.GetComponent<ParticleSystem>().Play();
            empBlastParticles.GetComponent<ParticleSystem>().Play();
            if (RB.drag != 15)
                RB.drag = 15;
            isAttacking = 2f;
        }

        //True while attacking
        if (isAttacking > 0)
        {
            if (isAttacking > 0.5f)
                jumpAxes = 0;
            else
            {
                if (empSphereParticles.GetComponent<ParticleSystem>().isPlaying)
                    empSphereParticles.GetComponent<ParticleSystem>().Stop();

                if (empBlastParticles.GetComponent<ParticleSystem>().isPlaying)
                    empBlastParticles.GetComponent<ParticleSystem>().Stop();

                if (RB.drag != 0)
                    RB.drag = 0;

                if (attackHurtbox.GetComponent<CapsuleCollider>().enabled)
                    attackHurtbox.GetComponent <CapsuleCollider>().enabled = false;
            }

            isAttacking -= Time.deltaTime;
        }

		////Jumps
		//Ground Detection
		RaycastHit groundHit;
		if (Physics.Raycast(transform.position, -transform.up, out groundHit, transform.lossyScale.y * 1.09f))
		{
			onGround = true;
			onWall = false;
			inWallRun = false;
			wallJumpTurnSpeed = 1;
		}
		else
			onGround = false;

        Debug.DrawRay(transform.position, -transform.up * transform.lossyScale.y * 1.05f);

		//If on the ground, not recovering, and the jump buttons were pressed.
		if (jumpAxes != 0 && onGround && landRecovery <= 0f)
            RB.AddForce(0f, jumpPower * Time.deltaTime, 0f, ForceMode.VelocityChange);
		
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
			RB.AddForce(0f, 25f, 0f);
		}
		else if (RB.velocity.y < 0f)
			RB.AddForce(0f, -30f, 0f);
		
		//Perpetual Fast Fall
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

        Debug.DrawRay(transform.position, cameraRay.direction * 5f, Color.white);

		//If player presses fire1 and boost is off cooldown.
        if (canBoost)
        {
            if (fire1Axes != 0 && boostCooldown_Var <= 0 && boostNumber_Var > 0)
            {
                if (onGround)
                    RB.AddForce(cameraRay.direction * boostPower * (wallJumpTurnSpeed), ForceMode.Impulse);
                else
                    RB.AddForce(cameraRay.direction * boostPower * (wallJumpTurnSpeed) + transform.up * 7f, ForceMode.Impulse);
                boostCooldown_Var = boostCooldown;
                boostNumber_Var--;
            }
        }
    }

    void FixedUpdate()
    {
        ////Wind FX
        //Wind that appears below player to help with landing
        RaycastHit verticalAlignHit;
        if (Physics.Raycast(transform.position, -transform.up, out verticalAlignHit, 30f))
        {
            windParticles.transform.position = verticalAlignHit.point;
            if (!windParticles.GetComponent<ParticleSystem>().isPlaying)
            {
                windParticles.GetComponent<ParticleSystem>().Play();
            }
        }
        else
            windParticles.GetComponent<ParticleSystem>().Stop();

        //Wind to indicate exceeding regular top speed
        speedWind.transform.LookAt(RB.velocity + transform.position, transform.up);
        speedWind.GetComponent<ParticleSystem>().emissionRate = (RB.velocity.magnitude * 0.7f - (topSpeed + 5f)) * 20;

        //EMP attack particles
        {
            Vector3 rot = transform.rotation.eulerAngles;
            rot = new Vector3(rot.x, rot.y + 180, rot.z);
            empBlastParticles.transform.rotation = Quaternion.Euler(rot);
        }

        
    }

    //Handling Wall Collisions and Wall State
    void OnTriggerStay(Collider other)
    {
        if (isAttacking > 0.5 && isChaser)
        {
            if (other.transform.CompareTag("Player"))
            {
                other.SendMessage("Tagged", SendMessageOptions.DontRequireReceiver);
                playerModel.GetComponentInChildren<Renderer>().material = runnerMaterial;
                isChaser = false;
            }
            return;
        }


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
            if (RB.velocity.y < -0.8)
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

    void OnCollisionEnter(Collision coll)
    {
        //Allows the chaser to manually collide with runners.
        if (coll.transform.CompareTag("Player") && isChaser)
        {
            coll.transform.SendMessage("Tagged", SendMessageOptions.DontRequireReceiver);
            playerModel.GetComponentInChildren<Renderer>().material = runnerMaterial;
            isChaser = false;
        }
    }

    void Tagged()
    {
        if (!isChaser)
        {
            playerModel.GetComponentInChildren<Renderer>().material = chaserMaterial;
            StartCoroutine(TransitionToChaser());
        }
    }

    public void EnableControlsIfOwner()
    {
        if (GetComponent<NetworkView>().isMine)
        {
            controlsEnabled = true;
        }
    }

    IEnumerator TransitionToChaser()
    {
        controlsEnabled = false;
        hAxesInput = vAxesInput = jumpAxes = fire1Axes = fire2Axes = 0;
        shiftTechInput = false;

        yield return new WaitForSeconds(3);

        controlsEnabled = true;
        isChaser = true;
    }

}