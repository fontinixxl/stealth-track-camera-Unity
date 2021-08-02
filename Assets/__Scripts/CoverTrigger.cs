//#define DEBUG_CoverTrigger
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void CoverTriggerDelegate(CoverTrigger coverTrigger);

public class CoverTrigger : MonoBehaviour {
    // The coverNum describes the direction of this cover relative to the character.
    //  0 = Right, 1 = Forward, 2 = Left, 3 = Back, -1 = undefined
    //  This could also be automatically set based on the relative position of the CoverTrigger and the UnRotator.
    public int      coverNum = -1;
    public Collider coverColl = null;
	public CoverTriggerDelegate triggerHandler, triggerExitHandler;
    public SphereCollider sphereCollider;

	private void Awake()
	{
        sphereCollider = GetComponent<SphereCollider>();
	}

	private void OnTriggerEnter(Collider other)
    {
        #if DEBUG_CoverTrigger
        Debug.Log("CoverTrigger:OnTriggerEnter() - Cover "+other.name+" hit by CoverTrigger "+name);
        #endif
        InCover(other);
    }

    private void OnTriggerStay(Collider other)
    {
        #if DEBUG_CoverTrigger
        Debug.Log("CoverTrigger:OnTriggerStay() - Cover "+other.name+" hit by CoverTrigger "+name);
        #endif
        InCover(other);
    }

    void OnTriggerExit(Collider other)
    {
        #if DEBUG_CoverTrigger
        Debug.Log("CoverTrigger:OnTriggerExit() - Cover "+other.name+" hit by CoverTrigger "+name);
        #endif
        if (other == coverColl) {
            coverColl = null;
            if (triggerExitHandler != null) {
                triggerExitHandler(this);
            }
        }
    }

	void InCover(Collider other) {
        coverColl = other;
        if (triggerHandler != null) {
            triggerHandler(this);
        }
    }

}
