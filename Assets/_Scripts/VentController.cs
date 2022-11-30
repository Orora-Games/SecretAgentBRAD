using System.Collections.Generic;
using UnityEngine;

public class VentController : MonoBehaviour {
	[HideInInspector]
	public List<Transform> movementDestinations;
	[HideInInspector]
	public GameObject cameraContainer;

	// Start is called before the first frame update
	void Start () {
		cameraContainer = GameObject.Find( "CameraContainer" );
		foreach ( Transform child in transform ) {
			movementDestinations.Add( child );
		}
	}

	#region Movement
	/// <summary>
	/// Will provide the next teleportTarget
	/// </summary>
	/// <param name="currentLocation"></param>
	/// <returns></returns>
	public Transform NextLoaction (Transform currentLocation) {
		Transform teleportTarget;
		int currentIndex = movementDestinations.IndexOf( currentLocation );
		currentIndex++;
		currentIndex = ( currentIndex == movementDestinations.Count ) ? 0 : currentIndex;

		teleportTarget = movementDestinations[ currentIndex ];
		return teleportTarget;
	}
	/// <summary>
	/// Takes currentLocation (transform object of gameObject), and "PlayerObject", moves the player there while making sure they're not interrupting the teleportation
	/// </summary>
	/// <param name="currentLocation"></param>
	/// <param name="playerObject"></param>
	public void MoveToLocation(Transform currentLocation, GameObject playerObject ) {
		Transform nextLocation = NextLoaction( currentLocation );
		InteractionHandler interactionHandler = nextLocation.GetComponent<InteractionHandler>();
		CharacterController charController = playerObject.GetComponent<CharacterController>();

		interactionHandler.disableEnterTrigger = true;
		interactionHandler.EnableInteraction(false, playerObject);

		//Play animation (Dim Down camera or something)
		charController.enabled = false;
		float playerHeight = playerObject.transform.position.y; // We're getting the player height here, because nextLocation is not the height we want our player at.
		playerObject.transform.position = new Vector3( nextLocation.position.x, playerHeight, nextLocation.position.z );
		cameraContainer.transform.position = nextLocation.position;
		charController.enabled = true;

	}
	#endregion
}
