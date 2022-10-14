using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterBehaviour : MonoBehaviour {
	TeleportController teleportController;
	// Start is called before the first frame update
	void Start () {
		teleportController = gameObject.transform.parent.GetComponent<TeleportController>();
		gameObject.GetComponent<Collider>().isTrigger = true;
	}

	private void OnTriggerEnter ( Collider other ) {
		if ( teleportController != null ) {
			if ( other.transform.tag == "Player" ) {
				teleportController.Teleport( transform, other.gameObject );
			}
		}
	}
}
