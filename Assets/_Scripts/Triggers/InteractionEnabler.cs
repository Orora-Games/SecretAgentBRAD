using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionEnabler : MonoBehaviour {
	[SerializeField]
	private InteractionHandler interactionHandler;
	[HideInInspector]
	public bool interactionDone = false;

	// Start is called before the first frame update
	void Start () {
		gameObject.GetComponent<Collider>().isTrigger = true;
	}

	private void OnTriggerEnter ( Collider other ) {
		if ( interactionHandler == null ||
			interactionHandler.disableEnterTrigger || 
			other.transform.tag != "Player" || 
			interactionDone ) return;

		interactionHandler.EnableInteraction(true, other.gameObject);
	}

	private void OnTriggerExit ( Collider other ) {
		if ( other.transform.tag != "Player" ) return; 
		
		interactionDone = false;
		interactionHandler.disableEnterTrigger = false;
		interactionHandler.EnableInteraction(false, other.gameObject );
	}
}
