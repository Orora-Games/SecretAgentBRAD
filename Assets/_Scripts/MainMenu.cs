using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
	//Load Scene
	public void Play() {
		GameManager.Instance.NextLevel(  );
	}
	//Quit Game
	public void Quit() {
		Application.Quit();
		Debug.Log( "Player Has Quit The Game" );
	}
}
