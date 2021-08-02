using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableRootMotionOnTaggedAnimations : MonoBehaviour {
    public bool         rootMotionEnabled = false;

    Animator            animator;
    AnimatorStateInfo   stateInfo;

    void Awake () {
        animator = this.gameObject.GetComponent<Animator> ();
        animator.applyRootMotion = false;
    }

    void OnAnimatorMove () {
        stateInfo = animator.GetCurrentAnimatorStateInfo (0);
        if (rootMotionEnabled = !stateInfo.IsTag ("DisableRootMotion")) {
            animator.ApplyBuiltinRootMotion ();
        }
    }
}
