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

	private int currentPickedUpIntelCount;
	private int currentLevelIntelTotal;
	private float anykeyTimeLimit = 1f;
	private float anykeyTimer = 0f;

	/* Set static levels here. */
	[Header("Scene Selection")] 
	public GameObject gameOverscreen;
	public string winLevel = "Finished";
	public string menuScene = "Main Menu";

	[Header( "Prefab Settings" )]
	public Canvas MissionListCanvas;
	public TMP_Text MissionListText;
	public GameObject escScreen, nextLevelScreen, helpScreen;

	private GameObject currentIntelObject;
	private List<GameObject> intelState;
	private List<GameObject> allIntelObjects = new List<GameObject>();
	private List<int> checkpointIntelState = new List<int>();
	private List<GameObject> allCheckpoints = new List<GameObject>();
	private int currentCheckpoint = -1;
	private string lastLevel;

	private GameState currentGameState;

	private bool debugMessages = false;

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
		} else if ( ( currentLevelName == "Finished" || GetGameState() == GameState.GameOver ) && anykeyTimer > anykeyTimeLimit && Input.anyKeyDown ) {
			anykeyTimer = 0f;
			RestartLevel();
		} /* Still used to get out of YouLost scene. */
		anykeyTimer += Time.deltaTime;
	}

	/// <summary>
	/// Restarts last loaded level.
	/// </summary>
	public void RestartLevel (bool resetCheckpoint = false) {
		string data = getTutorialOrRegularLevel();
		if (resetCheckpoint == true) {
			currentCheckpoint = -1;

			UpdateIntelState( true );
			MissionList();
		}
		ChangeLevel( data );
	}
	/// <summary>
	/// Will return level name if known in tutorials OR in levelNames
	/// </summary>
	/// <param name="additive"></param>
	/// <returns></returns>
	private string getTutorialOrRegularLevel (int additive = 0) {
		int tutorialLevelIndex = -1;
		if ( tutorialLevels.IndexOf( currentLevelName ) != -1 ) {
			tutorialLevelIndex = tutorialLevels.IndexOf( currentLevelName ) + additive;
		} else if ( tutorialLevels.IndexOf( SceneManager.GetActiveScene().name ) != -1) {
			tutorialLevelIndex = tutorialLevels.IndexOf( SceneManager.GetActiveScene().name ) + additive;
		}
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
				RestartLevel(true);
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

		lastLevel = currentLevelName;
		currentLevelName = level;
		anykeyTimer = 0f;

		/* If the level exists in our list of levels, load it . */
		if ( levelNames.IndexOf( level ) != -1) {
			currentLevelIndex = levelNames.IndexOf( currentLevelName );

			SceneManager.LoadScene( level );
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
		if ( currentPickedUpIntelCount > 0 && !ignoreIntelState )
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
		checkpointIntelState.Add( allIntelObjects.IndexOf( intelObject ) );

		/* TODO: Move to Level Manager */
		intelObject.SetActive( false );

		MissionList();
		UnlockExit();
	}

	/// <summary>
	/// Default behaviour: Check if all intel is picked up, by "currentLevelIntelCount", searches for tags named "ExfilZone", disables all "exit_lock"-named objects.
	/// </summary>
	/// <param name="parentTag">string tagName of parent containing exit_lock-object</param>
	/// <param name="exitCheck">bool enable intel-check?</param>
	public void UnlockExit (string parentTag = "ExfilZone", bool exitCheck = true ) {
		if ( exitCheck &&  currentPickedUpIntelCount > 0 ) return; /* TODO: Move to Level Manager */

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
				RestartLevel(true);
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
	/// 
	#region GameState
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
				gameOverscreen.SetActive( false );
				break;
			case GameState.Paused:
				break;
			case GameState.LevelSelect:
				
				/* Reset currentLevelIndex */
				currentLevelIndex = 0;
				ChangeLevel( menuScene );
				break;
			case GameState.GameOver:
				gameOverscreen.SetActive(true);
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
	#endregion


	/// <summary>
	/// SceneChangeActions is run after a scene is loaded. Any script functionality that needs to be run after a scene is loaded goes here.
	/// </summary>
	/// <param name="scene"></param>
	/// <param name="mode"></param>
	private void SceneChangeActions (Scene scene, LoadSceneMode mode ) {
		GameState gs = GetGameState();

		if (gs == GameState.LevelSelect ) {
			ChangeGameState( GameState.Playing );
		}

		InitializeLevel(scene.name);
		nextLevelScreen.SetActive( false );
		escScreen.SetActive( false );
	}

	/// <summary>
	/// Initializes level states.
	/// </summary>
	/// <param name="name"></param>
	private void InitializeLevel(string name) {
		if ( levelNames.IndexOf( name ) != -1 || tutorialLevels.IndexOf( name ) != -1 ) {
			allIntelObjects = new List<GameObject>();

			allCheckpoints = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Checkpoint" ) );

			if ( lastLevel != currentLevelName) {
				currentCheckpoint = -1;
				checkpointIntelState = new List<int>();
				
				UpdateIntelState( true );
				MissionList();
				UnlockExit();
			} else {
				UpdateIntelState( true );
				ReturnToCheckpoint();
			}

			ChangeGameState( GameState.Playing );
		} else {
			MissionList( false );
		}
	}

	public void Checkpoint (GameObject checkpoint) {
		currentCheckpoint = allCheckpoints.IndexOf(checkpoint);

		checkpoint.transform.GetComponent<Renderer>().material.color = Color.green;
	}
	/// <summary>
	/// Should return the player to the checkpoint, as well as re-seat the intel-state 
	/// </summary>
	public void ReturnToCheckpoint () {
		if (currentCheckpoint == -1 ) {
			checkpointIntelState = new List<int>();
			MissionList();

			if ( debugMessages ) { 
				Debug.LogError( "You have no checkpoint to return to." ); 
			} 
			return; 
		}
		if ( allIntelObjects.Count == 0) { if ( debugMessages ) { Debug.LogError( "allIntelObjects not set, this variable is required for ReturnToCheckPoint." ); } return; }

		// Go through all the intel-objects, and disable intel-objects we do _NOT_ find in checkpointIntelState
		for ( int i = 0; i < allIntelObjects.Count; i++ ) {
			allIntelObjects[i].SetActive(( checkpointIntelState.IndexOf( i ) == -1) );
		}

		//Update missions list and unlock if that is required.
		MissionList();
		UnlockExit();

		// IF the respawn zone exists, 
		GameObject respawn = GameObject.FindGameObjectWithTag("RespawnZone");
		GameObject player = GameObject.FindGameObjectWithTag( "Player" );
		allCheckpoints[ currentCheckpoint ].transform.GetComponent<CheckpointBehaviour>().enableCheckpoint = false;

		if ( respawn == null && player != null) {
			CharacterController characterController = player.transform.GetComponent<CharacterController>();

			//Play animation (Dim Down camera or something)
			characterController.enabled = false;
			float playerHeight = player.transform.position.y; // We're getting the player height here, because nextLocation is not the height we want our player at.
			player.transform.position = new Vector3( allCheckpoints[ currentCheckpoint ].transform.position.x, playerHeight, allCheckpoints[ currentCheckpoint ].transform.position.z );
			characterController.enabled = true;

			characterController.transform.position = allCheckpoints[ currentCheckpoint ].transform.position;
		} else {
			respawn.transform.position = new Vector3( allCheckpoints[ currentCheckpoint ].transform.position.x, respawn.transform.position.y, allCheckpoints[ currentCheckpoint ].transform.position.z );
		}

		allCheckpoints[ currentCheckpoint ].transform.GetComponent<Renderer>().material.color = Color.green;
	}

	/// <summary>
	/// Initializes, and updates our intel-state related variables/objects. 
	/// </summary>
	/// <param name="setIntelObjects"></param>
	private void UpdateIntelState (bool setIntelObjects = false) {
		if ( allIntelObjects.Count == 0 || setIntelObjects) {
			currentPickedUpIntelCount = 0;
			checkpointIntelState = new List<int>();
			allIntelObjects = new List<GameObject> ( GameObject.FindGameObjectsWithTag( "Intel" )); 
			currentLevelIntelTotal = allIntelObjects.Count;
		}
		
		currentPickedUpIntelCount = checkpointIntelState.Count;
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

		UpdateIntelState();

		MissionListCanvas.gameObject.SetActive(true);
		if ( currentPickedUpIntelCount == currentLevelIntelTotal ) {
			MissionListText.SetText( "<s>- Find all Intel-folders (" + currentLevelIntelTotal + " / " + currentLevelIntelTotal + ")</s>" );
			//MissionListText.text = ;
		} else {
			MissionListText.text = "- Find all Intel-folders (" + currentPickedUpIntelCount + " / " + currentLevelIntelTotal + ")";
		}
	}
	/// <summary>
	/// These are our gamestates.
	/// </summary>
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
