using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//[RequireComponent( typeof( NavMeshAgent ) )]
public class EnemyController : MonoBehaviour {
	//Alerted State
	public bool alerted = false;
	private bool wasAlerted = false;
	public float alertedTimer = 5.0f;

	/* Movement turn smoothing*/
	public float turnSmoothTime = 0.5f;

	/* How long will the bots stay alerted? Is set to alertedTimer, used to restore the timer after use. */
	private float currentAlertTime;

	/* It is used when checking if an entity that has eyes on target has alerted everyone to their position. */
	private bool hasAlerted;

	/* It is used to alert everyone when Player leaves their eyesOnTarget sight. */
	private bool hadEyesOnTarget;

	/* It is used to contain the Exclamation object, to enable/disable when alerted. */
	private GameObject exclamation;

	/* Get the FieldOfView game object. */
	private ColoredFieldOfView fieldOfView;

	/* Our patrol waypoints. Source: https://youtu.be/c8Nq19gkNfs */
	public Transform waypointContainer;
	private List<Transform> waypoints = new List<Transform>();

	private int waypointIndex;
	public Transform waypointTarget;
	public float waypointWaitTime = 5.0f;
	private float defaultWaypointWaitTime;

	private Vector3 targetPosition;
	private Vector3 startPosition;
	private Vector3 lookRotationVector;
	private Quaternion startRotation;

	private NavMeshAgent agent;

	private Transform lastTarget;

	// These are the objects that need to be alerted when a Player has been detected.
	private GameObject[] toBeAlerted;
	private LevelManager levelManager;

	// Start is called before the first frame update
	void Start () {
		agent = GetComponent<NavMeshAgent>();
		currentAlertTime = 0;
		startPosition = transform.position;
		startRotation = transform.rotation;
		exclamation = gameObject.transform.Find( "Exclamation" ).gameObject;
		fieldOfView = GetComponent<ColoredFieldOfView>();

		toBeAlerted = GameObject.FindGameObjectsWithTag( "Enemy" );

		/* Initializes waypoint system, and gets next waypoint */
		Transform nextTarget = InitializeWaypoints();

		/* .. Tell AI movement to move to the next waypointTarget ... */
		if ( agent ) { 
			agent.SetDestination( nextTarget.position );
		}
	}

	// Update is called once per frame
	void Update () {
		/* Get the list of visible targets from FieldOfView component ..*/
		List<Transform> visibleTargets = fieldOfView.visibleTargets;

		/* .. if this Entity (Enemy) sees Player (target) ... */
		if ( visibleTargets.Count > 0 ) {
			/* .. Set the alerted-timer to the default alerted timer .. */
			currentAlertTime = 0;

			foreach ( var target in visibleTargets ) {
				lastTarget = target;
				/* .. Set Exclamation to Active ... */
				exclamation.SetActive( true );

				/* .. alerted-check is used when Enemies are no longer spotting Player  ... */
				alerted = true;

				UpdateGameManagerAlertedList();

				/* .. When the Enemy spots the Player they are supposed to alert everyone to their location ... */
				if ( !hasAlerted ) {
					/* .. Tell every Enemy where the Player was spotted ... */
					AlertEveryone( target );
					/* .. set the hasAlerted state, this way the spotting Enemy only alerts once per spot ... */
					hasAlerted = true;
				}
				/* hadEyesOnTarget is used when Player leaves the Enemy Field of view to trigger the "they just ran away here"-alert */
				hadEyesOnTarget = true;
				wasAlerted = true;

				/* Start by becoming angry (change fov to red) ... */
				fieldOfView.ChangeColorState( ColoredFieldOfView.FieldOfViewStates.Detected );

				/* .. Get our target position ... */
				targetPosition = target.position;
				/* .. Tell AI movement to move to the next waypointTarget ... */
				if ( agent ) {
					agent.SetDestination( targetPosition );
				}
				/* .. Find out what direction to look in ... */
				Vector3 direction = ( targetPosition - transform.position ).normalized;
				/* .. Convert that to a Vector ... */
				lookRotationVector = new Vector3( direction.x, 0, direction.z );

				/* .. When the Enemy hasn't changed where he looks, we get a zero vector ... */
				if ( lookRotationVector != Vector3.zero ) {
					/* TODO: figure out what Quaternion.LookRotation does exactly. */
					Quaternion lookRotation = Quaternion.LookRotation( lookRotationVector );

					/* TODO: figure out what Quaternion.Slerp does exactly. */
					transform.rotation = Quaternion.Slerp( transform.rotation, lookRotation, Time.deltaTime * 5 );
				}
			}

			return;
		}

		if ( hadEyesOnTarget ) {
			AlertEveryone( lastTarget, true );
			hadEyesOnTarget = false;
			hasAlerted = false;
		}

		if ( alerted && currentAlertTime < alertedTimer) {
			/* .. countdown till when the bots stop being alerted ... */
			currentAlertTime += Time.deltaTime;

			/* .. Set Exclamation mark to Active ... */
			exclamation.SetActive( true );

			/* .. turnTowardNavSteeringTarget is the navigation mesh agent's target ... */
			if ( agent ) {
				Vector3 turnTowardNavSteeringTarget = agent.steeringTarget;
			
				/* .. Calculate a direction by subtracting the transform.position from steeringTarget ... */
				Vector3 direction = ( turnTowardNavSteeringTarget - transform.position ).normalized;
				lookRotationVector = new Vector3( direction.x, 0, direction.z );
			}


			if ( lookRotationVector != Vector3.zero ) {
				Quaternion lookRotation = Quaternion.LookRotation( lookRotationVector );
				transform.rotation = Quaternion.Slerp( transform.rotation, lookRotation, Time.deltaTime * 5 );
			}
		} else {
			if ( wasAlerted ) {
				wasAlertedReset();
			}

			/* This angle-check can be less stringent that way we start counting down a little earlier. */
			if ( Quaternion.Angle( transform.rotation, waypointTarget.rotation ) <= 2f && ( transform.position - waypointTarget.position ).magnitude <= 2f ) {
				waypointWaitTime -= Time.deltaTime;
			}

			if ( waypointWaitTime < 0f ) {
				/* Get the next wayPointTarget */
				Transform nextTarget = NextWaypoint();

				/* .. Tell AI movement to move to the next waypointTarget ... */
				if ( agent ) {
					agent.SetDestination( nextTarget.position );
				}
				waypointWaitTime = defaultWaypointWaitTime;
			}
		}


		/* Checks that the angle between current and waypointTarget is over 2f, and checks that current and waypoint position is less than 2 meters from each other */
		if ( Quaternion.Angle( transform.rotation, waypointTarget.rotation ) > 2f && ( transform.position - waypointTarget.position ).magnitude < 2 ) {

			/* .. to reset the rotation, we start by calculating what direction is the shortest direction to turn.*/
			transform.rotation = Quaternion.Lerp( transform.rotation, waypointTarget.rotation, Time.deltaTime * turnSmoothTime );
		}
	}

	private void UpdateGameManagerAlertedList () {
		if ( GameManager.Instance && !GameManager.Instance.enemiesAlerted.Contains( gameObject ) ) {
			GameManager.Instance.enemiesAlerted.Add( gameObject );
		}
	}

	private void wasAlertedReset () {
		wasAlerted = false;
		/* .. beneath this line resets the EnemyController and VisionLine's materials ... */
		fieldOfView.ChangeColorState( ColoredFieldOfView.FieldOfViewStates.Undetected );

		/* .. set alerted to false, as we are no longer alerted ... */
		this.alerted = false;

		/* .. No more exclamation point! ... */
		exclamation.SetActive( false );

		/* .. Set the alerted-timer to the default alerted timer .. */
		currentAlertTime = 0;

		if ( GameManager.Instance ) {
			GameManager.Instance.enemiesAlerted.Remove( gameObject );
		}

		/* .. Set the waypointTimer to the default waypoint timer .. */
		waypointWaitTime = defaultWaypointWaitTime;

		
		/* .. Check if we have an navmesh agent, if not don't try to move the enemy... */
		if ( agent ) {
			/* .. Send Bot back to their previous waypoint .. */
			agent.SetDestination( waypointTarget.position);
		}
	}

	public void BecomeAlerted ( Transform target ) {
		/* agent.SetDestination / Navigation Mesh code/setup
		 *   Source: How to use Unity NavMesh Pathfinding! (Unity Tutorial) https://www.youtube.com/watch?v=atCOd4o7tG4 - Code Monkey (youtube)    */

		/* Activate Alerted state ... */
		alerted = true;
		/* .. Activeate WasAlerted-state (used for resetting values) ... */
		wasAlerted = true;
		/* .. Set Exclamation mark to Active ... */
		exclamation.SetActive( true );
		
		/* .. Check if we have an navmesh agent, if not don't try to move the enemy. */
		if ( agent ) { 
		/* .. Tell AI movement to move to target location ... */
			agent.SetDestination( target.position );
		}
		/* .. Set the alerted-timer to the default alerted timer .. */
		currentAlertTime = 0;

		UpdateGameManagerAlertedList();

		/* .. Trigger the yellow vision-indicator. */
		fieldOfView.ChangeColorState( ColoredFieldOfView.FieldOfViewStates.Alerted );
	}

	void AlertEveryone ( Transform target, bool exitTriggered = false ) { //exitTriggered is used to skip detectingVision-check.
		for ( int i = 0; i < toBeAlerted.Length; i++ ) {
			/* Makes sure the detecting vision cone stays red. (detectingVision) */
			if ( !exitTriggered && toBeAlerted[ i ].gameObject.GetInstanceID() == gameObject.GetInstanceID() )
				continue;

			/* Alerts all other GameObjects with Enemy tag. */
			this.toBeAlerted[ i ].GetComponent<EnemyController>().BecomeAlerted( target );
		}
	}

	#region WayPoints
	 private Transform InitializeWaypoints ( ) {
		/* .. Set the default wait timer for waypointWaitTimer ... */
		defaultWaypointWaitTime = waypointWaitTime;

		if ( waypointContainer ) {
			foreach ( Transform child in waypointContainer.transform ) {
				waypoints.Add( child );
			}
		}
		/* .. If there are no waypoints, set one ... */
		if ( waypoints.Count == 0 ) {
			GameObject newWayPoint = new GameObject();
			newWayPoint.transform.position = startPosition;
			newWayPoint.transform.rotation = startRotation;
			waypoints.Add( newWayPoint.transform );
		}

		/* .. Set/return the next waypoint ... */
		return NextWaypoint();
	}

	private Transform NextWaypoint () {
		waypointIndex = ( waypointIndex == waypoints.Count ) ? 0 : waypointIndex;
		waypointTarget = waypoints[ waypointIndex ];
		waypointIndex++;
		return waypointTarget;
	}

	#endregion
}
