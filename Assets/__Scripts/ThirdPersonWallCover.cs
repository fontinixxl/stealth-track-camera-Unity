// The #defines below are left in to show how I went about testing and debugging 
//  various aspects of this script.

//#define DEBUG_ThirdPersonTakeCoverAgainstWall
//#define DEBUG_ThirdPersonTakeCoverAgainstWall_Triggers
#define DEBUG_ThirdPersonTakeCoverAgainstWall_Raycasts
//#define DEBUG_ThirdPersonTakeCoverAgainstWall_CreepValue
//#define LockCreepValueTo1
#define WhenCamReversedSwitchingDirectionReleasesInputDirectionLock

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;


//public delegate void AgainstCoverDelegate 

[RequireComponent(typeof (ThirdPersonCharacter))]
[RequireComponent(typeof (ThirdPersonUserControl))]
[RequireComponent(typeof (Animator))]
[RequireComponent(typeof (ThirdPersonWallEdges))]
public class ThirdPersonWallCover : MonoBehaviour {

    [Header("Inscribed fields")]
    [Tooltip("The amount of time to push into cover before taking cover")]
    public float            pushTimeToCover = 0.5f;
    public float            coverTriggerDist = 0.3f;
    public float            coverTriggerRadius = 0.1f;
    public float            creepSpeed = 1;
    public float            inCoverDistFromWall = 0.5f;
    public float            coverTransitionU = 0.2f;
    public float            creepWallEdgeEasing = 0.1f;

    [Header("Dynamic fields")]
    [Tooltip("The primary direction that the player is pushing into cover")]
    public int              pushingDirection = -1;
    public float            pushingStartTime, pushingDuration;
    public bool[]           againstCover = new bool[4];
    public int              inCover = -1;
    public float            creep;

    ThirdPersonUserControl  tPUControl;
    ThirdPersonCharacter    tPCharacter;
    Animator                anim;
    CoverTrigger[]          coverTriggers;
    ThirdPersonWallEdges    tpwe;
    Rigidbody               rigid;

	float                   lastAnimMoveTime, animMoveTimeDelta;


    // The direction of the wall the player is against when inCover (e.g, when 
    //  inCover==0, the player is against a wall in the Vector3.forward direction)
    Vector3[]               pushingDirs = {
        Vector3.forward, Vector3.right, Vector3.back, Vector3.left
    };
    // The direction a player will move to the right in cover
    Vector3[]               inCoverMovementDirs = {
        Vector3.right, Vector3.back, Vector3.left, Vector3.forward
    }; // NOTE: This is just a 90 deg rotation from pushingDirs, 
       //  but I didn't want it to be confusing


    // Use this for initialization
    void Awake () {
        tPUControl = GetComponent<ThirdPersonUserControl>();
        tPCharacter = GetComponent<ThirdPersonCharacter>();
        anim = GetComponent<Animator>();
        tpwe = GetComponent<ThirdPersonWallEdges>();
        rigid = GetComponent<Rigidbody>();

        // Gather all the CoverTriggers
        coverTriggers = GetComponentsInChildren<CoverTrigger>();
        for (int i=0; i<coverTriggers.Length; i++) {
            InitCoverTrigger(coverTriggers[i]);
        }
    }

    void InitCoverTrigger(CoverTrigger ct) {
        ct.triggerHandler = TriggerHandler;
        ct.triggerExitHandler = TriggerExitHandler;
        // Position the CoverTrigger at the coverTriggerDist
        ct.transform.localPosition = ct.transform.localPosition.normalized * coverTriggerDist;
        // Scale the SphereCollider radius to coverTriggerRadius
        ct.sphereCollider.radius = coverTriggerRadius;
    }
    
    void TriggerHandler(CoverTrigger coverTrigger) {
        #if DEBUG_ThirdPersonTakeCoverAgainstWall_Triggers
        Debug.Log("ThirdPersonTakeCoverAgainstWall:TriggerHandler - Called by "
          +coverTrigger.name);
        #endif
        if (coverTrigger.coverNum != -1 && !againstCover[coverTrigger.coverNum]) {
            againstCover[coverTrigger.coverNum] = true;
        }
    }

    void TriggerExitHandler(CoverTrigger coverTrigger) {
        #if DEBUG_ThirdPersonTakeCoverAgainstWall_Triggers
        Debug.Log("ThirdPersonTakeCoverAgainstWall:TriggerExitHandler - Called by "
          +coverTrigger.name);
        #endif
        if (coverTrigger.coverNum != -1) {
            againstCover[coverTrigger.coverNum] = false;
        }
    }


    // Update is called once per frame
    void Update () {
        //updateTime = Time.time;

        int newPushingDirection = ReturnPrimaryPushingDirection();
        if (newPushingDirection != pushingDirection) {
            pushingStartTime = Time.time;
            pushingDirection = newPushingDirection;
            pushingDuration = 0;
        }

        // Check to see if cover needs to be exited
        if (inCover != -1 && pushingDirection != inCover) {
            ExitCover();
        }

        // If no pushingDirection, exit this method
        if (pushingDirection == -1) {
            return;
        }
        
        bool raycastAgainstCover = tpwe.wallL && tpwe.wallR;

        // If player has been pushing in the same direction as cover for long enough,
        //  enter cover
        pushingDuration = (Time.time - pushingStartTime);
        bool pushingLongEnough = pushingDuration > pushTimeToCover;
        if (inCover == -1 && raycastAgainstCover && pushingLongEnough 
          && againstCover[pushingDirection]) {
            // Enter cover!
            EnterCover(pushingDirection);
        }

        if (inCover != -1) {
            AlignWithCover();
            //CreepAlongCover();
        }
    }

	//private void OnAnimatorMove()
	//{
 //       if (inCover != -1) {
 //           CreepAlongCover();
 //       }
	//}

	void AlignWithCover() {
        // if in cover, try to get into the right rotation and distance from cover
        RaycastHit hitInfo;
        LayerMask coverMask = LayerMask.GetMask("Cover");

        // We know because inCover != -1 that this will hit, so it doesn't need to be in an if statement
        Physics.Raycast(transform.position+Vector3.up, pushingDirs[inCover], out hitInfo, 2, coverMask);
        if (hitInfo.collider == null) {
            Debug.LogError("ThirdPersonTakeCoverAgainstWall:Update - inCover>-1 yet somehow the Raycast didn't hit a collider.");
            return;
        }

        // Rotate into the correct position
        transform.rotation = Quaternion.Euler(0, 90*inCover, 0);
    }


    void CreepAlongCover(float deltaTime) {
        // Allow creeping left and right while inCover
        creep = GetCreepValue();
        if (Mathf.Abs(creep) < 0.1f) {
            SetAnimFloatCreep(0);
            return;
        }

        Vector3 pos0 = transform.position;
        Vector3 pos1 = pos0 + inCoverMovementDirs[inCover]*( creepSpeed*creep*deltaTime );
        transform.position = pos1;
        // Check to make sure it's not the edge of the cover
        if ( (!tpwe.wallL && creep<0) || (!tpwe.wallR && creep>0) ) {
            // This movement would have pushed the character outside of cover, 
            //  so return to previous position
            transform.position = pos0;
            SetAnimFloatCreep(0.1f*creep, creepWallEdgeEasing);
            return;
        }
        // We're not at the edge of cover, so allow move to new position and animate
        SetAnimFloatCreep(creep);
    }

    void SetAnimFloatCreep(float val, float easing=1) {
        #if DEBUG_ThirdPersonTakeCoverAgainstWall_CreepValue
        //Debug.LogWarning("SetAnimFloatCreep( "+val+" )\tTime: "+Time.time+"\tFixedTime: "+Time.fixedTime);
        GraphLogger.Log("SetAnimFloatCreep( "+val+" )", val, "Time:", updateTime, "FixedTime:", Time.fixedTime, "AnimMoveTimeDelta", animMoveTimeDelta);
        #endif
        val = (1-easing)*anim.GetFloat("Creep") + easing*val;
        anim.SetFloat("Creep", val);
    }

    float GetCreepValue() {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
#if DEBUG_ThirdPersonTakeCoverAgainstWall_CreepValue
        //print("GetCreepValue()\tinCover:"+inCover+"\th:"+h+"\tv:"+v);
#endif

        StealthPlayerCamera.eCamMode newCamMode = StealthPlayerCamera.MODE;
#if LockCreepValueTo1
        float threshold = 0.1f;
        switch (inCover) {
            case 0:
                if (Mathf.Abs(h) > threshold) {
                    return (h > 0) ? 1 : -1;
                }
                break;
            case 1:
                if (Mathf.Abs(v) > threshold) {
                    return (-v > 0) ? 1 : -1;
                }
                break;
            case 2:
                // NOTE: This version does not include the still holding stuff from the other version.
                if (Mathf.Abs(h) > threshold) {
                    return (-h > 0) ? 1 : -1;
                }
                break;
            case 3:
                if (Mathf.Abs(v) > threshold) {
                    return (v > 0) ? 1 : -1;
                }
                break;
        }
#else
        switch (inCover) {
            case 0:
                return h;
            case 1:
                return -v;
            case 2:
                // You need to add the bit here that will implement controls
                //  like those shown in the Challenge video

                // This breaks the pattern here, but it's much less confusing to the player.
                // Because the camera rotates 180 deg in near mode, the left and right keys
                //  need to be reversed in near mode. However, if the player is already holding
                //  one of those keys, that key should not be reversed until the key is released
                //  and then re-pressed. It's a bit complex, but it's the way that Metal Gear
                //  Solid handled the issue.

                // Replace this line
                return -h;

            case 3:
                return v;
        }
#endif

        return 0;
    }


    // Check to see which cardinal direction the player is moving in
    int ReturnPrimaryPushingDirection() {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // If inCover != -1, then first check in the direction of cover
        // This keeps the player in cover more easily, especially when using an analog stick.
        switch (inCover)
        {
            case 0:
                if (v>0) return 0;
                break;

            case 1:
                if (h>0) return 1;
                break;

            case 2:
                if (v < 0) return 2;
                break;

            case 3:
                if (h < 0) return 3;
                break;
        }

        if (Mathf.Abs(h) < 0.25f && Mathf.Abs(v) < 0.25f) {
            return -1;
        }
        if (Mathf.Abs(h) > Mathf.Abs(v)) {
            if (h > 0) {
                return 1;
            } else {
                return 3;
            }
        } else {
            if (v > 0) {
                return 0;
            } else {
                return 2;
            }
        }
    }


    void EnterCover(int pDir) {
        #if DEBUG_ThirdPersonTakeCoverAgainstWall
        Debug.Log("ThirdPersonTakeCoverAgainstWall:EnterCover()");
        #endif
        inCover = pDir;
        tPUControl.enabled = false;
        tPCharacter.enabled = false;
        anim.SetBool("InCover", true);
        //rigid.isKinematic = true; // Disable physics from moving the character randomly
        // Position character right next to cover

        #if DEBUG_ThirdPersonTakeCoverAgainstWall_CreepValue
        GraphLogger.ClearLog();
        #endif
    }


    void ExitCover() {
        #if DEBUG_ThirdPersonTakeCoverAgainstWall
        Debug.Log("ThirdPersonTakeCoverAgainstWall:ExitCover()");
        #endif
        inCover = -1;
        anim.SetBool("InCover", false);
        tPUControl.enabled = true;
        tPCharacter.enabled = true;
        //rigid.isKinematic = false; // Re-enable physics

        #if DEBUG_ThirdPersonTakeCoverAgainstWall_CreepValue
        GraphLogger.PrintLog();
        #endif
    }


	private void OnAnimatorMove()
	{
        animMoveTimeDelta = Time.time - lastAnimMoveTime;
		if (inCover != -1) {
            CreepAlongCover(animMoveTimeDelta);
		}
        lastAnimMoveTime = Time.time;
    }


    private void FixedUpdate()
    {
        if (inCover != -1) {
            rigid.velocity = Vector3.zero;
        }
    }


    public class CoverInfo {
        public int  inCover;
        public bool wallL, wallR, zoomL, zoomR;
    }


    public CoverInfo GetCoverInfo() {
        CoverInfo coverInfo = new CoverInfo();
        coverInfo.inCover = inCover;
        coverInfo.wallL = tpwe.wallL;
        coverInfo.wallR = tpwe.wallR;
        coverInfo.zoomL = tpwe.zoomL;
        coverInfo.zoomR = tpwe.zoomR;

        return coverInfo;
    }
}
