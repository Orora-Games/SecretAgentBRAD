using UnityEngine;

public class PlayerController : MonoBehaviour {
	public float speed = 6f;
	public float turnSmoothTime = 0.1f;

	[Header( "Controls" )]
	public bool testAbsoluteControls = false;
	[HideInInspector]
	public float movementRotation = 0f;

	private float startHeight; 
	private float turnSmoothVelocity;
	private CharacterController controller;
	private bool planningMode = false;
	private int defaultLayerMask;
	private GameObject hatObject;

	void Start () {
		controller = GetComponent<CharacterController>();

		hatObject = gameObject.transform.Find( "BRAD_improved" ).transform.Find( "hat" ).gameObject;
		if ( gameObject.tag == "Untagged" )
			Debug.LogError( "Your " + gameObject.name + " object needs to have the correct tag to be killable." ); // Make sure to Tag your player-object Player.
		if ( gameObject.layer == 0 )
			Debug.LogError( "Your " + gameObject.name + " object needs to have the correct layer to be detectable by the FieldOfView Controller." ); // Player-Layer will fix this issue.

		startHeight = gameObject.transform.position.y;
		defaultLayerMask = gameObject.layer;

		Disguised( planningMode );
	}

	// Update is called once per frame
	void Update () {
		float horizontal = Input.GetAxisRaw( "Horizontal" );
		float vertical = Input.GetAxisRaw( "Vertical" );

		/* We went with True North Absolute controls. */
		Vector3 direction = new Vector3( horizontal, 0f, vertical ).normalized;

		/* This next one is supposed to move Player back to the ground if they manage to bug themselves to a floating position. */
		if ( Mathf.Abs( transform.position.y - startHeight ) >= 0.001f ) {
			transform.position = new Vector3( transform.position.x, startHeight, transform.position.z );
		}
		/* This block makes Player Turn the way they're moving. */
		if ( direction.magnitude >= 0.1f ) {
			/* doubleStraightControls checks if the mapmaker wants to force people to press two buttons to go down or up a hallway. */
			float doubleStraightControls = (testAbsoluteControls) ? 0f : 45f;

			/* movementRotation contains the rotation angle the cameras send the player Object, this is where we rotate the player's movement to make it "straight". 
				*   Source: http://answers.unity.com/answers/46772/view.html */
			direction = Quaternion.AngleAxis( movementRotation - doubleStraightControls, Vector3.up ) * direction;

			/* Next comes smooth rotation stuff. */
			float targetAngle = Mathf.Atan2( direction.x, direction.z ) * Mathf.Rad2Deg;

			/* "turnSmoothTime/4000" here comes from  having a 0.1 ratio before, and our current number being 400 */
			float angle = Mathf.SmoothDampAngle( transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, (turnSmoothTime/4000) );
			transform.rotation = Quaternion.Euler( 0f, angle, 0f );

			/* Time to go move the character in our calculated direction. */
			controller.Move( direction * speed * Time.deltaTime );
		}
	}

	public void Disguised ( bool disguised, bool fromGameManager = false) {
		if ( !GameManager.Instance ) { 
			Debug.LogError("To enable disguise, make sure you have a GameManager.");
			return;
		}

		if ( disguised ) {
			gameObject.layer = 0;
			hatObject.SetActive(false);
			GameManager.Instance.DisguisePlayer( gameObject.GetComponent<PlayerController>(), disguised );
		} else {
			hatObject.SetActive(true);
			hatObject.GetComponent<Renderer>().material.SetColor( "_Color", Color.black );
			gameObject.layer = defaultLayerMask;
		}
	}
}
