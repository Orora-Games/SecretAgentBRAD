using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelController : MonoBehaviour
{
	/* It is used to contain the Exclamation object, to enable/disable when alerted. */
	private GameObject lock_obj;


	private void Start () {
		gameObject.GetComponent<Collider>().isTrigger = true;
		lock_obj = GameObject.Find("ExfilZone").transform.Find( "exit_lock" ).gameObject;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.tag == "Player")
		{
			if ( GameObject.FindGameObjectsWithTag( "Intel" ).Length-1 == 0) {

				lock_obj.SetActive( false );
			}

			/* TODO: Move to Level Manager */
			Destroy( gameObject );
		}

	}
}





