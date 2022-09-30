using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( FieldOfView ) )]
public class FieldOfViewEditor : Editor {
	/* Source: Sebastian League Youtube channel
     *    Field of view visualisation (E01) https://www.youtube.com/watch?v=rQG9aUWarwE */

	private void OnSceneGUI () {
		/* Get Field of View transform ... */
		FieldOfView fov = (FieldOfView)target;

		/* .. Set first drawing colour to white ... */
		Handles.color = Color.white;

		/* .. Draw circle around FOV-entity ... */
		Handles.DrawWireArc( fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius );

		/* .. Calculate angles for FOV-slice ...*/
		Vector3 viewAngleA = fov.DirFromAngle( -fov.viewAngle / 2, false );
		Vector3 viewAngleB = fov.DirFromAngle( fov.viewAngle / 2, false );

		/* .. Draw lines out to the circle ... */
		Handles.DrawLine( fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius );
		Handles.DrawLine( fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius );

		/* .. Change drawing colour to red ... */
		Handles.color = Color.white;

		/* .. Go through each of the visibleTargets and ... */
		foreach ( Transform visibleTarget in fov.visibleTargets ) {
			/* .. Draw a line to that target. */
			Handles.DrawLine( fov.transform.position, visibleTarget.position );
		}
	}
}
