using UnityEngine;

public class LevelExitController : MonoBehaviour {
	public bool ignoreIntelState = false;
	public string nextScene;

	private void Start () {
		gameObject.GetComponent<Collider>().isTrigger = true;

		if ( !GameManager.Instance ) return;
		GameManager.Instance.UnlockExit();
	}

	private void OnTriggerEnter ( Collider other ) {
		if ( other.transform.tag != "Player" ) return;
		if ( !GameManager.Instance ) return;
		
		GameManager.Instance.LevelCompleteCheck();
	}
}
