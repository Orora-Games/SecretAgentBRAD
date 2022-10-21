using UnityEngine;

public class MainMenu : MonoBehaviour {
	public GameObject mainMenu, levelSelectMenu, optionsMenu;

	private void Start () {
		if ( Application.platform == RuntimePlatform.WebGLPlayer ) {
			GameObject.Find( "quitButton" ).gameObject.SetActive( false );
		}
		if ( !GameManager.Instance ) { return;  }
		
		if (GameManager.Instance.GetGameState() == GameManager.GameState.LevelSelect) {
			mainMenu.SetActive( false );
			levelSelectMenu.SetActive( true );
		}
		
	}
	//Load Scene
	public void Play() {
		if ( !GameManager.Instance ) { return; }
		GameManager.Instance.NextLevel( "", true );
	}
	//Quit Game
	public void Quit() {
		Application.Quit();
		Debug.Log( "Player Has Quit The Game" );
	}
}
