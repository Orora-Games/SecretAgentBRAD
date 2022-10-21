using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using static GameManager;

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

	public List<string> levelNames = new List<string> { "Level01", "Level02", "Level03", "Level04", "Level05", "Level06", "Level07" };
	public List<string> tutorialLevels = new List<string> { "Tut01", "Tut02"};
	private int currentLevelIndex = 0;
	private string currentLevelName;

	public GameState state;
	public static event Action<GameState> OnGameStateChange;

	private int currentLevelIntelCount;
	private int currentLevelIntelTotal;
	private float anykeyTimeLimit = 1f;
	private float anykeyTimer = 0f;

	/* Set static levels here. */
	[Header("Scene Selection")] 
	public string gameOverLevel = "YouLost";
	public string winLevel = "Finished";
	public string menuScene = "Main Menu";

	[Header( "Prefab Settings" )]
	public Canvas MissionListCanvas;
	public TMP_Text MissionListText;
	public GameObject escScreen, nextLevelScreen, helpScreen;

	private GameObject currentIntelObject;

	private GameState currentGameState;

	private void OnEnable () {
		SceneManager.sceneLoaded += SceneChangeActions;
	}

	private void Awake () {
		DontDestroyOnLoad( this );

		if ( _instance == null ) {
			_instance = this;
		} else {
			Destroy( this.gameObject );
		}
	}


	void Start () {
		currentLevelName = SceneManager.GetActiveScene().name;	
		currentLevelIndex = 0;
	}

	/// <summary>
	///		Lets you move on from the Start scene. 
	/// </summary>
	void Update () {
		if ( Input.GetKeyDown( KeyCode.Escape ) && currentLevelName != menuScene ) {
				if ( escScreen.activeSelf == true ) {
					ChangeGameState(GameState.Playing );
				} else {
					ChangeGameState( GameState.EscScreen );
				}
		} else if ( ( currentLevelName == "Finished" || currentLevelName == "YouLost" ) && anykeyTimer > anykeyTimeLimit && Input.anyKeyDown ) {
			anykeyTimer = 0f;
			RestartLevel();
		} /* Still used to get out of YouLost scene. */
		anykeyTimer += Time.deltaTime;
	}

	/// <summary>
	/// Restarts last loaded level.
	/// </summary>
	public void RestartLevel () {
		string data = getTutorialOrRegularLevel();
		ChangeLevel( data );
	}
	/// <summary>
	/// Will return level name if known in tutorials OR in levelNames
	/// </summary>
	/// <param name="additive"></param>
	/// <returns></returns>
	private string getTutorialOrRegularLevel (int additive = 0) {
		int tutorialLevelIndex = ( tutorialLevels.IndexOf( currentLevelName ) != -1) ? tutorialLevels.IndexOf( currentLevelName ) + additive : tutorialLevels.IndexOf( SceneManager.GetActiveScene().name ) + additive;
		return ( tutorialLevelIndex != -1 ) ? ( tutorialLevelIndex >= tutorialLevels.Count ) ? levelNames[ currentLevelIndex + additive ] : tutorialLevels[ tutorialLevelIndex ] : levelNames[ currentLevelIndex + additive ];
	}
	/// <summary>
	/// Pulls up the next-level-screen. Allowing the player their time to chose to move on to the next level, restart, go to the main menu, or a level select.
	/// </summary>
	/// <param name="mode"></param>
	public void NextLevelScreen (string mode = "") {
		string data = getTutorialOrRegularLevel(1);
		//* Make sure ChangeGameState/nextLevelScreen/if mode return is in that order. *//
		ChangeGameState( GameState.Paused );
		nextLevelScreen.SetActive( true );

		if ( mode == "" ) { return; }

		switch ( mode ) {
			case "NextLevel":
				NextLevel(data);
				break;
			case "RestartLevel":
				RestartLevel();
				break;
			case "MainMenu":
				ChangeGameState( GameState.MainMenu );
				break;
			case "LevelSelect":
				ChangeGameState( GameState.LevelSelect );
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Moves you on to the next level, or the Finished-level if you managed to finish all the levels. 
	///		takes string level, which lets you override the level
	/// </summary>
	/// <param name="level"></param>
	public void NextLevel ( string level = "", bool skipLevelIncrease = false) {
		ChangeGameState( GameState.Playing );
		
		if (level != "") {
			ChangeLevel( level );
			return;
		}

		if (!skipLevelIncrease ) { 
			currentLevelIndex++;
		}

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
		if ( level == "" ) {
			Debug.LogError("ChangeLevel requires a level in the form of string \"level\".");
			return;
		}

		currentLevelName = level;
		anykeyTimer = 0f;

		/* If the level exists in our list of levels, load it . */
		if ( levelNames.IndexOf( level ) != -1) {
			currentLevelIndex = levelNames.IndexOf( currentLevelName );

			SceneManager.LoadScene( level);
			/* Functions needed for missions to work, can be found in SceneChangeActions() */
			return;
		}

		SceneManager.LoadScene( level );
		//MissionList(false);
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
			NextLevelScreen();
		} else {
			NextLevel( level ); 
		}
	}

	/// <summary>
	/// Reduces the IntelCount, runs ExitLockCheck, then Destroys intelObject.
	/// </summary>
	/// <param name="intelObject"></param>
	public void PickedUpIntel (GameObject intelObject ) {
		currentIntelObject = intelObject;
		currentLevelIntelCount -= 1;
		UnlockObject();
		MissionList();

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
	}
	
	/// <summary>
	/// Handles button-functionality in menu navigation.
	/// </summary>
	/// <param name="buttonType"></param>
	public void MenuButtonBehaviour ( string buttonType ) {
		switch ( buttonType ) {
			case "MainMenu":
				/* Deactivate escScreen, we don't want to see it when we enter a new level. */
				ChangeGameState( GameState.MainMenu );
				break;
			case "RestartLevel":
				RestartLevel();
				break;
			case "HelpScreen":
				break;
			case "ReturnToGame":
				ChangeGameState( GameState.Playing );
				break;
			default:
				Debug.Log("You are trying to access a button type that is not yet defined: \"" + buttonType + "\".");
				break;
		}
	}
	/// <summary>
	/// Returns the current game-state in a string format.
	/// </summary>
	/// <returns></returns>
	public GameState GetGameState () {
		return currentGameState;
	}
	/// <summary>
	/// Changes game state to whichever gamestate you wish.
	/// </summary>
	/// <param name="newState">eq GameState.Menu</param>
	/// <param name="data">string data</param>
	public void ChangeGameState ( GameState newState, string data = "") {
		currentGameState = newState;

		switch ( newState ) {
			case GameState.MainMenu:
				if ( SceneManager.GetActiveScene().name == menuScene ) { 
					break;
				}
				/* Reset currentLevelIndex */
				currentLevelIndex = 0;
				ChangeLevel( menuScene );
				break;
			case GameState.Playing:
				escScreen.SetActive( false );
				helpScreen.SetActive( false );
				nextLevelScreen.SetActive( false );
				break;
			case GameState.Paused:
				break;
			case GameState.LevelSelect:
				
				/* Reset currentLevelIndex */
				currentLevelIndex = 0;
				ChangeLevel( menuScene );
				break;
			case GameState.GameOver:
				ChangeLevel( gameOverLevel );
				break;
			case GameState.WinGame:
				ChangeLevel( winLevel );
				break;
			case GameState.EscScreen:
				escScreen.SetActive( true );
				break;
			default:
				break;
		}
		OnGameStateChange?.Invoke( currentGameState );
	}
	private void SceneChangeActions (Scene scene, LoadSceneMode mode ) {
		GameState gs = GetGameState();
		if (gs == GameState.MainMenu || gs == GameState.LevelSelect) {
			ChangeGameState( GameState.Playing );
		}

		if ( levelNames.IndexOf( scene.name ) != -1  || tutorialLevels.IndexOf( scene.name ) != -1 ) {
			SetIntelState();
			MissionList();
			UnlockObject();
		} else {
			MissionList(false);
		}
		nextLevelScreen.SetActive( false );
		escScreen.SetActive( false );
	}

	/// <summary>
	/// Sets int currentLevelIntelCount and int currentLevelIntelTotal;
	/// </summary>
	private void SetIntelState () {
		currentLevelIntelTotal = GameObject.FindGameObjectsWithTag( "Intel" ).Length;
		Scene newScene = SceneManager.GetActiveScene();
		currentLevelIntelCount = currentLevelIntelTotal;
	}

	/// <summary>
	/// Enables (by default) and generates mission list. bool activateMissionList 
	/// </summary>
	/// <param name="activateMissionList"></param>
	public void MissionList (bool activateMissionList = true) {
		if (!activateMissionList ) {
			MissionListCanvas.gameObject.SetActive( false );
			MissionListText.text = "";
			return;
		}

		MissionListCanvas.gameObject.SetActive(true);

		if ( currentLevelIntelCount == 0) {
			MissionListText.SetText( "<s>- Find all Intel-folders (" + currentLevelIntelTotal + " / " + currentLevelIntelTotal + ")</s>" );
			//MissionListText.text = ;
		} else {
			MissionListText.text = "- Find all Intel-folders (" + (currentLevelIntelTotal-currentLevelIntelCount) + " / " + currentLevelIntelTotal + ")";
		}
	}

	public enum GameState {
		MainMenu,
		Playing,
		Paused,
		LevelSelect,
		GameOver,
		WinGame,
		EscScreen
	}
}
