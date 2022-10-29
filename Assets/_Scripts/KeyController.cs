using UnityEngine;

public class KeyController : MonoBehaviour {
	public string tagName;

	// Start is called before the first frame update
	void Start () {
		if ( string.IsNullOrEmpty( tagName ) ) {
			Debug.LogError("Please enter a tagName, otherwise I won't know what to search for.");
			gameObject.SetActive(false);
			return;
		}

		if( !DoesTagExist(tagName) ) {
			Debug.LogError( "Please create the tag \"" + tagName + "\".");
			gameObject.SetActive(false);
			return;
		}
	}

	private void OnTriggerEnter ( Collider other ) {
		if ( other.transform.tag == "Player" ) {
			if ( !GameManager.Instance )
				return;
			GameManager.Instance.PickedUpKey( this.gameObject );
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
