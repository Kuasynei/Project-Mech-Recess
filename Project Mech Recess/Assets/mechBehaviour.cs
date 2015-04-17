using UnityEngine;
using System.Collections;

public class mechBehaviour : MonoBehaviour {

	public float acceleration = 5f;
    public float topSpeed = 10f;
    public float jumpPower = 10f;
    public float reticleMaxDistance;
    private bool onGround = false;
    private float landRecovery = 0.2f;

	private float hAxesInput;
	private float vAxesInput;
    private float jumpAxes;

    public GameObject mainCamera;
    public GameObject reticleObj;

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
        //\\//\\//\\

        ////Horizontal Movement
        RB.AddForce(new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z) * acceleration * vAxesInput);
        RB.AddForce(new Vector3(mainCamera.transform.right.x, 0, mainCamera.transform.right.z) * acceleration * hAxesInput);
        //\\//\\//\\

        ////Speed Limit
        {
            Vector3 tempXZ = new Vector3(RB.velocity.x, 0f, RB.velocity.z);
            Vector3 tempY = new Vector3(0f, RB.velocity.y, 0f);

            if (tempXZ.magnitude > topSpeed)
                RB.velocity = tempXZ.normalized*topSpeed + tempY;
        }
        //\\//\\//\\

        ////Reticle
        RaycastHit cameraRayHit; //Getting the ray from the center of the camera.
        Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out cameraRayHit, reticleMaxDistance);

        RaycastHit playerRayHit; //Firing a new ray from the player, to the first point of collision from the camera ray.
        if (Physics.Raycast(transform.position, (transform.position - cameraRayHit.point).normalized * -reticleMaxDistance, out playerRayHit, reticleMaxDistance))
        {
            reticleObj.transform.position = playerRayHit.point;
        }
        //\\//\\//\\

        ////Jump
        RaycastHit groundHit;
        if (Physics.Raycast(transform.position, transform.up * -1, out groundHit, transform.lossyScale.y + 0.4f))
            onGround = true;
        else
            onGround = false;

        if (jumpAxes != 0 && onGround && landRecovery <= 0f)
            RB.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);

        if (!onGround) landRecovery = 0.2f;
        else landRecovery -= Time.deltaTime;
        //\\//\\//\\
    }
}
