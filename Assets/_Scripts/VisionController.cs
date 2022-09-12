using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionController : MonoBehaviour 
{
	private GameObject guardDogObject;
	public Material detectedMaterial;
	public Material undetectedMaterial;
	public Material alertedMaterial;
	private GameObject[] toBeAlerted;
	private DogController dogController;


	// Start is called before the first frame update
	void Start()
    {
		guardDogObject = gameObject.transform.parent.gameObject;
		toBeAlerted = GameObject.FindGameObjectsWithTag( "Enemy" );
		dogController = guardDogObject.GetComponent<DogController>();
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	/* Triggers as player walks into vision of an enemy */
	private void OnTriggerEnter ( Collider other ) {
		if ( other.transform.tag == "Player" ) {
			/* Next line sets material to "detected" (red) */
			GetComponent<Renderer>().material = detectedMaterial;
			/* Make this enemy alerted */
			dogController.alerted = true;
			/* This enemy claims to see Player */
			dogController.eyesOnTarget = true;
			this.AlertEveryone(); // This alerts all enemies, triggers the timers, and movement (soon)
		}
	}
	/* Triggers when player exits enemy vision */
	void OnTriggerExit ( Collider Other ) {
		if ( Other.gameObject.tag == "Player" ) {
			dogController.eyesOnTarget = false; // This tells the system that this dog no longer sees Player
			this.AlertEveryone(true); // This alerts all dogs, triggers the timer,skipps detectingVision-check, and movement (soon)
		}
	}

	/* Is used as a shorthand to run BecomeAlerted() on every guardDog. */
	void AlertEveryone (bool exitTriggered = false) { //exitTriggered is used to skip detectingVision-check.
		for ( int i = 0; i < toBeAlerted.Length; i++ ) {
			/* Makes sure the detecting vision cone stays red. (detectingVision) */
			if ( !exitTriggered && toBeAlerted[ i ].gameObject.GetInstanceID() == guardDogObject.GetInstanceID() )
				continue;
			/* Alerts all other GameObjects with Enemy tag. */
			this.toBeAlerted[ i ].GetComponent<DogController>().BecomeAlerted();
		}
	}
}
