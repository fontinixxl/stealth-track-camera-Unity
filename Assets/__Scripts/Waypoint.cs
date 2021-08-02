using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {
    public float    waitTime = 1;

	private void Awake()
	{
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in rends)
        {
            rend.enabled = false;
        }
	}

	public Vector3  pos {
        get {
            return transform.position;
        }
    }

    public Vector3 fwd {
        get { 
            return transform.forward;
        }
    }

    public Vector3 lookAt {
        get {
            return (transform.position + transform.forward);
        }
    }

    public Quaternion rot {
        get {
            return (transform.rotation);
        }
    }
}
