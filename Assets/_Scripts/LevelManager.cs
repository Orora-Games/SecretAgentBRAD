using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	public int disguisesAvailable = 7;
	private int usedDisguises = 0;

	private void Start () {
		if ( GameManager.Instance ) {
			GameManager.Instance.UpdateDisguiseState( usedDisguises, disguisesAvailable );
		}
	}
	/// <summary>
	/// Returns true/false if the player is able to disguise themselves.
	/// </summary>
	/// <returns></returns>
	public bool DisguiseCheck () {
		bool disguiseCheck = disguisesAvailable > usedDisguises && !AlertCheck();

		if ( disguiseCheck ) {
			usedDisguises++;
			if ( GameManager.Instance ) {
				GameManager.Instance.UpdateDisguiseState( usedDisguises, disguisesAvailable );
			} else {
				Debug.LogError( "You have no GameManager, so Disguise state can't be updated." );
			}
		}
		return disguiseCheck;
	}
	public bool AlertCheck () { 
		return GameManager.Instance.enemiesAlerted.Count>0;
	}

}