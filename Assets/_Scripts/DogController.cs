using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogController : MonoBehaviour
{
    //Alerted State
    public bool alerted = false;
    public float alertedTimer = 5.0f;
    public bool eyesOnTarget = false;
    public float turnSmoothTime = 0.1f;
    private Vector3 targetPosition;
    private GameObject exclamation;
    private GameObject visionLine;
    private Vector3 startPosition;
    private Vector3 lookRotationVector;
    private Quaternion spawnRotation;
    public UnityEngine.AI.NavMeshAgent agent;

    //Materials
    private Material visionAlertedMaterial;
    private Material visionUndetectedMaterial;

	// Start is called before the first frame update
	void Start()
    {
        startPosition = transform.position;
        spawnRotation = transform.rotation;
        exclamation = gameObject.transform.Find( "Exclamation" ).gameObject;
		visionLine = gameObject.transform.Find( "VisionLine" ).gameObject;
		visionAlertedMaterial = visionLine.GetComponent<VisionController>().alertedMaterial;
		visionUndetectedMaterial = visionLine.GetComponent<VisionController>().undetectedMaterial;
	}

    // Update is called once per frame
    void Update()
    {
        if (alerted && alertedTimer > 0f ) {
            alertedTimer = alertedTimer - Time.deltaTime;
			exclamation.SetActive(true);

            if (eyesOnTarget)
            {
                EyesOnTarget();
                return;
            }
            var turnTowardNavSteeringTarget = agent.steeringTarget;
            Vector3 direction = (turnTowardNavSteeringTarget - transform.position).normalized;
            lookRotationVector = new Vector3(direction.x, 0, direction.z);

            if (lookRotationVector != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookRotationVector);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
            }
        } else {
            /* The next check checks if this enemy is seeing BRAD now and stops the reset. */
            if (eyesOnTarget)
            {
                EyesOnTarget();
                return;
            }
            /* beneath this line resets the DogController and VisionLine's materials */
            this.visionLine.GetComponent<Renderer>().material = visionUndetectedMaterial;
			this.alerted = false;
			exclamation.SetActive(false);
            alertedTimer = 5.0f;
            agent.SetDestination(startPosition);
        }
    }
    public void BecomeAlerted () {
		this.alerted = true;
        targetPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        agent.SetDestination(targetPosition);
        alertedTimer = 5.0f;
		this.visionLine.GetComponent<Renderer>().material = visionAlertedMaterial;
	}
    private void EyesOnTarget ()
    {
        targetPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        agent.SetDestination(targetPosition);

        Vector3 direction = (targetPosition - transform.position).normalized;
        lookRotationVector = new Vector3(direction.x, 0, direction.z);

        if (lookRotationVector != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookRotationVector);
            Debug.Log("We should be seeing you, " + eyesOnTarget + " // " + lookRotation);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
        }
    }
}
