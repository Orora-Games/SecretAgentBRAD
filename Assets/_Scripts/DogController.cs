using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogController : MonoBehaviour
{
    //Alerted State
    public bool alerted = false;
    public float alertedTimer = 5.0f;

    /* Movement turn smoothing*/
    public float turnSmoothTime = 0.5f;

    /* How long will the bots stay alerted? Is set to alertedTimer, used to restore the timer after use. */
    private float defaultAlertedTimer;

    /* Used when checking if an entity that has eyes on target has alerted everyone to their position. */
    private bool hasAlerted;

    /* Used to alert everyone when Player leaves their eyesOnTarget sight. */
    private bool hadEyesOnTarget;

    /* Used to contain the Exclamation object, to enable/disable when alerted. */
    private GameObject exclamation;

    /* Get the FieldOfView game object. */
    private GameObject fieldOfView;

    //Materials
    private Material visionAlertedMaterial;
    private Material visionUndetectedMaterial;
    private Material visionDetectedMaterial;

    private Vector3 targetPosition;
    private Vector3 startPosition;
    private Vector3 lookRotationVector;
    private Quaternion spawnRotation;

	public UnityEngine.AI.NavMeshAgent agent;
    private Transform lastTarget;

    // These are the objects that need to be alerted when a Player has been detected.
    private GameObject[] toBeAlerted;

    // Start is called before the first frame update
    void Start()
    {
        defaultAlertedTimer = alertedTimer;
        startPosition = transform.position;
        spawnRotation = transform.rotation;
        exclamation = gameObject.transform.Find( "Exclamation" ).gameObject;
        fieldOfView = gameObject.transform.Find("ViewVisualization").gameObject;
        visionAlertedMaterial = gameObject.GetComponent<FieldOfView>().alertedMaterial;
        visionUndetectedMaterial = gameObject.GetComponent<FieldOfView>().undetectedMaterial;
        visionDetectedMaterial = gameObject.GetComponent<FieldOfView>().detectedMaterial;
        toBeAlerted = GameObject.FindGameObjectsWithTag("Enemy");
    }

    // Update is called once per frame
    void Update()
    {
        /* Get the list of visible targets from FieldOfView component ..*/
        List<Transform> visibleTargets = GetComponent<FieldOfView>().visibleTargets;

        /* .. if this Entity (Enemy) sees Player (target) ... */
        if (visibleTargets.Count > 0)
        {
            /* .. Set the alerted-timer to the default alerted timer .. */
            alertedTimer = defaultAlertedTimer;

            foreach (var target in visibleTargets)
            {
                lastTarget = target;
                /* .. Set Exclamation to Active ... */
                exclamation.SetActive(true);

                /* .. alerted-check is used when Enemies are no longer spotting Player  ... */
                this.alerted = true;

                /* .. When the Enemy spots the Player they are supposed to alert everyone to their location ... */
                if (!hasAlerted)
                {
                    /* .. Tell every Enemy where the Player was spotted ... */
                    this.AlertEveryone(target);
                    /* .. set the hasAlerted state, this way the spotting Enemy only alerts once per spot ... */
                    this.hasAlerted = true;
                }
                /* hadEyesOnTarget is used when Player leaves the Enemy Field of view to trigger the "they just ran away here"-alert */
                this.hadEyesOnTarget = true;

                /* Start by becoming angry (change fov to red) ... */
                this.fieldOfView.GetComponent<Renderer>().material = visionDetectedMaterial;

                /* .. Get our target position ... */
                targetPosition = target.position;
                /* .. Tell AI movement to move to target location ... */
                agent.SetDestination(targetPosition);
                /* .. Find out what direction to look in ... */
                Vector3 direction = (targetPosition - transform.position).normalized;
                /* .. Convert that to a Vector ... */
                lookRotationVector = new Vector3(direction.x, 0, direction.z);

                /* .. When the Enemy hasn't changed where he looks, we get a zero vector ... */
                if (lookRotationVector != Vector3.zero)
                {
                    /* TODO: figure out what Quaternion.LookRotation does exactly. */
                    Quaternion lookRotation = Quaternion.LookRotation(lookRotationVector);

                    /* TODO: figure out what Quaternion.Slerp does exactly. */
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
                }
            }

            return;
        }

        if (hadEyesOnTarget)
        {
            this.AlertEveryone(lastTarget);
            this.hasAlerted = false;
            this.hadEyesOnTarget = false;
        }

        if (alerted && alertedTimer > 0f ) {
            /* .. countdown till when the bots stop being alerted ... */
            alertedTimer -= Time.deltaTime;
            
            /* .. Set Exclamation mark to Active ... */
            exclamation.SetActive(true);

            /* .. turnTowardNavSteeringTarget is the navigation mesh agent's target ... */
            var turnTowardNavSteeringTarget = agent.steeringTarget;
            /* .. Calculate a direction by subtracting the transform.position from steeringTarget ... */
            Vector3 direction = (turnTowardNavSteeringTarget - transform.position).normalized;

            lookRotationVector = new Vector3(direction.x, 0, direction.z);

            if (lookRotationVector != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookRotationVector);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
            }
        } else {
            /* beneath this line resets the DogController and VisionLine's materials */
            this.fieldOfView.GetComponent<Renderer>().material = visionUndetectedMaterial;

			/* .. set alerted to false, as we are no longer alerted ... */
            this.alerted = false;

            /* .. No more exclamation point! ... */
            exclamation.SetActive(false);

            /* .. Set the alerted-timer to the default alerted timer .. */
            alertedTimer = defaultAlertedTimer;

            /* .. Tell AI movement to move to startPosition ... */
            agent.SetDestination(startPosition);
        }

        /* Checks that the angle between current and spawnRotation is over 2f, and checks that current and start position is less than 2 meters from each other */
		if ( Quaternion.Angle( transform.rotation, spawnRotation ) > 2f  && (transform.position - startPosition ).magnitude < 2) {
            /* .. we're now resetting the rotation ... */
            transform.Rotate(new Vector3(spawnRotation.eulerAngles.x, spawnRotation.eulerAngles.y,spawnRotation.eulerAngles.z) * Time.deltaTime * turnSmoothTime );
		}
	}
    public void BecomeAlerted (Transform target) {
        /* agent.SetDestination / Navigation Mesh code/setup
         *   Source: How to use Unity NavMesh Pathfinding! (Unity Tutorial) https://www.youtube.com/watch?v=atCOd4o7tG4 - Code Monkey (youtube)    */

        /* Activate Alerted state ... */
        this.alerted = true;
        /* .. Set Exclamation mark to Active ... */
        exclamation.SetActive(true);
        /* .. Tell AI movement to move to target location ... */
        agent.SetDestination(target.position);
        /* .. Set the alerted-timer to the default alerted timer .. */
        alertedTimer = defaultAlertedTimer;
        /* .. Trigger the red vision-indicator. */
		this.fieldOfView.GetComponent<Renderer>().material = visionAlertedMaterial;
	}
  
    void AlertEveryone(Transform target, bool exitTriggered = false)
    { //exitTriggered is used to skip detectingVision-check.
        for (int i = 0; i < toBeAlerted.Length; i++)
        {
            /* Makes sure the detecting vision cone stays red. (detectingVision) */
            if (!exitTriggered && toBeAlerted[i].gameObject.GetInstanceID() == gameObject.GetInstanceID())
                continue;

            /* Alerts all other GameObjects with Enemy tag. */
            this.toBeAlerted[i].GetComponent<DogController>().BecomeAlerted(target);
        }
    }
}
