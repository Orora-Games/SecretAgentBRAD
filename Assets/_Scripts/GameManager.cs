using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	/* Game Manager tutorial
	 * Source:  https://youtu.be/4I0vonyqMi8 */
	private static GameManager _instance;
	public static GameManager Instance {
		get {
			if ( _instance == null )
				Debug.LogError("GameManager does not exist!");
			return _instance;
		}
	}

	public List<string> levelNames = new List<string> { "Tutorial_Level1_Prototype", "Tutorial_Level2_Prototype", "Level00", "Level01", "Level03" };
	private int currentLevelIndex = 0;
	private string currentLevelName = "Start";

	public GameState state;
	public static event Action<GameState> OnGameStateChange;

	/* Set static levels here. */
	public string gameOverLevel = "YouLost";
	public string winLevel = "Finished";
	public string menuScene = "Start";

	private int currentLevelIntelCount;
	private float anykeyTimeLimit = 1f;
	private float anykeyTimer = 0f;

	private void Awake () {
		_instance = this;
		//levelNames = new List<string> { "TestScene" };
	}


	void Start () {
		DontDestroyOnLoad(this);
		ChangeGameState( GameState.Menu );
	}

	/// <summary>
	///		Lets you move on from the Start scene. 
	/// </summary>
	void Update () {
		if ( Input.GetKey( KeyCode.Escape ) ) {
			Application.Quit();
		} else if ( ( currentLevelName == "Start" || currentLevelName == "Finished" || currentLevelName == "YouLost" ) && anykeyTimer > anykeyTimeLimit && Input.anyKeyDown ) {
				anykeyTimer = 0f;
				string level = levelNames[ currentLevelIndex ];

				ChangeLevel( level );
		}
		anykeyTimer += Time.deltaTime;
	}

	/// <summary>
	/// Moves you on to the next level, or the Finished-level if you managed to finish all the levels. 
	///		takes string level, which lets you override the level
	/// </summary>
	/// <param name="level"></param>
	public void NextLevel (string level = "") {
		if (level != "") {
			ChangeLevel( level );
			return;
		}

		currentLevelIndex++;

		if ( currentLevelIndex > levelNames.Count - 1 ) {
			currentLevelIndex = 0;
			ChangeGameState( GameState.WinGame );
			return;
		}

		ChangeLevel( levelNames[ currentLevelIndex ] );
	}

	/// <summary>
	/// Changes the level based on the string level, if the string is found in the level-list, currentLevelIndex is updated, and 
	/// </summary>
	/// <param name="level"></param>
	public void ChangeLevel (string level) {
		currentLevelName = level;
		anykeyTimer = 0f;

		/* If the level exists in our list of levels, load it . */
		if ( levelNames.IndexOf( level ) != -1) {
			SceneManager.LoadScene( level );
			currentLevelIntelCount = GameObject.FindGameObjectsWithTag( "Intel" ).Length;
			UnlockObject();
			return;
		}

		SceneManager.LoadScene( level );

	}


	/// <summary>
	///		Checks that you have picked up all the intel, accepts bool ignoreIntelState which makes winConditionCheck go to the nextLevel
	/// </summary>
	/// <param name="ignoreIntelState"></param>
	public void LevelCompleteCheck ( string level = "", bool ignoreIntelState = false) {
		/* TODO: Move to Level Manager */

		/* This check makes sure that Player has picked up all intel needed to allow their exfiltration. */
		if ( currentLevelIntelCount > 0 && !ignoreIntelState )
			return;

		if ( !ignoreIntelState ) {
			NextLevel();
		} else { 
			NextLevel( level ); 
		}
	}

	/// <summary>
	/// Reduces the IntelCount, runs ExitLockCheck, then Destroys intelObject.
	/// </summary>
	/// <param name="intelObject"></param>
	public void PickedUpIntel (GameObject intelObject ) {
		currentLevelIntelCount -= 1;
		UnlockObject();
		
		/* TODO: Move to Level Manager */
		Destroy(intelObject);
	}

	/// <summary>
	/// Default behaviour: Check if all intel is picked up, by "currentLevelIntelCount", searches for tags named "ExfilZone", disables all "exit_lock"-named objects.
	/// </summary>
	/// <param name="parentTag">string tagName of parent containing exit_lock-object</param>
	/// <param name="exitCheck">bool enable intel-check?</param>
	public void UnlockObject (string parentTag = "ExfilZone", bool exitCheck = true ) {
		if ( exitCheck &&  currentLevelIntelCount > 0 ) return; /* TODO: Move to Level Manager */

		/* Allow for more than one exit Zone. */
		GameObject[] lockObjectParents = GameObject.FindGameObjectsWithTag( parentTag );

		for ( int i = 0; i < lockObjectParents.Length; i++ ) {
			if ( exitCheck ) {
				lockObjectParents[ i ].transform.Find( "exit_lock" ).gameObject.SetActive( false );
			} else {
				lockObjectParents[ i ].gameObject.SetActive( false );
			}
		}
		//GameObject lockObject = GameObject.Find( "ExfilZone" ).transform.Find( "exit_lock" ).gameObject;
		//
	}
	/// <summary>
	/// Changes game state to whichever gamestate you wish.
	/// </summary>
	/// <param name="newState">eq GameState.Menu</param>
	/// <param name="data">string data</param>
	public void ChangeGameState ( GameState newState, string data = "") {
		state = newState;

		switch ( newState ) {
			case GameState.Menu:
				ChangeLevel( menuScene );
				break;
			case GameState.Playing:
				break;
			case GameState.Paused:
				break;
			case GameState.GameOver:
				ChangeLevel( gameOverLevel );
				break;
			case GameState.WinGame:
				ChangeLevel( winLevel );
				break;
			default:
				break;
		}
		OnGameStateChange?.Invoke( state );
	}

	public enum GameState {
		Menu,
		Playing,
		Paused,
		GameOver,
		WinGame
	}
}
