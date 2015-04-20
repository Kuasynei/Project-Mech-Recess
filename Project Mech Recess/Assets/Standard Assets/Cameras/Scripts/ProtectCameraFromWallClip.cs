using System;
using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Cameras
{
    public class ProtectCameraFromWallClip : MonoBehaviour
    {
        public float clipMoveTime = 0.05f;              // time taken to move when avoiding cliping (low value = fast, which it should be)
        public float returnTime = 0.4f;                 // time taken to move back towards desired position, when not clipping (typically should be a higher value than clipMoveTime)
        public float sphereCastRadius = 0.1f;           // the radius of the sphere used to test for object between camera and target
        public bool visualiseInEditor;                  // toggle for visualising the algorithm through lines for the raycast in the editor
        public float closestDistance = 0.5f;            // the closest distance the camera can be from the target
        public bool protecting { get; private set; }    // used for determining if there is an object between the target and the camera
        public string dontClipTag = "Player";           // don't clip against objects with this tag (useful for not clipping against the targeted object)
        public GameObject playerObj;

        private Transform m_Cam;                  // the transform of the camera
        private Transform m_Pivot;                // the point at which the camera pivots around
        private float m_OriginalDist;             // the original distance to the camera before any modification are made
        private float m_MoveVelocity;             // the velocity at which the camera moved
        private float m_CurrentDist;              // the current distance from the camera to the target
        private Ray m_Ray;                        // the ray used in the lateupdate for casting between the camera and the target
        private RaycastHit[] m_Hits;              // the hits between the camera and the target
        private RayHitComparer m_RayHitComparer;  // variable to compare raycast hit distances
        private Ray p_Ray;
        private RaycastHit[] p_Hits;

        private void Start()
        {
            // find the camera in the object hierarchy
            m_Cam = GetComponentInChildren<Camera>().transform;
            m_Pivot = m_Cam.parent;
            m_OriginalDist = m_Cam.localPosition.magnitude;
            m_CurrentDist = m_OriginalDist;

            // create a new RayHitComparer
            m_RayHitComparer = new RayHitComparer();
        }


        private void LateUpdate()
        {
            // initially set the target distance
            float targetDist = m_OriginalDist;

            m_Ray.origin = m_Pivot.position + m_Pivot.forward*sphereCastRadius;
            m_Ray.direction = -m_Pivot.forward;

            // initial check to see if start of spherecast intersects anything
            Collider[] cols = Physics.OverlapSphere(m_Ray.origin, sphereCastRadius);

            bool initialIntersect = false;
            bool hitSomething = false;

            // loop through all the collisions to check if something we care about
            for (int i = 0; i < cols.Length; i++)
            {
                if ((!cols[i].isTrigger) &&
                    !(cols[i].attachedRigidbody != null && cols[i].attachedRigidbody.CompareTag(dontClipTag)))
                {
                    initialIntersect = true;
                    break;
                }
            }
            // if there is a collision
            if (initialIntersect)
            {
                Debug.DrawRay(m_Ray.origin, m_Ray.direction * (m_OriginalDist - sphereCastRadius), Color.yellow); //[CUSTOM]
                m_Ray.origin -= m_Pivot.forward*sphereCastRadius;

                // do a raycast and gather all the intersections
                m_Hits = Physics.RaycastAll(m_Ray, m_OriginalDist - sphereCastRadius);
            }
            else
            {
                // if there was no collision do a sphere cast to see if there were any other collisions
                m_Hits = Physics.SphereCastAll(m_Ray, sphereCastRadius, m_OriginalDist + sphereCastRadius);
            }

            // sort the collisions by distance
            Array.Sort(m_Hits, m_RayHitComparer);

            // set the variable used for storing the closest to be as far as possible
            float nearest = Mathf.Infinity;

            // loop through all the collisions
            for (int i = 0; i < m_Hits.Length; i++)
            {
                // only deal with the collision if it was closer than the previous one, not a trigger, and not attached to a rigidbody tagged with the dontClipTag
                if ((m_Hits[i].distance < nearest && (!m_Hits[i].collider.isTrigger) &&
                    !(m_Hits[i].collider.attachedRigidbody != null &&
                      m_Hits[i].collider.attachedRigidbody.CompareTag(dontClipTag))))
                {
                    // change the nearest collision to latest
                    nearest = m_Hits[i].distance;
                    targetDist = -m_Pivot.InverseTransformPoint(m_Hits[i].point).z;
                    hitSomething = true;
                }
            }
            // visualise the cam clip effect in the editor
            if (hitSomething)
            {
                Debug.DrawRay(m_Ray.origin, -m_Pivot.forward*(targetDist + sphereCastRadius), Color.red);
            }
            
            // [CUSTOM] Extra Hit Detection, new raycast
            Vector3 tempCustomTarget = new Vector3(playerObj.transform.position.x, m_Pivot.transform.position.y, playerObj.transform.position.z);
            Debug.DrawRay(tempCustomTarget, m_Cam.transform.position - tempCustomTarget, Color.green);
            p_Ray = new Ray(tempCustomTarget, (m_Cam.transform.position - tempCustomTarget));
            p_Ray.origin += ((m_Cam.transform.position - tempCustomTarget).normalized)*0.1f;
            p_Hits = Physics.RaycastAll(p_Ray, m_OriginalDist);

            if (p_Hits.Length <= 0)
            {
                p_Hits = Physics.SphereCastAll(p_Ray, sphereCastRadius, m_OriginalDist + sphereCastRadius);
            }
            
            // Loop through all collisions
            for (int i = 0; i < p_Hits.Length; i++)
            {
                if (p_Hits[i].distance < nearest && p_Hits[i].collider != null &&
                    !p_Hits[i].collider.CompareTag(dontClipTag))
                {
                    nearest = p_Hits[i].distance;
                    targetDist = (m_Cam.transform.position - tempCustomTarget).magnitude;
                    Debug.Log(targetDist);
                }
            }

            //if (customHit && !hitSomething)
            // {
            //    m_Cam.position = Vector3.MoveTowards(m_Cam.position, tempCustomTarget, m_CurrentDist / 20);
            //}
            //\\//\\//\\

            // hit something so move the camera to a better position
            protecting = hitSomething;
            m_CurrentDist = Mathf.SmoothDamp(m_CurrentDist, targetDist, ref m_MoveVelocity,
                                           m_CurrentDist > targetDist ? clipMoveTime : returnTime);
            m_CurrentDist = Mathf.Clamp(m_CurrentDist, closestDistance, m_OriginalDist);
            m_Cam.localPosition = -Vector3.forward * m_CurrentDist;
        }


        // comparer for check distances in ray cast hits
        public class RayHitComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return ((RaycastHit) x).distance.CompareTo(((RaycastHit) y).distance);
            }
        }
    }
}
