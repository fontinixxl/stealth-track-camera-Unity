using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCollisionNormal : MonoBehaviour {

    public ContactPoint[]     contacts;

	private void Start()
	{
        contacts = new ContactPoint[0];
	}

	private void OnCollisionEnter(Collision collision)
	{
        OnCollisionStay(collision);
	}

	private void OnCollisionStay(Collision collision)
	{
		// Find the ContactPoints
        contacts = collision.contacts;
	}

	private void OnCollisionExit(Collision collision)
	{
        contacts = new ContactPoint[0];
	}

	private void OnDrawGizmos()
	{
        if (!Application.isEditor || !Application.isPlaying || contacts.Length == 0) {
            return;
        }

		Gizmos.color = Color.yellow;
        for (int i=0; i<contacts.Length; i++) {
            Gizmos.DrawWireSphere(contacts[i].point, 0.1f);
            Gizmos.DrawRay(contacts[i].point, contacts[i].normal);
        }
	}

}
