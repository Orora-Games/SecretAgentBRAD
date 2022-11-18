using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	public int disguisesAvailable = 7;
	private int usedDisguises = 0;

	private void Start () {
		UpdateDisguiseNumbers();
	}
	/// <summary>
	/// Updates the Disguise State using GameManager
	/// </summary>
	public void UpdateDisguiseNumbers () {
		if ( !GameManager.Instance ) { return; }
		GameManager.Instance.UpdateDisguiseState( usedDisguises, disguisesAvailable );
	}

	/// <summary>
	/// Returns true/false if the player is able to disguise themselves.
	/// </summary>
	/// <returns></returns>
	public bool DisguiseCheck () {
		bool disguiseCheck = disguisesAvailable - usedDisguises > 0 && !AlertCheck();

		if ( disguiseCheck ) {
			usedDisguises++;
			UpdateDisguiseNumbers( );
		}
		return disguiseCheck;
	}
	/// <summary>
	/// Checks GameManager if enemies are alerted.
	/// </summary>
	/// <returns></returns>
	public bool AlertCheck () {
		if ( !GameManager.Instance ) { return false; }
		return GameManager.Instance.enemiesAlerted.Count>0;
	}
}