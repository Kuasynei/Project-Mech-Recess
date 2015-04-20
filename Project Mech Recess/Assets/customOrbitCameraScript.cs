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
        transform.position = Vector3.Lerp(transform.position, player.position, cameraSpeed * Time.deltaTime);
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

        //Adds and subtracts height based on the angle of the camera
        pivotTransform.localPosition = Vector3.Lerp(pivotTransform.localPosition, new Vector3(pivotTransform.localPosition.x, pivotOffset.localPosition.y * (cameraRotation.x+0.2f) * 7, 
                                                    pivotOffset.localPosition.z * (cameraRotation.x+0.5f)), cameraSpeed * Time.deltaTime);
        Debug.Log("CAMERA Y: " + cameraTransform.position.y + "| PLAYER Y: " + player.transform.position.y + "|Y MOD: " + pivotTransform.position.y * (cameraRotation.x + 1) * 2);

        //\\//\\//\\

        ////Ray for pivot object to prevent clipping.
        pivotRay = new Ray(transform.position, (pivotOffset.position - transform.position).normalized * maxDistance);
        pivotRay.origin -= pivotRay.direction.normalized * 0.1f;
        Debug.DrawRay(transform.position - pivotRay.direction.normalized * 0.1f, (pivotOffset.position - transform.position).normalized * maxDistance);
        //pivotRayHits = Physics.RaycastAll(pivotRay, maxDistance);

        ////Camera Spherecast to detect collision
        pivotRayHits = Physics.SphereCastAll(pivotRay, sphereCastRadius, maxDistance + sphereCastRadius);

        //Determine closest collision point on pivotRayHits.
        float nearest = Mathf.Infinity;
        for (int i = 0; i < pivotRayHits.Length; i++)
        {
            if (pivotRayHits[i].distance < nearest)
            {
                nearest = pivotRayHits[i].distance;
                if (pivotRayHits[i].collider.tag != player.gameObject.tag) //If closest collision point and not the player, move pivot there.
                {
                    Debug.DrawLine(pivotTransform.position, pivotRayHits[i].point - pivotRay.direction, Color.blue);
                    pivotTransform.position = Vector3.SmoothDamp(pivotTransform.position, pivotRayHits[i].point - pivotRay.direction,
                        ref pivotDampenerVelocity, clipSmoothing, clipMaxCorrectSpeed);

                    //Debug.Log(pivotRayHits[i].distance);
                    //Debug.Log("Cinder");
                }
            }
        }

        //\\//\\//\\

        ////Left and Right Camera Bumpers [U/C]
        /*
        Ray rightRay = new Ray(pivotTransform.position, pivotTransform.right);
        RaycastHit rightRayHit;
        Debug.DrawRay(pivotTransform.position, pivotTransform.right, Color.cyan);
        if (Physics.Raycast(rightRay, 1f))
        {
            Debug.Log("Shock");
            pivotTransform.tra
        }


        Ray leftRay = new Ray(pivotTransform.position, -pivotTransform.right);
        RaycastHit leftRayHit;
        Debug.DrawRay(pivotTransform.position, -pivotTransform.right, Color.magenta);
         */
        //\\//\\//\\

        ////If no Collisions (excluding the one with the player) then resume normal function.
        if ((pivotRayHits.Length <= 1 && pivotRayHits.Length > 0) /*&& !(Physics.Raycast(rightRay, 1f) || Physics.Raycast(leftRay, 1f))*/)
        {
            //Debug.Log("Ash");
            if (pivotRayHits[0].collider.tag == player.gameObject.tag)
            {
                pivotTransform.position = Vector3.SmoothDamp(pivotTransform.position, pivotOffset.position,
                    ref pivotDampenerVelocity, clipSmoothing, clipMaxCorrectSpeed);
            }
        }
        else if (pivotRayHits.Length <= 0)
        {
            //Debug.Log("Coal");
            pivotTransform.position = Vector3.SmoothDamp(pivotTransform.position, pivotOffset.position,
                ref pivotDampenerVelocity, clipSmoothing, clipMaxCorrectSpeed);
        }
        //\\//\\//\\
    }
}
