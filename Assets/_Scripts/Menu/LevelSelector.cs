using UnityEngine;

public class LevelSelector : MonoBehaviour {
	public string level;
	public void OpenScene () {
		if ( !GameManager.Instance ) return; 
		GameManager.Instance.ChangeLevel(level);
	}
}
