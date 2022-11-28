using System.Collections.Generic;
using UnityEngine;

public class VentController : MonoBehaviour {
	[HideInInspector]
	public List<Transform> teleportLocations;
	[HideInInspector]
	public GameObject cameraContainer;

	// Start is called before the first frame update
	void Start () {
		cameraContainer = GameObject.Find( "CameraContainer" );
		foreach ( Transform child in transform ) {
			teleportLocations.Add( child );
		}
	}

	#region Teleports
	/// <summary>
	/// Will provide the next teleportTarget
	/// </summary>
	/// <param name="currentLocation"></param>
	/// <returns></returns>
	public Transform NextLoaction (Transform currentLocation) {
		Transform teleportTarget;
		int currentIndex = teleportLocations.IndexOf( currentLocation );
		currentIndex++;
		currentIndex = ( currentIndex == teleportLocations.Count ) ? 0 : currentIndex;

		teleportTarget = teleportLocations[ currentIndex ];
		return teleportTarget;
	}
	/// <summary>
	/// Takes currentLocation (transform object of teleport zone), and "PlayerObject", moves the player there while making sure they're not interrupting the teleportation
	/// </summary>
	/// <param name="currentLocation"></param>
	/// <param name="playerObject"></param>
	public void Teleport(Transform currentLocation, GameObject playerObject ) {
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
