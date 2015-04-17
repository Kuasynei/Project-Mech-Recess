using UnityEngine;
using System.Collections;

public class mechBehaviour : MonoBehaviour {

	public float acceleration = 5f;
    public float topSpeed = 10f;
    public float xSensitivity = 200f;
    public float ySensitivity = 200f;

	private float hAxisInput;
	private float vAxisInput;
	private float mouseX;
	private float mouseY;

    public GameObject mainCamera;

	private Rigidbody RB;

	void Awake () {
		RB = GetComponent<Rigidbody>();

	}
	
	void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

	}

	void Update()
	{
		hAxisInput = Input.GetAxis ("Horizontal");
		vAxisInput = Input.GetAxis ("Vertical");
		//mouseX = Input.GetAxis ("Mouse X");
		//mouseY = Input.GetAxis ("Mouse Y");

        RB.AddForce(new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z) * acceleration * vAxisInput);
        RB.AddForce(new Vector3(mainCamera.transform.right.x, 0, mainCamera.transform.right.z) * acceleration * hAxisInput);

        if (RB.velocity.magnitude > topSpeed)
        {
            RB.velocity = RB.velocity.normalized * topSpeed;
        }

        //transform.Rotate(new Vector3(0, mouseX*xSensitivity, 0) * Time.deltaTime);

	}
}
