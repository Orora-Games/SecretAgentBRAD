using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionController : MonoBehaviour 
{
	private GameObject guardDogObject;
	public Material DetectedMaterial;
	public Material UndetectedMaterial;
	public Material AlertedMaterial;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter ( Collider other ) {

		guardDogObject = gameObject.transform.parent.gameObject;

		if ( other.transform.tag == "Player" ) {
			Debug.Log( "WHEEEE!!!!" );
			GetComponent<Renderer>().material = DetectedMaterial;
			guardDogObject.GetComponent<DogController>().alerted = true;
			//StartCoroutine( Wait(5) );
			//GetComponent<Renderer>().material = UndetectedMaterial;
		}
	}
	void OnTriggerExit ( Collider Other ) {
		if ( Other.gameObject.tag == "Player" ) {

			GetComponent<Renderer>().material = AlertedMaterial;
		}
	}
	//IEnumerator Wait (int timeInSeconds) {
	//	//Print the time of when the function is first called.
	//	Debug.Log( "Started Coroutine at timestamp : " + Time.time );

	//	//yield on a new YieldInstruction that waits for 5 seconds.
	//	yield return new WaitForSeconds( timeInSeconds );

	//	//After we have waited 5 seconds print the time again.
	//	Debug.Log( "Finished Coroutine at timestamp : " + Time.time );
	//}
}
