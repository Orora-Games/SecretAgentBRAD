using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisguiseZone : MonoBehaviour {
	private void OnTriggerEnter ( Collider other ) {
		if ( other.transform.tag == "Player" ) {
			other.GetComponent<PlayerController>().Disguised(true);
		}
	}
}
