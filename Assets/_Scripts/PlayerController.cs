using UnityEngine;
using static GameManager;

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
	private int defaultLayerMask;
	[SerializeField]
	private GameObject disguiseObject, frenchDisguise;
	private LevelManager levelManager;
	private int disguisesAvailable = 7, usedDisguises = 0;
	private bool disguised;
	public float disguiseHoldTimerMax = 1f;
	public float disguiseTimer;

	void Start () {
		controller = GetComponent<CharacterController>();

		//disguiseObject = gameObject.transform.Find( "Model" ).transform.Find( "CowBoy_Brad" ).transform.Find( "disguise" ).gameObject;

		if ( gameObject.tag == "Untagged" )
			Debug.LogError( "Your " + gameObject.name + " object needs to have the correct tag to be killable." ); // Make sure to Tag your player-object Player.
		if ( gameObject.layer == 0 )
			Debug.LogError( "Your " + gameObject.name + " object needs to have the correct layer to be detectable by the FieldOfView Controller." ); // Player-Layer will fix this issue.

		startHeight = gameObject.transform.position.y;
		defaultLayerMask = gameObject.layer;

		levelManager = GameObject.FindObjectOfType<LevelManager>();
		Disguised( false );

		if (!levelManager) {
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



		/* We went with True North Absolute controls. */
		Vector3 direction = new Vector3( horizontal, 0f, vertical ).normalized;

		/* This next one is supposed to move Player back to the ground if they manage to bug themselves to a floating position. */
		if ( Mathf.Abs( transform.position.y - startHeight ) >= 0.001f ) {
			transform.position = new Vector3( transform.position.x, startHeight, transform.position.z );
		}

		/* This block makes Player Turn the way they're moving. */
		if ( direction.magnitude >= 0.1f ) {
			Disguised(false);

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

		if (Input.GetKeyDown( KeyCode.R ) && !(GameManager.Instance.enemiesAlerted.Count > 0) ) {
			if (!levelManager ) {
				Debug.LogError("We do not have a level-manager, using default values.");
				bool disguiseCheck =  disguisesAvailable - usedDisguises > 0;

				Disguised( disguiseCheck );
				GameManager.Instance.UpdateDisguiseState( usedDisguises, disguisesAvailable );
			} else {
				levelManager.UpdateDisguiseNumbers();
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
			foreach ( Transform item in disguiseObject.transform ) {
				item.GetComponent<Renderer>().material.SetColor( "_Color", Color.black );
			}
			gameObject.layer = defaultLayerMask;
		}
	}
}
