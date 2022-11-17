using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using static GameManager;
using System.Collections;

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

	public List<string> levelNames = new List<string> { "T_Level01", "T_Level02", "T_Level03", "T_Level04", "A_Level05", "A_Level06-Maze", "A_Level07", "TA_Asset_Museum" };
	public List<string> experimentLevels = new List<string> { "Tut01", "Level01", "Level02", "Level04", "Level05", "Level06", "Level07" , "Level09"};
	
	private Dictionary<string,string> presentableLevelNames = new Dictionary<string, string>() {
		{ "T_Level01","Level 1" },
		{ "T_Level02","Level 2" },
		{ "T_Level03", "Level 3"},
		{ "T_Level04", "Level 4" },
		{"A_Level05", "Level 5" },
		{"A_Level06-Maze", "Level 6 - Maze" },
		{"Z_level_fuckk", "Level 7" },
		{"TA_Asset_Museum", "Asset Museum" },
		{ "Tut01", "Experiment 1"},
		{ "Level01", "Experiment 2"},
		{ "Level02", "Experiment 3"},
		{ "Level04", "Experiment 4"},
		{ "Level05", "Experiment 5"},
		{ "Level06", "Experiment 6"},
		{ "Level07", "Experiment 7"},
		{ "Level09", "Experiment 8"}
	};


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
	public string menuScene = "Main Menu";

	[Header( "Prefab Settings" )]
	public Canvas missionListCanvas;
	public TMP_Text ui_t_intelStateField, ui_t_disguiseStateField, uiTMapNameField;
	public GameObject escScreen, nextLevelScreen, helpScreen, gameOverscreen, winGameScreen, disguisedOverlay, levelSelectScreen;

	private GameObject currentIntelObject;
	private List<GameObject> intelState;
	private List<GameObject> allIntelObjects = new List<GameObject>();
	private List<int> checkpointIntelState = new List<int>();
	private List<GameObject> allCheckpoints = new List<GameObject>();
	private List<int> checkpointKeyState = new List<int>();
	private List<GameObject> allKeyObjects = new List<GameObject>();
	private int currentCheckpoint = -1;
	private string lastLevel;
	//[HideInInspector]
	public List<GameObject> enemiesAlerted;

	private GameState currentGameState;

	private bool debugMessages = false;
	private PlayerController playerController;

	private Camera mainCamera;

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
		currentLevelIndex = 0;
		currentLevelName = SceneManager.GetActiveScene().name;
		if (currentLevelName == "Preload") {
			currentLevelName = menuScene;
			ChangeGameState(GameState.MainMenu);
		}
	}

	/// <summary>
	///		Lets you move on from the Start scene. 
	/// </summary>
	void Update () {
		if ( GetGameState() == GameState.GameOver && anykeyTimer > anykeyTimeLimit && Input.anyKeyDown ) {
			if ( Input.GetKeyDown( KeyCode.Escape ) ) {
				ChangeGameState( GameState.MainMenu );
				return;
			}
			anykeyTimer = 0f;
			RestartLevel();
		} else if ( GetGameState() == GameState.WinGame && anykeyTimer > anykeyTimeLimit && Input.anyKeyDown ) {
			anykeyTimer = 0f;
			ChangeGameState(GameState.MainMenu);
			return;
		} else if ( Input.GetKeyDown( KeyCode.Escape ) && currentLevelName != menuScene ) {
			if ( escScreen.activeSelf == true ) {
				ChangeGameState( GameState.Playing );
			} else {
				ChangeGameState( GameState.EscScreen );
			}
		}
		if ( GetGameState() == GameState.GameOver  || GetGameState() == GameState.WinGame ) {
			anykeyTimer += Time.deltaTime;
		}
	}

	/// <summary>
	/// Activates/Deactivates the disguise-overlay
	/// </summary>
	/// <param name="disguised"></param>
	public void DisguisePlayer ( bool disguised) {
		if ( disguised ) {
			disguisedOverlay.SetActive( true );
		} else {
			disguisedOverlay.SetActive( false );
		}
	}

	/// <summary>
	/// Returns level name, and updates currentLevelIndex if the level is found in experimentLevels, or levelNames
	/// </summary>
	/// <param name="additive"></param>
	/// <returns></returns>
	private string GetTutorialOrRegularLevel (int additive = 0) {
		string levelName = "";

		if ( experimentLevels.IndexOf( currentLevelName ) != -1 || experimentLevels.IndexOf( SceneManager.GetActiveScene().name ) != -1 ) {
			currentLevelIndex = ( experimentLevels.IndexOf( currentLevelName ) != -1 ) ? experimentLevels.IndexOf( currentLevelName ) : experimentLevels.IndexOf( SceneManager.GetActiveScene().name );
			levelName = experimentLevels[ ( currentLevelIndex + additive < experimentLevels.Count ) ? currentLevelIndex + additive : currentLevelIndex ];
		} else if ( levelNames.IndexOf( currentLevelName ) != -1 || levelNames.IndexOf( SceneManager.GetActiveScene().name ) != -1 ) {
			currentLevelIndex = ( levelNames.IndexOf( currentLevelName ) != -1 ) ? levelNames.IndexOf( currentLevelName ) : levelNames.IndexOf( SceneManager.GetActiveScene().name);
			levelName = levelNames[ (currentLevelIndex + additive < levelNames.Count) ? currentLevelIndex + additive: currentLevelIndex];
		} else {
			levelName = SceneManager.GetActiveScene().name;
		}
		
		return levelName;
	}

	/// <summary>
	/// Pulls up the next-level-screen. Allowing the player their time to chose to move on to the next level, restart, go to the main menu, or a level select.
	/// </summary>
	/// <param name="mode"></param>
	public void NextLevelScreen (string mode = "") {
		string data = GetTutorialOrRegularLevel(1);
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

	#region Level changing stuff (NextLevel/ChangeLevel/RestartLevel)
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

		UpdateMapNameTitle();
		lastLevel = currentLevelName;
		currentLevelName = level;
		anykeyTimer = 0f;

		/* If the level exists in our list of levels, load it . */
		if ( levelNames.IndexOf( level ) != -1 || experimentLevels.IndexOf( level ) != -1 ) {
			currentLevelIndex = (levelNames.IndexOf( level ) != -1 ) ? levelNames.IndexOf( currentLevelName ): experimentLevels.IndexOf( currentLevelName );

			SceneManager.LoadScene( level );
			/* Functions needed for missions to work, can be found in SceneChangeActions() */
			return;
		}

		SceneManager.LoadScene( level );
		//MissionList(false);
	}

	/// <summary>
	/// Restarts last loaded level.
	/// </summary>
	public void RestartLevel ( bool resetCheckpoint = false ) {
		string data = GetTutorialOrRegularLevel();
		
		if ( resetCheckpoint == true ) {
			currentCheckpoint = -1;
			checkpointIntelState = new List<int>();//Resetting Checkpoint Intel State
			checkpointKeyState = new List<int>(); //Resetting Checkpoint Key State

			UpdateIntelState( true );
			MissionList();
		}

		ChangeLevel( data );
	}

	/// <summary>
	///		Checks that you have picked up all the intel, accepts bool ignoreIntelState which makes winConditionCheck go to the nextLevel
	/// </summary>
	/// <param name="ignoreIntelState"></param>
	public void LevelCompleteCheck ( string level = "", bool ignoreIntelState = false) {
		/* TODO: Move to Level Manager */

		/* This check makes sure that Player has picked up all intel needed to allow their exfiltration. */
		if ( currentPickedUpIntelCount != currentLevelIntelTotal && !ignoreIntelState )
			return;

		if (currentLevelIndex + 1 >= levelNames.Count) {
			currentLevelIndex = 0;
			ChangeGameState( GameState.WinGame );
			return;
		}

		if ( !ignoreIntelState ) {
			NextLevelScreen();
		} else {
			NextLevel( level ); 
		}
	}

	#endregion

	#region Gathering Stuff (Intel/Keys)
	/// <summary>
	/// Picks up key, disables all structures associated with key, updates checkpointKeyState
	/// </summary>
	/// <param name="keyObject"></param>
	public void PickedUpKey ( GameObject keyObject ) {
		string tagName = keyObject.GetComponent<KeyController>().tagName;
		int keyIndex = allKeyObjects.IndexOf( keyObject );
		if (keyIndex == -1) { Debug.Log("allKeyObjects is missing this key. Please verify.");  return; }

		if ( checkpointKeyState.IndexOf(keyIndex) == -1) {
			checkpointKeyState.Add( keyIndex );
		}

		GameObject[] sesameTargets = GameObject.FindGameObjectsWithTag( tagName );

		if ( sesameTargets.Length == 0 ) {
			Debug.LogError( "You need to place something for me to disable. Please place something with the tag \"" + tagName + "\"." );
			gameObject.SetActive( false );
			return;
		}

		for ( int i = 0; i < sesameTargets.Length; i++ ) {
			sesameTargets[ i ].SetActive( false );
		}

		/* TODO: Move to Level Manager */
		keyObject.SetActive( false );
	}

	/// <summary>
	/// Reduces the IntelCount, runs ExitLockCheck, then Destroys intelObject.
	/// </summary>
	/// <param name="intelObject"></param>
	public void PickedUpIntel (GameObject intelObject) {
		currentIntelObject = intelObject;
		int intelIndex = allIntelObjects.IndexOf( intelObject );
		if ( intelIndex == -1 ) { Debug.Log( "allIntelObjects is missing this key. Please verify." ); return; }

		if ( checkpointIntelState.IndexOf( intelIndex ) == -1 ) {
			checkpointIntelState.Add( intelIndex );
		}

		/* TODO: Move to Level Manager */
		intelObject.SetActive( false );

		MissionList();
		UnlockExit();
	}
	#endregion

	/// <summary>
	/// Default behaviour: Check if all intel is picked up, by "currentLevelIntelCount", searches for tags named "ExfilZone", disables all "exit_lock"-named objects.
	/// </summary>
	/// <param name="parentTag">string tagName of parent containing exit_lock-object</param>
	/// <param name="exitCheck">bool enable intel-check?</param>
	public void UnlockExit (string parentTag = "ExfilZone", bool exitCheck = true ) {
		if ( exitCheck && currentPickedUpIntelCount != currentLevelIntelTotal ) return; /* TODO: Move to Level Manager */

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
	#region Checkpoints 
	public void Checkpoint (GameObject checkpoint) {
		currentCheckpoint = allCheckpoints.IndexOf(checkpoint);
		checkpoint.transform.GetComponent<Renderer>().material.color = Color.cyan;
		checkpoint.transform.GetComponent<Renderer>().material.color = Color.green;
	}
	/// <summary>
	/// Should return the player to the checkpoint, as well as re-seat the intel-state 
	/// </summary>
	public void ReturnToCheckpoint () {
		if (currentCheckpoint == -1 ) {
			checkpointIntelState = new List<int>();
			checkpointKeyState = new List<int>();
			MissionList();

			if ( debugMessages ) { 
				Debug.LogError( "You have no checkpoint to return to." ); 
			}

			//Update missions list and unlock if that is required.
			MissionList();
			UnlockExit();
			return; 
		}

		if ( allIntelObjects.Count == 0 ) { 
			if ( debugMessages ) { 
				Debug.LogError( "allIntelObjects not set, this variable is required for ReturnToCheckPoint." ); 
			}  
		} else {
			// Go through all the intel-objects, and disable intel-objects we find in checkpointIntelState
			for ( int i = 0; i < allIntelObjects.Count; i++ ) {
				if ( ( checkpointIntelState.IndexOf( i ) != -1 ) ) {
					allIntelObjects[ checkpointIntelState[ i ] ].SetActive( false );
					//PickedUpIntel( allIntelObjects[ checkpointIntelState[i] ] );
				}
			}

			//Update missions list and unlock if that is required.
			MissionList();
			UnlockExit();
		}


		if ( allKeyObjects.Count == 0) { 
			if ( debugMessages ) { 
				Debug.LogError( "allKeys not set, this variable is required for ReturnToCheckPoint." ); 
			}
		} else { 
			for ( int i = 0; i < allKeyObjects.Count; i++ ) {
				if ( ( checkpointKeyState.IndexOf( i ) != -1 ) ) {
					PickedUpKey( allKeyObjects[checkpointKeyState[i]]);
				}
			}
		}

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
	#endregion

	#region Mission List
	/// <summary>
	/// Initializes, and updates our intel-state related variables/objects. 
	/// </summary>
	/// <param name="setIntelObjects"></param>
	private void UpdateIntelState (bool setIntelObjects = false) {
		if ( allIntelObjects.Count == 0 || setIntelObjects) {
			currentPickedUpIntelCount = 0;
			allIntelObjects = new List<GameObject> ( GameObject.FindGameObjectsWithTag( "Intel" )); 
			currentLevelIntelTotal = allIntelObjects.Count;
		}
		
		currentPickedUpIntelCount = checkpointIntelState.Count;
		
		if ( currentPickedUpIntelCount == currentLevelIntelTotal ) {
			ui_t_intelStateField.SetText( "<s>Collect Intel: " + currentLevelIntelTotal + " / " + currentLevelIntelTotal + "</s>" );
		} else {
			ui_t_intelStateField.SetText( "Collect Intel: " + currentPickedUpIntelCount + " / " + currentLevelIntelTotal );
		}
	}
	/// <summary>
	/// Update the Used disguises number to the most recent one.
	/// </summary>
	/// <param name="usedDisguises"></param>
	/// <param name="disguisesTotal"></param>
	public void UpdateDisguiseState ( int usedDisguises, int totalDisguises) {
		int subtractedDisguises = totalDisguises - usedDisguises;
		
		ui_t_disguiseStateField.SetText( "Disguises Available: " + subtractedDisguises.ToString() + " (R)");
	}

	/// <summary>
	/// Enables (by default) and generates mission list. bool activateMissionList 
	/// </summary>
	/// <param name="activateMissionList"></param>
	public void MissionList (bool activateMissionList = true) {
		if ( !activateMissionList ) {
			missionListCanvas.gameObject.SetActive( false );
		} else {
			UpdateIntelState();
			missionListCanvas.gameObject.SetActive(true);
		}
	}

	private void UpdateMapNameTitle () {
		if ( currentLevelName == null ) {
			return;
		}


		if ( GetGameState() == GameState.Playing && presentableLevelNames.ContainsKey(currentLevelName)) { 
			uiTMapNameField.text = presentableLevelNames[currentLevelName];
		} else if ( currentLevelName != menuScene && !presentableLevelNames.ContainsKey( currentLevelName ) ) {
			Debug.LogError("You have to add a presentable Level Name for " + currentLevelName + " to presentableLevelName in the GameManager.");
		}
	}
	#endregion

	#region New Sceene Activity
	/// <summary>
	/// SceneChangeActions is run after a scene is loaded. Any script functionality that needs to be run after a scene is loaded goes here.
	/// </summary>
	/// <param name="scene"></param>
	/// <param name="mode"></param>
	private void SceneChangeActions ( Scene scene, LoadSceneMode mode ) {
		InitializeLevel( scene.name );
		nextLevelScreen.SetActive( false );
		escScreen.SetActive( false );
	}

	public void UpdateMainCamera () {
		mainCamera = FindObjectOfType<Camera>();
		GameObject[] overlays = { escScreen, nextLevelScreen, helpScreen, gameOverscreen, winGameScreen, disguisedOverlay, missionListCanvas.gameObject};
		foreach ( var item in overlays ) {
			item.GetComponent<Canvas>().worldCamera = mainCamera;
			item.GetComponent<Canvas>().planeDistance = 10;
		}
	}

	/// <summary>
	/// Initializes level states.
	/// </summary>
	/// <param name="name"></param>
	private void InitializeLevel ( string name ) {
		UpdateMainCamera();
		
		if (GetGameState() == GameState.LevelSelect ) {
			if ( name != null && name == menuScene ) {
				MainMenu mainMenu = GameObject.FindObjectOfType<MainMenu>();
				mainMenu.mainMenuPanel.SetActive( false );
				mainMenu.levelSelectMenu.SetActive( true );
			} else {
				Debug.LogError("No menuScene in name.");
			}
			return;
		}
		GameManager.Instance.enemiesAlerted = new List<GameObject>();

		if ( levelNames.IndexOf( name ) != -1 || experimentLevels.IndexOf( name ) != -1 || GameObject.Find("LevelManager") || GetGameState() == GameState.Playing) {
			allIntelObjects = new List<GameObject>();

			allCheckpoints = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Checkpoint" ) );
			allKeyObjects = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Key" ) );

			if ( lastLevel != currentLevelName ) {
				currentCheckpoint = -1;
				checkpointIntelState = new List<int>();
				checkpointKeyState = new List<int>();

				UpdateIntelState( true );
				MissionList();
				UnlockExit();
			} else {
				UpdateIntelState();
				UpdateIntelState( true );
				ReturnToCheckpoint();
			}

			ChangeGameState( GameState.Playing );
		} else {
			MissionList( false );
			ChangeGameState( GameState.Playing );
		}

		UpdateMapNameTitle();
	}
	#endregion

	#region GameState
	/// <summary>
	/// Returns the current game-state in a string format.
	/// </summary>
	/// <returns></returns>
	/// 
	public GameState GetGameState () {
		return currentGameState;
	}

	/// <summary>
	/// Changes game state to whichever gamestate you wish.
	/// </summary>
	/// <param name="newState">eq GameState.Menu</param>
	/// <param name="data">string data</param>
	public void ChangeGameState ( GameState newState, string data = "" ) {
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
				winGameScreen.SetActive( false );
				levelSelectScreen.SetActive( false );
				break;
			case GameState.Paused:
				break;
			case GameState.LevelSelect:

				/* Reset currentLevelIndex */
				currentLevelIndex = 0;
				ChangeLevel( menuScene );
				break;
			case GameState.GameOver:
				gameOverscreen.SetActive( true );
				break;
			case GameState.WinGame:
				winGameScreen.SetActive( true );
				break;
			case GameState.EscScreen:
				escScreen.SetActive( true );
				break;
			default:
				break;
		}
		OnGameStateChange?.Invoke( currentGameState );
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
	#endregion
}
