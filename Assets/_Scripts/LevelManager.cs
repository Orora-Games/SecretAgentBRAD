using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	public int disguisesAvailable = 7;
	private int usedDisguises = 0;

	private void Start () {
		doNumbers();
	}

	/// <summary>
	/// Will update based on levelManager's numbers.
	/// </summary>
	public void doNumbers () {
		UpdateDisguiseNumbers( usedDisguises, disguisesAvailable );
	}

	/// <summary>
	/// Updates the Disguise State using GameManager
	/// </summary>
	/// <param name="usedDisguisesLocal"></param>
	/// <param name="disguiseAvailableLocal"></param>
	public void UpdateDisguiseNumbers ( int usedDisguisesLocal, int disguiseAvailableLocal) {
		if ( !GameManager.Instance ) { return; }
		GameManager.Instance.UpdateDisguiseState( usedDisguisesLocal, disguiseAvailableLocal );
	}

	/// <summary>
	/// Returns true/false if the player is able to disguise themselves.
	/// </summary>
	/// <returns></returns>
	public bool DisguiseCheck () {
		bool disguiseCheck = disguisesAvailable > usedDisguises && !AlertCheck();

		if ( disguiseCheck ) {
			usedDisguises++;
			UpdateDisguiseNumbers( usedDisguises, disguisesAvailable );
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