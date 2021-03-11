using Unity.Mathematics;
using UnityEngine;

public class FlockDebuger : MonoBehaviour
{

    [SerializeField] bool active = false;
    [SerializeField] int unitIndex;
    [SerializeField] LayerMask mask;

    private SFlockUnit[] _allUnits;
    private float _obstacleDistance;
    private float _sphereCastRadius;
    private float _mouthDistance;
    private Vector3 _testClosestpoint = Vector3.zero;

    public void InitDebugger(SFlockUnit[] allUnits, float obstacleDistance, float sphereCastRadius)
    {
        _allUnits = allUnits;
        _obstacleDistance = obstacleDistance;
        _sphereCastRadius = sphereCastRadius;
    }


    private void OnDrawGizmos()
    {
        if (!active) return;

        if (_allUnits != null && _allUnits.Length > 0)
        {
            var chosen = _allUnits[unitIndex];
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(chosen.MyTransform.position + (chosen.MyTransform.forward) * chosen.KillBoxDistance /** 0.85f*/, _sphereCastRadius * 0.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(chosen.MyTransform.position + (chosen.MyTransform.forward) * _obstacleDistance * 0.85f, _sphereCastRadius);
            Gizmos.color = Color.red;
            for (int i = 0; i < chosen.Directions.Length; i++)
            {
                var dir = chosen.transform.rotation * chosen.Directions[i];
                float angle = Vector3.Angle(chosen.transform.forward, math.normalize(dir));
                if (angle <= chosen.FOVAngle)
                    Gizmos.DrawWireSphere(chosen.MyTransform.position + dir * _obstacleDistance, _sphereCastRadius);
            }

            RaycastHit hit;
            if (Physics.SphereCast(chosen.MyTransform.position, _sphereCastRadius, chosen.MyTransform.forward, out hit, _obstacleDistance * 0.85f, mask))
            {
                float minAngle = math.INFINITY;
                for (int i = 0; i < chosen.Directions.Length; i++)
                {
                    // var dir = AllUnits[0].transform.TransformDirection(AllUnits[0].Directions[i].normalized);
                    var dir = chosen.transform.rotation * chosen.Directions[i];
                    //float3 currentDirection = ((AllUnits[0].MyTransform.position + (dir)) - AllUnits[0].transform.position);
                    float angle = Vector3.Angle(chosen.transform.forward, dir);
                    // var dir = AllUnits[0].transform.rotation * TestFlockUnit.Directions[i];
                    //Debug.Log("Dir: " + dir);
                    //Gizmos.DrawRay(AllUnits[0].MyTransform.position, dir * obstacleDistance);
                    if (!Physics.SphereCast(chosen.MyTransform.position, _sphereCastRadius, dir, out hit, _obstacleDistance * 0.85f, mask))
                    {
                        if (angle >= 20 && minAngle > angle)
                        {
                            minAngle = angle;
                            _testClosestpoint = chosen.MyTransform.position + dir * _obstacleDistance;
                        }
                    }
                }

                // Debug.Log("Test angle" + minAngle);
            }
            Gizmos.color = Color.red;
            // Gizmos.DrawWireSphere(testClosestpoint, 0.3f);
            //Debug.Log(testClosestpoint);
        }
    }
}
