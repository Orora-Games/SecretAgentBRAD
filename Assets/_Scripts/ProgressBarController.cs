using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBarController : MonoBehaviour {
	[SerializeField] private GameObject progressSlider;
	[SerializeField] private RectTransform informationDisplay;
	[SerializeField] private TMP_Text progressBarTimer;
	public TMP_Text progressBarText;
	public bool finishedTimer = false;

	public float timeLimit = 5f;
	private float timeLimitTimer = 0f;
	private Image sliderRenderer;


	// Start is called before the first frame update
	void Start () {
		sliderRenderer = progressSlider.GetComponent<Image>();
		informationDisplay.rotation = new Quaternion( informationDisplay.rotation.x, informationDisplay.rotation.y - 18f, informationDisplay.rotation.z, 45f );
	}

	// Update is called once per frame
	void Update () {
		if ( !sliderRenderer ) {
			return;
		}
		sliderRenderer.fillAmount = 1 - timeLimitTimer / timeLimit;

		if ( !progressSlider ) { //Resets our timeLimitTimer when progressSlider.SetActive(false) has been called
			timeLimitTimer = 0f;
			return;
		}
		if ( Input.GetKey( KeyCode.F ) ) {
			timeLimitTimer += Time.deltaTime;

			progressBarTimer.gameObject.SetActive( true );
			progressBarTimer.text = ( timeLimit - timeLimitTimer ).ToString( "0.00" ) + "s";

			if ( timeLimit < timeLimitTimer ) {
				//disableInformationDisplay();
				finishedTimer = true;
			}
		} else {
			if ( timeLimitTimer == 0f ) {
				progressBarTimer.gameObject.SetActive( false );
				progressBarTimer.text = "5s";
				return;
			}
			timeLimitTimer = ( timeLimitTimer > 0f ) ? timeLimitTimer - Time.deltaTime : 0f;
			progressBarTimer.text = ( timeLimit - timeLimitTimer ).ToString( "0.00" ) + "s";
		}
	}
	private void OnDisable () {
		timeLimitTimer = 0f;
	}
}
