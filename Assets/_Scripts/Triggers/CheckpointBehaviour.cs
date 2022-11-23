using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointBehaviour : MonoBehaviour {
	public bool enableCheckpoint = true;
	/// <summary>
	/// Whenever player collides, run the Checkpoint-Function on our GameManager (Checkpoint records your checkpoint, and changes checkpoint render colour to green and safe.)
	/// </summary>
	/// <param name="other"></param>
	private void OnTriggerEnter ( Collider other ) {
		if ( other.transform.tag == "Player"  && enableCheckpoint ) {
			if (!GameManager.Instance) return;

			GameManager.Instance.Checkpoint(gameObject);
		}
		enableCheckpoint = true;
	}
}
