using UnityEngine;

public class LevelSelector : MonoBehaviour {
	public string level;
	public void OpenScene () {
		GameManager.Instance.ChangeLevel(level);
	}

}
