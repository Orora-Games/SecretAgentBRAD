using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinConditionController : MonoBehaviour {
	public bool ignoreIntelState = false;
	public string nextScene;

	private void Start () {
		gameObject.GetComponent<Collider>().isTrigger = true;

		if (nextScene.Length == 0) {
			Debug.LogError( "The next scene has not been defined. Please define Next Scene on \"" + gameObject.name + " \"");
			return;
		}
	}

	private void OnTriggerEnter ( Collider other ) {
		if ( other.transform.tag == "Player" ) {


			/* Mapmaker knows best. Just load the next scene.*/
			if ( ignoreIntelState ) {
				SceneManager.LoadScene( nextScene );
				return;
			}

			/* This check makes sure that Player has picked up all intel needed to allow their exfiltration. */
			if (GameObject.FindGameObjectsWithTag("Intel").Length > 0) return; 

			SceneManager.LoadScene( nextScene );
		}
	}
}
