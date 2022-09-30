using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	private class CameraLocation {
		public float movementRotation = 0f;
		public Camera camera;

		public void ActivateCamera () {
			camera.gameObject.SetActive( true );
		}

		public void DeactivateCamera () {
			camera.gameObject.SetActive( false );
		}
	}

	private PlayerController playerController;
	private List<CameraLocation> cameraLocations = new List<CameraLocation>();
	private CameraLocation currentCamera;

	public Transform overrideTarget;
	public float smoothTime = 0.3f;
	private Vector3 velocity = Vector3.zero;
	
	/* cameraContainer is how we are getting all the cameras, it's set in the prefab. */
	public bool disableCameraSwap = false;

	private Transform Target {
		get {
			if (!playerController ) {
				playerController = FindObjectOfType<PlayerController>();
			}
			if ( !overrideTarget )
				overrideTarget = null;

			return overrideTarget ?? ( playerController ? playerController.transform : null);
		}
	}

	private void Start () {
		/* This is here to allow us to check for targetPlayer not existing*/
		if ( Target ) {
			/* Start by setting our camera to the target, wherever they are. */
			transform.position = Target.position;
		}

		/* .. Continue by figuring out what camera is next and previous ...*/

		foreach ( Camera child in transform.GetComponentsInChildren<Camera>() ) {
			var newCamera = new CameraLocation {
				camera = child,
				movementRotation = child.transform.localRotation.eulerAngles.y + 45f
			};

			newCamera.DeactivateCamera();

			cameraLocations.Add( newCamera );
		}

		currentCamera = cameraLocations[ 0 ];
		currentCamera.ActivateCamera();
	}

	// Update is called once per frame
	void Update () {
		/* Here we smoothly follow our target around. */
		/* Source: https://docs.unity3d.com/ScriptReference/Vector3.SmoothDamp.html */
		if ( !Target )
			return;

		transform.position = Vector3.SmoothDamp( transform.position, Target.position, ref velocity, smoothTime );

		/* If disableNextCamera is toggled, we do not run the code below this point. */
		if ( disableCameraSwap ) return; 

		if ( Input.GetKeyDown( KeyCode.E ) ) {
			SwapCamera( false );
		}
		if ( Input.GetKeyDown( KeyCode.Q ) ) {
			SwapCamera( true );
		}
	}

	private void SwapCamera(bool previous) {
		int movement = previous ? -1 : 1;
		int currentCameraIndex = cameraLocations.IndexOf( currentCamera );

		// If it goes minus one, it will go to Count - 1, and if goes to Count, it will go back to Zero
		int nextIndex = ((currentCameraIndex + movement) + cameraLocations.Count) % cameraLocations.Count;

		currentCamera.DeactivateCamera();
		currentCamera = cameraLocations[ nextIndex ];
		currentCamera.ActivateCamera();
		playerController.movementRotation = currentCamera.movementRotation;
	}
}

