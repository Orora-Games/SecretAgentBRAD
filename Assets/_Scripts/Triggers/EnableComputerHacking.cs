using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableComputerHacking : MonoBehaviour {
	[SerializeField]
	private ComputerIntelManager computerIntelManager;
	[HideInInspector]
	public bool doneHacking;

	// Start is called before the first frame update
	void Start () {
		//ComputerHackingIntel compuHacking = 
		gameObject.GetComponent<Collider>().isTrigger = true;
	}

	private void OnTriggerEnter ( Collider other ) {
		if ( computerIntelManager != null ) {
			if ( other.transform.tag == "Player" && !doneHacking ) {
				computerIntelManager.EnableHacking();
			}
		}
	}
	private void OnTriggerExit ( Collider other ) {
		if ( other.transform.tag == "Player" && doneHacking ) {
			
		}
	}
}
