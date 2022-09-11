using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogController : MonoBehaviour
{
    public bool alerted = false;
    private GameObject exclamation;

    // Start is called before the first frame update
    void Start()
    {
        exclamation = gameObject.transform.Find( "Exclamation" ).gameObject;

	}

    // Update is called once per frame
    void Update()
    {
        if (alerted) {
			exclamation.SetActive(true);
		} else {
			exclamation.SetActive(false);
		}
    }
}
