using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelController : MonoBehaviour
{
	private void Start () {
		gameObject.GetComponent<Collider>().isTrigger = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.tag == "Player")
		{
			/* TODO: Move to Level Manager */
			Destroy( gameObject );
		}
	}
}
