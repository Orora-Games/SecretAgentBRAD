using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogController : MonoBehaviour
{
    //Alerted State
    public bool alerted = false;
    public float alertedTimer = 5.0f;
    public bool eyesOnTarget = false;
    private GameObject exclamation;
    private GameObject visionLine;
    
    //Materials
    private Material visionAlertedMaterial;
    private Material visionUndetectedMaterial;

	// Start is called before the first frame update
	void Start()
    {
        exclamation = gameObject.transform.Find( "Exclamation" ).gameObject;
		visionLine = gameObject.transform.Find( "VisionLine" ).gameObject;
		visionAlertedMaterial = visionLine.GetComponent<VisionController>().alertedMaterial;
		visionUndetectedMaterial = visionLine.GetComponent<VisionController>().undetectedMaterial;
	}

    // Update is called once per frame
    void Update()
    {
        if (alerted && alertedTimer > 0f) {
            alertedTimer = alertedTimer - Time.deltaTime;
			exclamation.SetActive(true);
		} else {
            /* The next check checks if this enemy is seeing BRAD now and stops the reset. */
            if ( eyesOnTarget )	return;
            /* beneath this line resets the DogController and VisionLine's materials */
			this.visionLine.GetComponent<Renderer>().material = visionUndetectedMaterial;
			this.alerted = false;
			exclamation.SetActive(false);
            alertedTimer = 5.0f;
		}
    }
    public void BecomeAlerted () {
		this.alerted = true;
		alertedTimer = 5.0f;
		this.visionLine.GetComponent<Renderer>().material = visionAlertedMaterial;
	}
}
