using TMPro;
using UnityEngine;

public class SettingsMenu : MonoBehaviour {
	[SerializeField] TMP_Dropdown graphicsLevelSetting;

	// Start is called before the first frame update
	void Start () {
		graphicsLevelSetting.value = QualitySettings.GetQualityLevel();
	}
	public void SetGraphicsLevel (int graphicsLevelSetting) {
		QualitySettings.SetQualityLevel( graphicsLevelSetting );
	}
}
