using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InteractionHandler : MonoBehaviour {
	public bool interactionEnabled = false;
	[Header( "Information Display" )]
	[SerializeField] private GameObject progressSlider;
	public GameObject interactionEnablerObject;
	[SerializeField] private RectTransform informationDisplay;
	[SerializeField] private TMP_Text progressBarTimer;
	[SerializeField] private ProgressBarController progressBarController;
	[SerializeField] private string interactionAction = "";
	[SerializeField] private Dictionary<string,string> interactions = new Dictionary<string, string>() {
		{ "hacking", "extract information" },
		{ "vents", "climb through vent" }
	};
	private GameObject interactionTarget;
	public bool disableEnterTrigger = false;

	[Header( "Action: Computer Hacking" )]
	[SerializeField] private GameObject intelComputer;
	[SerializeField] private GameObject boringComputer;

	[Header( "Action: Vent Exploration" )]
	public VentController ventController;


	// Start is called before the first frame update
	void Start () {
		if ( interactionAction == "") return;
		progressBarController.progressBarText.text = "Press \"F\" to " + interactions[interactionAction] + ".";

	}

	// Update is called once per frame
	void Update () {
		if (!interactionEnabled || 
			!progressBarController || 
			!progressBarController.finishedTimer 
			) return;

		switch ( interactionAction ) {
			case "hacking":
				GrabIntel();
				break;
			case "vents":
				ClimbThroughVent();
				break;
			default:
				break;
		}
		progressBarController.finishedTimer = false;
	}

	private void ClimbThroughVent () {
		if ( ventController == null || interactionTarget == null) return;

		progressBarController.gameObject.SetActive( false );

		ventController.MoveToLocation( transform, interactionTarget );
		interactionEnabled = false;
	}

	public void GrabIntel () {
		boringComputer.SetActive( true );

		progressBarController.gameObject.SetActive( false );
		interactionEnabled = false;
		interactionEnablerObject.SetActive( false );

		if ( !GameManager.Instance ) return;
		GameManager.Instance.PickedUpIntel( intelComputer );
	}

	public void EnableInteraction (bool enable, GameObject localInteractionTarget ) {
		progressBarController.gameObject.SetActive( enable );
		interactionEnabled = enable;
		interactionTarget = localInteractionTarget;
	}
}
