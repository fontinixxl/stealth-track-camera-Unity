using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>Clones all the localPositions, localRotations, and localScales
/// of the children of a GameObject so that they can be applied to another
/// GameObject. This can be used to copy the current pose from one 
/// humanoid character to another.</para>
/// 
/// <para>When working on this project, I ran into an issue where the 
/// player character (called Player in my scene) got stuck in a really 
/// bizarre pose, and looking online, there really wasn't an easy way to 
/// reset her to a T pose. So I wrote this script.</para>
/// 
/// <para>To use this, I dragged a second copy of the player character 
/// model into the scene and dragged its Skeleton child GameObject into 
/// the activeGO field. Then, I checked the checkToCopy bool. Because this
/// is handled in OnDrawGizmos, the game does not need to be running, and 
/// it can make changes to the scene that are saved.</para>
/// 
/// <para>Then, I dragged the Skeleton child GameObject of Player into the
/// activeGO field and checked checkToPaste. This applied the T pose back 
/// to Player. </para>
/// 
/// <para>NOTE: The copy and paste is based on the names of all the 
/// GameObjects in the hierarchy, so if something is named differently or 
/// doesn't exist in one of the models, it is ignored.</para>
/// 
/// <para>NOTE: You should be very careful with this kind of use of 
/// OnDrawGizmos. I shoudl have written an Editor Editor Extension script 
/// to do this, but I haven't had the time yet.</para>
/// 
/// <para>FINAL NOTE: When you paste the pose, it won't immediately update
/// in the Scene window, but if you click on another object or do anything
/// else to update the Scene pane, you will see that the paste happened 
/// properly.</para>
/// </summary>
public class PoseCloner : MonoBehaviour {
    public struct TransRec {
        public Vector3      pos;
        public Quaternion   rot;
        public Vector3      scale;

        public TransRec(Transform transform) {
            pos = transform.localPosition;
            rot = transform.localRotation;
            scale = transform.localScale;
        }

        public void SetTransform(Transform t) {
            t.localPosition = pos;
            t.localRotation = rot;
            t.localScale = scale;
        }
    }

    static public Dictionary<string, TransRec> poseDict;

    public GameObject   activeGO;
    public bool         checkToCopy, checkToPaste;

    private void OnDrawGizmos()
    {
        if (checkToCopy) {
            Copy();
            checkToCopy = false;
        }

        if (checkToPaste) {
            Paste();
            checkToPaste = false;
        }
    }

    // Use this for initialization
    void Copy() {
        if (activeGO == null) return;
        Transform[] tforms = activeGO.GetComponentsInChildren<Transform>();

        poseDict = new Dictionary<string, TransRec>();

        string str;
        foreach (Transform t in tforms) {
            str = GetPathName(t);
            poseDict.Add(str, new TransRec(t));
        }

        Debug.Log("PoseCloner:Copy() - Copied "+tforms.Length+" transforms.");
    }

    void Paste() {
        if (activeGO == null) return;
        if (poseDict == null || poseDict.Count == 0) {
            Debug.Log("PoseCloner:Paste() - No copied data exists to paste.");
            return;
        }
        Transform[] tforms = activeGO.GetComponentsInChildren<Transform>();
        int pasteCount = 0;
        List<string> pasted = new List<string>();
        List<string> skipped = new List<string>();
        List<string> notFound = new List<string>();

        string str;
        foreach (Transform t in tforms) {
            str = GetPathName(t);
            if (poseDict.ContainsKey(str)) {
                poseDict[str].SetTransform(t);
                pasteCount++;
                pasted.Add(str);
            } else {
                notFound.Add(str);
            }
        }

        foreach (string s in poseDict.Keys) {
            if (pasted.IndexOf(s) == -1) {
                skipped.Add(s);
            }
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("PoseCloner:Paste() -");
        sb.Append(" Pasted:"+pasted.Count);
        sb.Append(" Skipped:"+skipped.Count);
        sb.AppendLine(" NotFoundInPoseDict:"+notFound.Count);
        sb.AppendLine();

        sb.AppendLine("Pasted:");
        foreach (string s in pasted) {
            sb.Append("\t");
            sb.AppendLine(s);
        }
        sb.AppendLine();

        sb.AppendLine("Skipped:");
        foreach (string s in skipped) {
            sb.Append("\t");
            sb.AppendLine(s);
        }
        sb.AppendLine();

        sb.AppendLine("NotFoundInPoseDict:");
        foreach (string s in notFound) {
            sb.Append("\t");
            sb.AppendLine(s);
        }

        Debug.Log(sb.ToString());
    }


    string GetPathName(Transform t) {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(t.gameObject.name);
        while (t != activeGO.transform) {
            t = t.parent;
            if (t == null) {
                break;
            }
            sb.Insert(0,":");
            sb.Insert(0, t.gameObject.name);
        }
        return sb.ToString();
    }

}
