using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static GameManager;

public class CaughtController : MonoBehaviour {
	private List<Transform> visibleTargets;
	private FieldOfView fov;
	[SerializeField] GameObject explosionPrefab;

	private void Start () {
		fov = gameObject.GetComponent<FieldOfView>();
	}

	// Update is called once per frame
	void Update () {
		if ( !fov ) { /* gameObject.transform.parent.name - Might end up going above the prefab-name, but that's fine-ish */
			Debug.LogError( "The object \"" + gameObject.transform.parent.name + "\" needs a FieldOfView-component. " );
			return;
		}

		/* .. if this Entity (Enemy) sees Player (target); We're trusting the FieldOfView-component to spot our enemies. ... */
		if ( fov && fov.visibleTargets.Count > 0 ) {
			/* We're trusting the FOV- to know when it sees a player */
			if (!GameManager.Instance ) return;
			if (GameManager.Instance.GetGameState() != GameState.Playing) return;
			
			GameManager.Instance.ChangeGameState( GameState.GameOver );
			GameObject explosion = Instantiate( explosionPrefab );
			explosion.transform.position = fov.visibleTargets[ 0 ].position;
			fov.visibleTargets[0].gameObject.SetActive( false );

		}
	}
}
