using UnityEngine;

public class ComputerIntelManager : MonoBehaviour {
	public bool hackingEnabled = false;
	[SerializeField]
	private GameObject progressSlider;
	// Start is called before the first frame update
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		if (!hackingEnabled ) {
			return; 
		}


	}
	public void EnableHacking () {
		hackingEnabled = true;
	}
}
