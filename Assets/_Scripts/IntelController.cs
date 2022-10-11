using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelController : MonoBehaviour {
	/* It is used to contain the Exclamation object, to enable/disable when alerted. */
	private GameObject lock_obj;


	private void Start () {
		gameObject.GetComponent<Collider>().isTrigger = true;
	}

	private void OnTriggerEnter ( Collider other ) {
		if ( other.transform.tag == "Player") {
			if ( !GameManager.Instance )
				return;
			GameManager.Instance.PickedUpIntel( this.gameObject );
		}
	}
}
