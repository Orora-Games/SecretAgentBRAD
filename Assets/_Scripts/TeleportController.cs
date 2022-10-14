using System.Collections.Generic;
using UnityEngine;

public class TeleportController : MonoBehaviour {
	[HideInInspector]
	public List<Transform> teleportLocations;
	[HideInInspector]
	public GameObject cameraContainer;

	// Start is called before the first frame update
	void Start () {
		cameraContainer = GameObject.Find( "CameraContainer" );
		foreach ( Transform child in transform ) {
			teleportLocations.Add( child.transform );
		}
	}

	#region Teleports
	/// <summary>
	/// Will porvide the next teleportTarget
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
	/// Takes currentLocation (transform object of teleport zone), and "PlayerObject"
	/// </summary>
	/// <param name="currentLocation"></param>
	/// <param name="playerObject"></param>
	public void Teleport(Transform currentLocation, GameObject playerObject ) {
		Transform nextLocation = NextLoaction(currentLocation);

		//Play animation (Dim Down camera)
		playerObject.transform.position = nextLocation.position;
		cameraContainer.transform.position = nextLocation.position;

	}
	#endregion
}
