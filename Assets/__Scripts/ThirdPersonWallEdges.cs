#define DEBUG_ThirdPersonWallEdges_Raycasts
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ThirdPersonWallCover))]
public class ThirdPersonWallEdges : MonoBehaviour
{
    public float camZoomWallEdgeDist = 1.25f;

    // Public get accessor properties for the four different edgeBools
    // wallL/wallR are close to the player and let us know if she is against a wall
    // When she is creeping along a wall to the Left, and wallL becomes false, then
    //  she is at the corner and must stop creeping in that direction (controlled
    //  by ThirdPersonWallCover.
    public bool wallL { get { return edgeBools[0]; } }
    public bool wallR { get { return edgeBools[1]; } }
    // zoomL/zoomR are positioned at camZoomWallEdgeDist from the player and tell
    //  us when she is approaching an edge, and the camera should move into Near mode.
    public bool zoomL { get { return edgeBools[2]; } }
    public bool zoomR { get { return edgeBools[3]; } }

    //Transform       wallLTrans, wallRTrans, zoomLTrans, zoomRTrans;
    private ThirdPersonWallCover tpwc;

    Transform[] sensorTransforms;
    float[] distances;
    bool[] edgeBools = new bool[4];

    // Use this for initialization
    void Start()
    {
        tpwc = GetComponent<ThirdPersonWallCover>();
        // We don't need to check whether tpwc is null because of the 
        //  RequireComponent compiler attribute above.

        // I'm putting both of these into arrays so that I can use a for loop 
        //  throughout rather than repeat very similar code 4x
        sensorTransforms = new Transform[4];
        distances = new float[] { -tpwc.coverTriggerDist, tpwc.coverTriggerDist,
            -camZoomWallEdgeDist, camZoomWallEdgeDist };

        for (int i = 0; i < sensorTransforms.Length; i++)
        {
            sensorTransforms[i] = new GameObject("WallEdgeSensor_" + i).transform;
            sensorTransforms[i].SetParent(transform);
            sensorTransforms[i].localPosition = new Vector3(distances[i], 1, 0);
            // Just to be sure, though they should have been initialized to false
            edgeBools[i] = false; 
        }
    }


    void FixedUpdate()
    {
        // Check collisions with walls
        LayerMask coverMask = LayerMask.GetMask("Cover");
        Vector3 raycastDir = transform.forward;
        float raycastDist = tpwc.coverTriggerDist * 1.25f;
        RaycastHit hitInfo;
        Vector3 toSensorTransform;

        for (int i = 0; i < sensorTransforms.Length; i++)
        {
            toSensorTransform = sensorTransforms[i].position - transform.position;
            // Check whether the sensorTransforms[i].position is inside a wall
            if (Physics.Raycast(transform.position, toSensorTransform, out hitInfo,
                                toSensorTransform.magnitude, coverMask))
            {
                // sensorTransforms[i] is inside a cover wall, which makes edgeBools[i]
                //  true (to keep the camera from zooming in that case)
                edgeBools[i] = true;
            }
            else
            {
                edgeBools[i] = Physics.Raycast(sensorTransforms[i].position, raycastDir,
                                               out hitInfo, raycastDist, coverMask);
            }
#if DEBUG_ThirdPersonWallEdges_Raycasts
            Debug.DrawLine(sensorTransforms[i].position, sensorTransforms[i].position
                           + raycastDir * raycastDist, edgeBools[i] ? Color.green 
                           : Color.red, 0, false);
#endif

        }

    }


}
