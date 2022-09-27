using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ColoredFieldOfView : FieldOfView {

	public enum FieldOfViewStates { Detected, Undetected, Alerted }

	public Material detectedMaterial;
	public Material undetectedMaterial;
	public Material alertedMaterial;

	private Renderer rend;

	protected override void Start () {
		base.Start();
		rend = viewMeshFilter.GetComponent<Renderer>();
	}

	public void ChangeColorState ( FieldOfViewStates newColor ) {

		Material selectedMaterial = null;

		switch ( newColor ) {
			case FieldOfViewStates.Detected:
				selectedMaterial = detectedMaterial;
				break;
			case FieldOfViewStates.Undetected:
				selectedMaterial = undetectedMaterial;
				break;
			case FieldOfViewStates.Alerted:
				selectedMaterial = alertedMaterial;
				break;
		}

		rend.material = selectedMaterial;
	}
}
