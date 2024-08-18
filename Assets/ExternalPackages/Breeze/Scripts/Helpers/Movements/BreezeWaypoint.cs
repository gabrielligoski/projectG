using UnityEngine;

namespace Breeze.Core
{
    public class BreezeWaypoint : MonoBehaviour
    {
        [HideInInspector] public GameObject NextWaypoint = null;
        [HideInInspector] public bool WaitOnWaypoint = true;
        [HideInInspector] public float MinIdleLength = 4f;
        [HideInInspector] public float MaxIdleLength = 8f;
        [HideInInspector] public bool DrawGizmos = true;
        [HideInInspector] public Color GizmosColor = Color.yellow;

        //Draw Gizmos
        private void OnDrawGizmos()
        {
            if (NextWaypoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, NextWaypoint.transform.position);
            }

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + new Vector3(0.35f, 1, 0), gameObject.name);      
#endif

            Gizmos.color = GizmosColor;
            Gizmos.DrawSphere(transform.position, 0.4f);   
        }

        //Make the waypoint follow ground
        public void OnValidate()
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position + new Vector3(0, 5, 0), -transform.up, out hit,100, -1, QueryTriggerInteraction.Ignore))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + 0.4f, transform.position.z);
            }
        }

        private void Awake()
        {
            GetComponent<Collider>().enabled = false;
        }
    }
}