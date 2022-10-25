using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointBehaviour : MonoBehaviour {
	/// <summary>
	/// Whenever player collides, run the Checkpoint-Function on our GameManager (Checkpoint records your checkpoint, and changes checkpoint render colour to green and safe.)
	/// </summary>
	/// <param name="other"></param>
	private void OnTriggerEnter ( Collider other ) {
		Debug.Log( other.transform );
		if ( other.transform.tag == "Player" ) {
			if (!GameManager.Instance) { return; }

			GameManager.Instance.Checkpoint(gameObject);
		}
	}
}
