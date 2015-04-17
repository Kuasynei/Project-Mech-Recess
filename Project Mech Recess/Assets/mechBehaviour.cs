using UnityEngine;
using System.Collections;

public class mechBehaviour : MonoBehaviour {

    //Public Variables
	public float acceleration = 5f;
    public float topSpeed = 10f;
    public float jumpPower = 10f;
    public float boostPower = 10f;
    public float boostAllowed = 0f;
    public float reticleMaxDistance;

    //Private Variables
    private bool onGround = false;
    private float landRecovery = 0.2f;

    //Input Variables
	private float hAxesInput;
	private float vAxesInput;
    private float jumpAxes;
    private float fire1Axes;

    //Public Components
    public GameObject mainCamera;
    public GameObject reticleObj;
    public GameObject dangerLight;

    //Private Components
	private Rigidbody RB;

	void Awake () {
		RB = GetComponent<Rigidbody>();

	}
	
	void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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

            if (tempXZ.magnitude > topSpeed && boostAllowed > -0.5f)
                RB.velocity = tempXZ.normalized*topSpeed + tempY;
        }
        //\\//\\//\\

        ////Reticle
        //Getting the ray from the center of the camera, forwards.
        Ray cameraRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit cameraRayHit;
        Vector3 defaultPoint = mainCamera.transform.position + mainCamera.transform.forward * reticleMaxDistance;

        //Firing a new ray from the player, to the first point of collision from the camera ray.
        Ray playerRay; 
        if (Physics.Raycast(cameraRay, out cameraRayHit, reticleMaxDistance)) //Determine whether raycast hits terrain or not.
            playerRay = new Ray(transform.position, (transform.position - cameraRayHit.point).normalized * -reticleMaxDistance);
        else
            playerRay = new Ray(transform.position, (transform.position - defaultPoint).normalized * -reticleMaxDistance);

        //If player ray hits terrain move the reticle to the point of collision. If it hits nothing move it to default point.
        RaycastHit playerRayHit;
        if (Physics.Raycast(playerRay, out playerRayHit, reticleMaxDistance))
            reticleObj.transform.position = playerRayHit.point;
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
        if (!onGround) landRecovery = 0.2f;
        else           landRecovery -= Time.deltaTime;
        //\\//\\//\\

        ////Click Boost
        //If boost is on cooldown
        if (boostAllowed < 0f)
        {
            boostAllowed += Time.deltaTime;
            GameObject tempLight = Instantiate(dangerLight, transform.position, Quaternion.identity) as GameObject;
            Destroy(tempLight, 0.05f);
        }

        //If player presses fire1 and boost is off cooldown.
        if (fire1Axes != 0 && boostAllowed >= 0)
        {
            RB.AddForce(playerRay.direction * boostPower, ForceMode.Impulse);
            boostAllowed = -1f;
        }

        //\\//\\//\\
    }
}
