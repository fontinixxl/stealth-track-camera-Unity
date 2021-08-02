using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNav : MonoBehaviour
{
    public enum eMode { idle, wait, preMoveRot, move, postMoveRot};// chase, stopChase };

    [Header("Inscribed")]
    public bool             drawGizmos;
    public List<Waypoint>   waypoints;
    public float            speed = 4;
    public float            angularSpeed = 90;

    [Header("Dynamic")]
    [SerializeField]
    private eMode           _mode = eMode.wait;
    public int              wpNum = 0;
    public float            pathTime;
    public float            waitUntil;
    [Tooltip("Left=-1, None=0, Right=1")]
    public int              turnDir = 0;

    protected NavMeshAgent  nav;

    public eMode mode
    {
        get
        {
            return _mode;
        }

        set
        {
            _mode = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        nav.stoppingDistance = 0.01f;

        pathTime = 0;
        wpNum = 0;
        MoveToWaypoint(0);
    }

    void MoveToNextWaypoint()
    {
        int wpNum1 = wpNum + 1;
        if (wpNum1 >= waypoints.Count)
        {
            wpNum1 = 0;
        }

        MoveToWaypoint(wpNum1);
    }

    void MoveToWaypoint(int num)
    {
        wpNum = num;
        nav.SetDestination(waypoints[wpNum].pos);
        nav.isStopped = true;
        nav.updatePosition = false;

        mode = eMode.preMoveRot;
    }


    bool RotateTowards(Vector3 goalPos, float deg, bool rotYOnly = true)
    {
        Vector3 delta = goalPos - transform.position;
        if (rotYOnly)
        {
            delta.y = 0;
        }
        Quaternion r0 = transform.rotation;
        Vector3 fwd0 = transform.forward;
        transform.LookAt(transform.position + delta);
        Quaternion r1 = transform.rotation;
        Vector3 fwd1 = transform.forward;
        transform.rotation = Quaternion.RotateTowards(r0, r1, deg);

        // Use a cross product to determine whether we're turning
        //  left (-1) or right (1).
        Vector3 cross = Vector3.Cross(fwd0,fwd1);
        if (cross.y > 0) {
            turnDir = 1;
        } else if (cross.y < 0) {
            turnDir = -1;
        }

        return (Quaternion.Angle(transform.rotation, r1) < 1);
    }

    // FixedUpdate is called once per Physics engine frame (default 50x/sec)
    void FixedUpdate()
    {
        if (!Mathf.Approximately(nav.speed, speed)
            || !Mathf.Approximately(nav.angularSpeed, angularSpeed) )
        {
            nav.speed = speed;
            nav.angularSpeed = angularSpeed;
        }

        pathTime += Time.fixedDeltaTime;
        turnDir = 0;

        switch (mode)
        {
            case eMode.preMoveRot:
                Vector3 goalPos = waypoints[wpNum].pos;// nav.nextPosition;
                if (RotateTowards(goalPos, angularSpeed * Time.fixedDeltaTime))
                {
                    nav.isStopped = false;
                    nav.updatePosition = true;
                    mode = eMode.move;
                }
                break;

            case eMode.move:
                // Navigate towards the waypoint
                if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance)
                {
                    // We've reached the destination waypoint
                    mode = eMode.postMoveRot;
                }
                break;

            case eMode.postMoveRot:
                if (RotateTowards(transform.position + waypoints[wpNum].fwd, angularSpeed * Time.fixedDeltaTime))
                {
                    waitUntil = pathTime + waypoints[wpNum].waitTime;
                    mode = eMode.wait;
                }
                break;

            case eMode.wait:
                // Are we still waiting?
                if (pathTime < waitUntil)
                {
                    break;
                }
                // We've waited long enough, Move on to the next Waypoint
                MoveToNextWaypoint();
                break;

        }

    }


    const string gizmoIconPrefix = "GizmoIcon_128_";
    private void OnDrawGizmos()
    {
        if (!drawGizmos || !Application.isEditor || 
            Application.isPlaying || waypoints.Count < 2)
        {
            return;
        }

        Vector3 p0, p1;
        Vector3 iconDrawLoc;
        List<Vector3> usedIconDrawLocs = new List<Vector3>();
        Gizmos.color = Color.red;
        Vector3 gizmoIconOverlapOffset = Camera.current.transform.right * 0.75f
                                          - Camera.current.transform.up * 0.25f;
        
        for (int i = 0; i < waypoints.Count; i++)
        {
            p0 = waypoints[i].pos + Vector3.up;
            p1 = Vector3.up + ((i < waypoints.Count - 1) ? waypoints[i + 1].pos
                               : waypoints[0].pos);
            Gizmos.DrawLine(p0, p1);
            
            // Draw the number icons (up to 9; I didn't implement it beyond that)
            if (i < 10)
            {
                iconDrawLoc = p0 + Vector3.up;
                while (usedIconDrawLocs.Contains(iconDrawLoc))
                {
                    iconDrawLoc += gizmoIconOverlapOffset;
                }
                usedIconDrawLocs.Add(iconDrawLoc);
                Gizmos.DrawIcon(iconDrawLoc, gizmoIconPrefix + i, true);
            }
        }
    }
}
