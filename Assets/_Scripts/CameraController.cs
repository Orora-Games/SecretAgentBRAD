using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public Transform target;
	public float smoothTime = 0.3F;
	private Vector3 velocity = Vector3.zero;
	
	/* cameraContainer is how we are getting all the cameras, it's set in the prefab. */
	public GameObject cameraContainer;
	public bool disableNextCamera = false;
	private GameObject previous;
	private GameObject next;
	public float movementRotation = 0f;
	public int cameraClippingHack = 5;

	private void Start () {
		/* This is here to allow us to check for targetPlayer not existing*/
		GameObject targetPlayer = GameObject.FindGameObjectWithTag( "Player" );

		target = ( !target && targetPlayer ) ? targetPlayer.transform : target;
		if ( target ) {
			/* Start by setting our camera to the target, wherever they are. */
			Vector3 targetPosition = target.position;

			transform.position = targetPosition;
			transform.position -= transform.forward * 5; /* Move the camera forwards to avoid clipping through the level */
		}
		/* .. Continue by figuring out what camera is next and previous ...*/
		cameraContainer = transform.parent.gameObject; /* .. this is an inoptimal assumption. */
		int childIndex = 0;
		List<Transform> childTransforms = new List<Transform>();

		foreach ( Transform child in cameraContainer.transform ) {
			childTransforms.Add( child );

			if ( child.name == gameObject.name ) {
				childIndex = child.GetSiblingIndex();
			}
		}
		previous = ( childIndex - 1 < 0 ) ? childTransforms[ childTransforms.Count - 1 ].gameObject : childTransforms[ childIndex - 1 ].gameObject;
		next = ( childIndex + 1 > childTransforms.Count - 1 ) ? childTransforms[ 0 ].gameObject : childTransforms[ childIndex + 1 ].gameObject;
	}

	// Update is called once per frame
	void Update () {
		target = ( !target ) ? GameObject.FindGameObjectWithTag( "Player" ).transform : target;
		GameObject targetObject = target.gameObject;

		/* Here we smoothly follow our target around. */
		/* Source: https://docs.unity3d.com/ScriptReference/Vector3.SmoothDamp.html */
		Vector3 targetPosition = target.position;

		transform.position = Vector3.SmoothDamp( transform.position, targetPosition, ref velocity, smoothTime );

		/* Move the camera forwards to avoid clipping through the level */
		transform.position -= transform.forward * cameraClippingHack; 

		/* If disableNextCamera is toggled, we do not run the code below this point. */
		if ( disableNextCamera ) return; 

		if ( Input.GetKeyDown( KeyCode.E ) ) {
			next.SetActive( true );
			/* The next line changes movementRotation on PlayerController */
			targetObject.GetComponent<PlayerController>().movementRotation = next.GetComponent<CameraController>().movementRotation;
			gameObject.SetActive( false );
		}
		if ( Input.GetKeyDown( KeyCode.Q ) ) {
			previous.SetActive( true );
			/* The next line changes movementRotation on PlayerController */
			targetObject.GetComponent<PlayerController>().movementRotation = previous.GetComponent<CameraController>().movementRotation;
			gameObject.SetActive( false );
		}
		
	}
}

