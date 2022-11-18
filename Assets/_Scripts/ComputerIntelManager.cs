using UnityEngine;

public class ComputerIntelManager : MonoBehaviour {
	public bool hackingEnabled = false;
	[SerializeField]
	private GameObject progressSlider;
	[SerializeField]
	private RectTransform informationDisplay;

	// Start is called before the first frame update
	void Start () {
		informationDisplay.Rotate( new Vector3( informationDisplay.rotation.x, informationDisplay.rotation.y - 45, informationDisplay.rotation.z));
	}

	// Update is called once per frame
	void Update () {
		if (!hackingEnabled ) {
			return; 
		}
	}

	public void EnableHacking (bool enable) {
		informationDisplay.gameObject.SetActive( enable );
		hackingEnabled = enable;
	}
}
