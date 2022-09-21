using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;

	private GameObject previous;
	private GameObject next;

	private void Start () {
		/* Start by setting our camera to the target, wherever they are. */
		Vector3 targetPosition = target.position;

		transform.position = targetPosition;
		transform.position -= transform.forward * 5; /* Move the camera forwards to avoid clipping through the level */

		/* .. Continue by figuring out what camera is next and previous ...*/
		GameObject cameraContainer = transform.parent.gameObject; /* .. this is an inoptimal assumption. */
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
	void Update()
    {
		if ( Input.GetKeyDown( KeyCode.E ) ) {
			next.SetActive(true);
            gameObject.SetActive(false);
   		}
		if ( Input.GetKeyDown( KeyCode.Q ) ) {
			previous.SetActive( true );
			gameObject.SetActive( false );
		}

		/* Here we smoothly follow our target around. */
		/* Source: https://docs.unity3d.com/ScriptReference/Vector3.SmoothDamp.html */
		Vector3 targetPosition = target.position;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.position -= transform.forward * 5; /* Move the camera forwards to avoid clipping through the level */
    }
}

