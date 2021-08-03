using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealthPlayerCamera : MonoBehaviour
{
    static private StealthPlayerCamera _S;

    public enum eCamMode { far, nearL, nearR };

    [Header("Inscribed")]
    [Tooltip("The instance of ThirdPersonWallCover attached to the player character.")]
    public ThirdPersonWallCover playerInstance;

    [Tooltip("[0..1] At 0, the camera will never move, at 1, the camera will " +
             "follow immediately with no lag.")]
    [Range(0, 1)]
    public float        cameraEasing = 0.25f;

    [Header("Inscribed – Far Mode")]
    [Tooltip("If this is set to [0,0,0], the relative position of the camera " +
             "to the player in the scene will be used.")]
    public Vector3      relativePosFar = Vector3.zero;

    [Tooltip("The rotation about the x axis of the camera in Far mode.")]
    public float        xRotationFar = 60;

    [Header("Inscribed – Near Mode")]
    [Tooltip("Relative position of the camera to the player when Near Mode")]
    public Vector3      relativePosNear = Vector3.zero;
    [Tooltip("Determines how far the camera will be on the X axis (relative to the player)")]
    public float relativeXPosNear = 1.2f;

    [Tooltip("The rotation about the x axis of the camera in Near mode.")]
    public float xRotationNear = 15;

    [Header("Dynamic")]
    public eCamMode camMode = eCamMode.far;

    private void Awake()
    {
        S = this;
        // If the desiredRelativePos is unset, base it on where the camera starts relative to the player
        if (relativePosFar == Vector3.zero)
        {
            relativePosFar = transform.position - playerInstance.transform.position;
        }
    }


    // Update is called once per frame
    void Update()
    {
        ThirdPersonWallCover.CoverInfo coverInfo = playerInstance.GetCoverInfo();
        if (coverInfo.inCover == -1)
        {
            // When not inCover, the camMode is always eCamMode.far
            camMode = eCamMode.far;
        }
        else
        {
            // When inCover, the camMode switches to eCamMode.near_ if the player is near the edge of cover
            if (coverInfo.IsNearLeftEdge())
            {
                camMode = eCamMode.nearL;
            } else if (coverInfo.IsNearRighEdge())
            {
                camMode = eCamMode.nearR;
            }
            else
            {
                // If we are in cover but not near either side edges OR
                // we are near the edge in both sides, either case we will not ZoomIn
                // and keep camera mode as far.
                camMode = eCamMode.far;
            }
        }

        // This is initially [0,0,0] to show the issue visually by jumping the Camera
        // to the origin if the position is not set properly in the switch statement.
        Vector3 pDesired = Vector3.zero;
        Quaternion rotDesired = Quaternion.identity;
        switch (camMode)
        {
            case eCamMode.far:
                pDesired = playerInstance.transform.position + relativePosFar;
                rotDesired = Quaternion.Euler(xRotationFar, 0, 0);
                break;
            case eCamMode.nearL:
            case eCamMode.nearR:
                // Desired position should be relative to playerInstance facing and position
                Vector3 pRelative = relativePosNear;
                // Offset camera to either left or right depending on what direction the player is looking at
                pRelative.x += (camMode == eCamMode.nearL) ? -relativeXPosNear : relativeXPosNear;
                // Reposition pRelative base on the player's local transform
                pDesired = playerInstance.transform.TransformPoint(pRelative);
                rotDesired = Quaternion.Euler(xRotationNear, coverInfo.inCover * 90, 0);
                break;
        }

        Vector3 pInterp = (1 - cameraEasing) * transform.position + cameraEasing * pDesired;
        transform.position = pInterp;

        Quaternion rotInterp = Quaternion.Slerp(transform.rotation, rotDesired, cameraEasing);
        transform.rotation = rotInterp;
    }


    public void JumpToFarPosition()
    {
        transform.position = playerInstance.transform.position + relativePosFar;
        transform.rotation = Quaternion.Euler(xRotationFar, 0, 0);
    }


    /// <summary>
    /// This provides a bit of protection to the Singleton-like implementation here.
    /// <para>Unlike a regular Singleton implementation, this one is not globally available.
    /// However, like a traditional Singleton (or Highlander), there can only be one.</para>
    /// </summary>
    static private StealthPlayerCamera S
    {
        get { return _S; }
        set
        {
            if (_S != null)
            {
                Debug.LogError("StealthPlayerCamera:S - Attempt to set Singleton" +
                               " when it has already been set.");
                Debug.LogError("Old Singleton: " + _S.gameObject.name +
                               "\tNew Singleton: " + value.gameObject.name);
            }
            _S = value;
        }
    }

    static public eCamMode MODE
    {
        get
        {
            if (_S == null)
            {
                return eCamMode.far;
            }
            return _S.camMode;
        }
    }

    static public void ResetToFarPosition()
    {
        S.JumpToFarPosition();
    }

}
