using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtOnDrawGizmos : MonoBehaviour {
    [Header("Inscribed")]
    [Tooltip("The Transform this should look at")]
    public Transform lookAt;

    private void OnDrawGizmos()
    {
        // This takes advantage of OnDrawGizmos being called in the editor
        //  both when the game is playing and when it's not.
        if (lookAt != null) {
            transform.LookAt(lookAt);
        }
    }
}
