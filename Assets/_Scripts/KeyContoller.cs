using UnityEngine;

public class KeyContoller : MonoBehaviour {
	public string tagName;
	private GameObject[] sesameTargets; 

	// Start is called before the first frame update
	void Start () {
		if ( string.IsNullOrEmpty( tagName ) ) {
			Debug.LogError("Please enter a tagName, otherwise I won't know what to search for.");
			gameObject.SetActive(false);
			return;
		}

		if( !DoesTagExist(tagName) ) {
			Debug.Log("Please create the tag \"" + tagName + "\".");
			gameObject.SetActive(false);
			return;
		}
		
		sesameTargets = GameObject.FindGameObjectsWithTag(tagName);

		if ( sesameTargets.Length == 0 ) {
			Debug.Log( "You need to place something for me to disable. Please place something with the tag \"" + tagName + "\"." );
			gameObject.SetActive( false );
			return;
		}
	}

	private void OnTriggerEnter ( Collider other ) {
		if ( other.transform.tag == "Player" ) {
			for ( int i = 0; i < sesameTargets.Length; i++ ) {
				sesameTargets[i].SetActive(false);
			}
			gameObject.SetActive(false);
		}
	}

	public static bool DoesTagExist ( string aTag ) {
		try {
			GameObject.FindGameObjectsWithTag( aTag );
			return true;
		} catch {
			return false;
		}
	}
}
