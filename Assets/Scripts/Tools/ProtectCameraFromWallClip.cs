using System;
using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Cameras
{
    public class ProtectCameraFromWallClip : MonoBehaviour
    {
        #region Exposed Variables
        [Tooltip("Time taken to move when avoiding cliping (low value = fast, which it should be).")]
        [SerializeField] private float clipMoveTime = 0.05f;

        [Tooltip("Time taken to move back towards desired position, when not clipping (typically should be a higher value than clipMoveTime).")]
        [SerializeField] private float returnTime = 0.4f;

        [Tooltip("The radius of the sphere used to test for object between camera and target.")]
        [SerializeField] private float sphereCastRadius = 0.1f;

        [Tooltip("Toggle for visualising the algorithm through lines for the raycast in the editor.")]
        [SerializeField] private bool visualiseInEditor;

        [Tooltip("The closest distance the camera can be from the target.")]
        [SerializeField] private float closestDistance = 0.5f;

        [Tooltip("Don't clip against objects with this tag (useful for not clipping against the targeted object).")]
        [SerializeField] private string dontClipTag = "Player";
        #endregion


        #region Hidden Variables
        /// <summary>
        /// The transform of the camera.
        /// </summary>
        private Transform m_Cam;

        /// <summary>
        /// The point at which the camera pivots around.
        /// </summary>
        private Transform m_Pivot;

        /// <summary>
        /// The original distance to the camera before any modification are made.
        /// </summary>
        private float m_OriginalDist;

        /// <summary>
        /// The velocity at which the camera moved.
        /// </summary>
        private float m_MoveVelocity;

        /// <summary>
        /// The current distance from the camera to the target.
        /// </summary>
        private float m_CurrentDist;

        /// <summary>
        /// The ray used in the lateupdate for casting between the camera and the target.
        /// </summary>
        private Ray m_Ray = new Ray();

        /// <summary>
        /// The hits between the camera and the target.
        /// </summary>
        private RaycastHit[] m_Hits;

        /// <summary>
        /// Variable to compare raycast hit distances.
        /// </summary>
        private RayHitComparer m_RayHitComparer;
        #endregion


        /// <summary>
        /// Used for determining if there is an object between the target and the camera.
        /// </summary>
        public bool protecting { get; private set; }

        public Vector3 cameraOffset { get; set; }


        private void Start()
        {
            // find the camera in the object hierarchy
            m_Cam = GetComponentInChildren<Camera>().transform;
            m_Pivot = m_Cam.parent;
            //m_OriginalDist = m_Cam.localPosition.magnitude;
            m_CurrentDist = m_OriginalDist;

            // create a new RayHitComparer
            m_RayHitComparer = new RayHitComparer();
        }


        private void LateUpdate()
        {
            m_OriginalDist = cameraOffset.magnitude;

            // initially set the target distance
            float targetDist = m_OriginalDist;

            m_Ray.origin = m_Pivot.position + m_Pivot.forward*sphereCastRadius;
            m_Ray.direction = Vector3.Normalize((m_Pivot.right * cameraOffset.x) +
                                                (m_Pivot.up * cameraOffset.y) +
                                                (m_Pivot.forward * cameraOffset.z));

            // initial check to see if start of spherecast intersects anything
            var cols = Physics.OverlapSphere(m_Ray.origin, sphereCastRadius);

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
                m_Ray.origin += m_Pivot.forward*sphereCastRadius;

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
                if (m_Hits[i].distance < nearest && (!m_Hits[i].collider.isTrigger) &&
                    !(m_Hits[i].collider.attachedRigidbody != null &&
                      m_Hits[i].collider.attachedRigidbody.CompareTag(dontClipTag)))
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
                Debug.DrawRay(m_Ray.origin, m_Ray.direction*(targetDist + sphereCastRadius), Color.red);
            }

            // hit something so move the camera to a better position
            protecting = hitSomething;
            m_CurrentDist = Mathf.SmoothDamp(m_CurrentDist, targetDist, ref m_MoveVelocity,
                                           m_CurrentDist > targetDist ? clipMoveTime : returnTime);
            m_CurrentDist = Mathf.Clamp(m_CurrentDist, closestDistance, m_OriginalDist);
            Vector3 offsetDir = cameraOffset.normalized;
            Vector3 newDir = (offsetDir.x * Vector3.right * m_CurrentDist) +
                             (offsetDir.y * Vector3.up * m_CurrentDist) +
                             (offsetDir.z * Vector3.forward * m_CurrentDist);
            m_Cam.localPosition = newDir;
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
