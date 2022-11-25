using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

public class MainMenu : MonoBehaviour {
	public GameObject mainMenuPanel, levelSelectMenu, optionsMenu;
	public TMP_Dropdown graphicsDropDown;

	private void Start () {
		if ( Application.platform == RuntimePlatform.WebGLPlayer ) {
			GameObject.Find( "quitButton" ).SetActive( false );
		}
		if ( !GameManager.Instance ) { SceneManager.LoadScene("Preload");  return;  }
		graphicsDropDown.value = QualitySettings.GetQualityLevel();
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

	public void SetGraphicsQuality (int qualityLevel) { 
		QualitySettings.SetQualityLevel( qualityLevel );
	}
}
