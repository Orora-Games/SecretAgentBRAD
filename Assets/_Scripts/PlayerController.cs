using UnityEngine;
using static GameManager;

public class PlayerController : MonoBehaviour {
	public float speed = 6f;
	public float turnSmoothTime = 0.1f;

	[HideInInspector]
	public float movementRotation = 0f;

	private float startHeight; 
	private float turnSmoothVelocity;
	private CharacterController controller;
	private int defaultLayerMask;
	[SerializeField]
	private GameObject disguiseObject, frenchDisguise;
	private LevelManager levelManager;
	private int disguisesAvailable = 7, usedDisguises = 0;
	private bool disguised;
	public float disguiseHoldTimerMax = 1f;
	public float disguiseTimer;

	void Start () {
		SetTransformHeightFloor( transform, 0.05f );
		
		controller = GetComponent<CharacterController>();

		//disguiseObject = gameObject.transform.Find( "Model" ).transform.Find( "CowBoy_Brad" ).transform.Find( "disguise" ).gameObject;

		if ( gameObject.tag == "Untagged" ) { 
			Debug.LogError( "Your " + gameObject.name + " object needs to have the correct tag to be killable." ); // Make sure to Tag your player-object Player.
		}
		if ( gameObject.layer == 0 ) { 
			Debug.LogError( "Your " + gameObject.name + " object needs to have the correct layer to be detectable by the FieldOfView Controller." ); // Player-Layer will fix this issue.
		}
		startHeight = gameObject.transform.position.y;
		defaultLayerMask = gameObject.layer;

		levelManager = GameObject.FindObjectOfType<LevelManager>();
		Disguised( false );

		if (!levelManager) {
			if ( !GameManager.Instance ) return;
			GameManager.Instance.UpdateDisguiseState( usedDisguises, disguisesAvailable );
		} else {
			levelManager.UpdateDisguiseNumbers();
		}
	}

	// Update is called once per frame
	void Update () {
		if ( !GameManager.Instance || GameManager.Instance.GetGameState() != GameState.Playing ) {
			return; 
		}
		if (Input.GetKeyDown(KeyCode.F)) {
			Disguised(false);
		}

		if ( disguised ) {
			disguiseTimer += Time.deltaTime;
			if ( disguiseTimer < disguiseHoldTimerMax ) {
				return;
			}
		}
		
		if ( !levelManager ) {
			levelManager = GameObject.FindObjectOfType<LevelManager>();
		}

		float horizontal = Input.GetAxisRaw( "Horizontal" );
		float vertical = Input.GetAxisRaw( "Vertical" );

		Vector3 direction = new Vector3( horizontal, 0f, vertical ).normalized;

		/* This next one is supposed to move Player back to the ground if they manage to bug themselves to a floating position. */
		if ( Mathf.Abs( transform.position.y - startHeight ) >= 0.001f ) {
			transform.position = new Vector3( transform.position.x, startHeight, transform.position.z ); // Move player position
		}

		/* This block makes Player Turn the way they're moving. */
		if ( (horizontal != 0f || vertical != 0f) &&  direction.magnitude >= 0.1f ) {
			Disguised(false);

			/* Rotate controlls 45 degrees to make up be north. */
			float controlRotation = 45f;

			/* movementRotation contains the rotation angle the cameras send the player Object, this is where we rotate the player's movement to make it "straight". 
				*   Source: http://answers.unity.com/answers/46772/view.html */
			direction = Quaternion.AngleAxis( movementRotation - controlRotation, Vector3.up ) * direction;

			/* Next comes smooth rotation stuff. */
			float targetAngle = Mathf.Atan2( direction.x, direction.z ) * Mathf.Rad2Deg;

			/* "turnSmoothTime/4000" here comes from  having a 0.1 ratio before, and our current number being 400 */
			float angle = Mathf.SmoothDampAngle( transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, (turnSmoothTime/4000) );
			transform.rotation = Quaternion.Euler( 0f, angle, 0f );

			/* Time to go move the character in our calculated direction. */
			controller.Move( direction * speed * Time.deltaTime );
		}

		if (Input.GetKeyDown( KeyCode.R ) && !disguised && !Input.GetKey( KeyCode.F ) && !(GameManager.Instance.enemiesAlerted.Count > 0) ) {
			if (!levelManager ) {
				Debug.LogError("We do not have a level-manager, using default values.");
				bool disguiseCheck =  disguisesAvailable - usedDisguises > 0;

				Disguised( disguiseCheck );
				GameManager.Instance.UpdateDisguiseState( usedDisguises, disguisesAvailable );
			} else {
				Disguised(levelManager.DisguiseCheck());
			}

			usedDisguises++;
		}
	}
	/// <summary>
	/// Changes players Layermask to Default, allowing them to hide from being seen.
	/// </summary>
	/// <param name="disguisedLocal"></param>
	public void Disguised ( bool disguisedLocal ) {
		disguised = disguisedLocal;

		if ( GameManager.Instance ) {
			GameManager.Instance.DisguisePlayer( disguisedLocal );
		}

		if ( disguisedLocal ) {
			gameObject.layer = 0;
			disguiseObject.SetActive(false);
			disguiseTimer = 0f;
		} else {
			disguiseObject.SetActive(true);
			//foreach ( Transform item in disguiseObject.transform ) {
			//	item.GetComponent<Renderer>().material.SetColor( "_Color", Color.black );
			//}
			gameObject.layer = defaultLayerMask;
		}
	}
	public void SetTransformHeightFloor ( Transform transformToHeightAdjust, float heightAdjustment = 0.05f ) {
		/* Find floor, so we can use floor.transform.position.y to find the floor height, then set ViewVisualization to floorHeight+some 
			*	Example: https://docs.unity3d.com/ScriptReference/RaycastHit-distance.html */
		RaycastHit hit;

		if ( Physics.Raycast( transformToHeightAdjust.position, Vector3.down, out hit, Mathf.Infinity, 1 ) ) {
			if ( hit.transform.name == "Floor" ) {
				float adjustedFloorHeight = 0f;
				adjustedFloorHeight = hit.point.y + heightAdjustment;

				transformToHeightAdjust.position = new Vector3( transformToHeightAdjust.position.x, adjustedFloorHeight, transformToHeightAdjust.position.z );
			}
		}
	}
}
