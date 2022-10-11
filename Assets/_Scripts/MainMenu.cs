using UnityEngine;

public class MainMenu : MonoBehaviour {

	private void Start () {
		if ( Application.platform == RuntimePlatform.WebGLPlayer ) {
			GameObject.Find( "quitButton" ).gameObject.SetActive( false );
		}
	}
	//Load Scene
	public void Play() {
		GameManager.Instance.NextLevel( "", true );
	}
	//Quit Game
	public void Quit() {
		Application.Quit();
		Debug.Log( "Player Has Quit The Game" );
	}
}
