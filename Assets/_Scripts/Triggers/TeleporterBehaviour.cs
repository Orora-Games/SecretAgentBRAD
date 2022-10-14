using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterBehaviour : MonoBehaviour {
	[HideInInspector]
	private TeleportController teleportController;
	[HideInInspector]
	public bool disableTeleport;

	// Start is called before the first frame update
	void Start () {
		teleportController = gameObject.transform.parent.GetComponent<TeleportController>();
		gameObject.GetComponent<Collider>().isTrigger = true;
	}

	private void OnTriggerEnter ( Collider other ) {
		if ( teleportController != null ) {
			if ( other.transform.tag == "Player" && !disableTeleport) {
				teleportController.Teleport( transform, other.gameObject );
			}
		}
	}
	private void OnTriggerExit ( Collider other ) {
		if ( other.transform.tag == "Player" && disableTeleport ) {
			disableTeleport = false;
		}
	}
}
