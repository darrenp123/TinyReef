
// Class used to make it easier to make, use and visualize the fish waypoints 
// way points must be child objects of this object
using UnityEngine;

public class FlockWaypoints : MonoBehaviour
{
    [SerializeField] bool drawPatrolPath;
    [SerializeField] float waypointGizmosRadious = 0.3f;
    [SerializeField] Color gizmosColor = Color.gray;

    public Vector3[] GetWaypoints()
    {
        Vector3[] Waypoints = new Vector3[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            Waypoints[i] = transform.GetChild(i).position;
        }

        return Waypoints;
    }

    // function that draws waypoints on the editor
    private void OnDrawGizmos()
    {
        if (!drawPatrolPath) return;

        Gizmos.color = gizmosColor;

        for (int i = 0; i < transform.childCount; i++)
        {
            Gizmos.DrawSphere(GetWaypoint(i), waypointGizmosRadious);
            Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(GetNextIndex(i)));
        }
    }

    private int GetNextIndex(int i) => i + 1 < transform.childCount ? i + 1 : 0;

    private Vector3 GetWaypoint(int i) => transform.GetChild(i).position;
}
