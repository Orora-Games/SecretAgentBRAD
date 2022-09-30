using UnityEngine;

/// <summary>
///		This script rotates the object it is attached to.
/// </summary>
public class ItemRotationator : MonoBehaviour {
	public float rotationSpeed = 30f;

	void Update () {
		transform.Rotate( 0f, 10f * rotationSpeed * Time.deltaTime, 0f, Space.Self );
	}
}
