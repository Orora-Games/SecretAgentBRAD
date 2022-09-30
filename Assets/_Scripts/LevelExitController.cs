using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExitController : MonoBehaviour {
	public bool ignoreIntelState = false;
	public string nextScene;

	private void Start () {
		gameObject.GetComponent<Collider>().isTrigger = true;
	}

	private void OnTriggerEnter ( Collider other ) {
		if ( other.transform.tag == "Player" ) {
			if ( !GameManager.Instance ) return;
			GameManager.Instance.LevelCompleteCheck();
		}
	}
}
