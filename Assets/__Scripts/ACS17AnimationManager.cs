using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ACS17AnimationManager : MonoBehaviour
{
    [Header("Inscribed")]
    public float        animTransTime = 0;

    private bool        inited = false;
    private Animator    anim;
    private EnemyNav    eNav;
    private string      currAnimState = "";

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();

        eNav = transform.parent.GetComponentInParent<EnemyNav>();
        if (eNav == null)
        {
            Debug.LogError("ACS17AnimationManager:Start() - Can't find" +
                           "EnemyNav component on self or parent.");
            return;
        }

        inited = true;
    }


    void CrossFade(string newState) {
        if (newState != currAnimState) {
            anim.CrossFade(newState, animTransTime);
            currAnimState = newState;
        }
    }


    void Update()
    {
        // If something went wrong with the initialization, don't try to animate.
        if (!inited) return;

        // Animate based on the EnemyNav eMode
        switch (eNav.mode)
        {
            case EnemyNav.eMode.idle:
            case EnemyNav.eMode.wait:
                CrossFade("ACS_Idle");
                break;

            case EnemyNav.eMode.preMoveRot:
            case EnemyNav.eMode.postMoveRot:
                if (eNav.turnDir == -1)
                {
                    CrossFade("ACS_TurnLeft");
                }
                else
                {
                    CrossFade("ACS_TurnRight");
                }
                break;

            case EnemyNav.eMode.move:
                CrossFade("ACS_Walk");
                break;
        }

    }

}
