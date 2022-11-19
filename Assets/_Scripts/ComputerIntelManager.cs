using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComputerIntelManager : MonoBehaviour {
	public bool hackingEnabled = false;

	[SerializeField] private GameObject progressSlider, intelComputer, boringComputer, hackingEnabler;
	[SerializeField] private RectTransform informationDisplay;
	[SerializeField] private TMP_Text progressBarTimer;
	[SerializeField] private ProgressBarController progressBarController;


	// Start is called before the first frame update
	void Start () {
		progressBarController.progressBarText.text = "Press \"F\" to extract intel.";
	}

	// Update is called once per frame
	void Update () {
		if (!hackingEnabled || !progressBarController ) {
			return; 
		}
		if ( progressBarController.finishedTimer ) {
			GrabIntel();
		}
	}

	public void GrabIntel () {
		boringComputer.SetActive(true);
		progressBarController.gameObject.SetActive( false );
		hackingEnabled = false;
		hackingEnabler.SetActive(false);
		if ( !GameManager.Instance )
			return;
		GameManager.Instance.PickedUpIntel( intelComputer );
	}

	public void EnableHacking (bool enable) {
		progressBarController.gameObject.SetActive( enable );
		hackingEnabled = enable;
	}
}
