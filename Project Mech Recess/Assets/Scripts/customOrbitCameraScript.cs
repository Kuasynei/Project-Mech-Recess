using UnityEngine;
using System.Collections;

public class customOrbitCameraScript : MonoBehaviour {
    
    public Transform player;
    public float maxDistance = 6;
    public float cameraSpeed = 10;
    public float turnSpeed = 10f;
    public float turnSmoothing;
    public float sphereCastRadius = 0.1f;
    public float clipSmoothing = 20f;
    public float clipMaxCorrectSpeed = 4f;
    public float maxTilt = 45;
    public float minTilt = 75;

    private float mouseX;
    private float mouseY;
    private float rigLookAngle;
    private Quaternion rigRotation;
    private float cameraTiltAngle;
    private Quaternion cameraRotation;
    private Vector3 cameraEulers;

    private Transform pivotTransform;   //Pivot point tied to camera
    private Transform pivotOffset;      //Pivot point purely for offsetting
    private Transform cameraTransform;

    private Ray pivotRay;
    private RaycastHit[] pivotRayHits;

    private Vector3 pivotDampenerVelocity;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        ////Set pivotTransform to position of the pivot object when the game starts.
        pivotTransform = transform.FindChild("Pivot").GetComponent<Transform>();
        pivotOffset = transform.FindChild("Pivot Set").GetComponent<Transform>();
        //\\//\\//\\

        //Get Camera Transform
        cameraTransform = GetComponentInChildren<Camera>().transform;

        cameraEulers = cameraTransform.rotation.eulerAngles;
        //\\//\\//\\
    }

    void Update()
    {
        ////Player Camera Follow
        if (player == null) return;
        transform.position = Vector3.Slerp(transform.position, player.position, cameraSpeed * Time.deltaTime);
        //\\//\\//\\

        ////Input
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        //\\//\\//\\

        ////X Rotation of Rig
        rigLookAngle += mouseX * turnSpeed;
        rigRotation = Quaternion.Euler(0f, rigLookAngle, 0f);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, rigRotation, turnSmoothing * Time.deltaTime);
        //\\//\\//\\

        ////Y Rotation of Camera
        cameraTiltAngle -= mouseY * turnSpeed;
        cameraTiltAngle = Mathf.Clamp(cameraTiltAngle, -minTilt, maxTilt);

        cameraRotation = Quaternion.Euler(cameraTiltAngle, cameraEulers.y, cameraEulers.z);
        cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, cameraRotation, turnSmoothing * Time.deltaTime);
        //Debug.Log("CAMERA Y: " + pivotTransform.position.y + "| PLAYER Y: " + player.transform.position.y + "|X rotation: " + (cameraRotation.x + 0.1f) * 2);
        //\\//\\//\\
        
        ////ray for pivot object to prevent clipping.
        pivotRay = new Ray(transform.position, 
            (new Vector3(pivotOffset.position.x, pivotOffset.position.y + (cameraRotation.x - 0.1f) * 7, pivotOffset.position.z) - transform.position).normalized * maxDistance);
        Debug.DrawRay(transform.position,
            (new Vector3(pivotOffset.position.x, pivotOffset.position.y + (cameraRotation.x - 0.1f) * 7, pivotOffset.position.z) - transform.position).normalized * maxDistance, Color.magenta);
        //\\//\\//\\

        ////Camera Spherecast to detect collision
        pivotRayHits = Physics.SphereCastAll(pivotRay, sphereCastRadius, maxDistance + sphereCastRadius);
        
        //Determine closest collision point on pivotRayHits.
        float nearest = Mathf.Infinity;
        
        for (int i = 0; i < pivotRayHits.Length; i++)
        {
            if (pivotRayHits[i].collider.tag != player.gameObject.tag) //If closest collision point and not the player, move pivot there.
            {
                if (pivotRayHits[i].distance < nearest)
                {
                    nearest = pivotRayHits[i].distance;
                    Debug.DrawLine(pivotTransform.position, new Vector3(pivotRayHits[i].point.x, pivotRayHits[i].point.y + 0.4f, pivotRayHits[i].point.z) - (pivotRay.direction), Color.cyan);
                    pivotTransform.position = Vector3.Slerp(pivotTransform.position, new Vector3(pivotRayHits[i].point.x, pivotRayHits[i].point.y + 0.4f, pivotRayHits[i].point.z) - (pivotRay.direction),
                                                            turnSmoothing * Time.deltaTime);
                    //Debug.Log("Cinder");
                }
            }
        }
        
        //\\//\\//\\

        ////If camera does not collide.
        if (nearest == Mathf.Infinity)
        {
            //Debug.Log("Magma");
            pivotTransform.position = Vector3.Slerp(pivotTransform.position, transform.position + pivotRay.direction*maxDistance, turnSmoothing * Time.deltaTime);
        }
        //\\//\\//\\
    }
}
