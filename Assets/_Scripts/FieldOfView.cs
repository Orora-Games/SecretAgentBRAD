using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour {
	/* Source: Sebastian League Youtube channel
	 *    Field of view visualisation (E01) https://www.youtube.com/watch?v=rQG9aUWarwE
	 *    Field of view visualisation (E02) https://www.youtube.com/watch?v=73Dc5JTCmKI */

	public float viewRadius;
	[Range( 0, 360 )]
	public float viewAngle;
	public float targetSpottingFrequency = 0.2f;
	private float targetSpottingTime = 0f;

	public LayerMask targetMask;
	public LayerMask obstacleMask;

	public int edgeResolveIterations;
	public float edgeDistanceThreshold;

	public float meshResolution;
	public MeshFilter viewMeshFilter;
	private Mesh viewMesh;

	[HideInInspector]
	public List<Transform> visibleTargets = new List<Transform>();
	private Vector3 visualizationDetectionHeight;

	protected virtual void Start () {
		viewMesh = new Mesh();
		viewMesh.name = "View Mesh";
		viewMeshFilter.mesh = viewMesh;

		SetDetectionHeight( viewMeshFilter.transform );
	}

	private void SetDetectionHeight ( Transform viewVisualization ) {
		/* Find floor, so we can use floor.transform.position.y to find the floor height, then set ViewVisualization to floorHeight+some 
		 *	Example: https://docs.unity3d.com/ScriptReference/RaycastHit-distance.html */
		RaycastHit hit;
		if ( Physics.Raycast( transform.parent.position, Vector3.down, out hit, Mathf.Infinity ) ) {

			if ( hit.transform.name == "Floor" ) {
				float visualizationHeight = 0f;
				visualizationHeight = hit.point.y + 0.05f;

				visualizationDetectionHeight = new Vector3( viewVisualization.position.x, visualizationHeight, viewVisualization.position.z );
				viewVisualization.position = visualizationDetectionHeight;
			}
		}
	}

	private void Update () {
		targetSpottingTime += Time.deltaTime;
		if ( targetSpottingTime >= targetSpottingFrequency ) {
			targetSpottingTime = 0f;
			FindVisibleTargets();
		}
		if ( visualizationDetectionHeight == Vector3.zero) {
			EnemyController enemyController = gameObject.transform.GetComponent<EnemyController>();
			if ( enemyController != null) {
				visualizationDetectionHeight = enemyController.visualizationDetectionHeight;
			}
		}
	}

	/* We're using LateUpdate-function to allow movement/rotation to happen before we redraw the fov. */
	private void LateUpdate () {
		DrawFieldOfView();
	}

	void FindVisibleTargets () {
		/* Make sure our visibleTargets list doesn't clog up with duplicates ...*/
		visibleTargets.Clear();

		Vector3 visualizationDetectionPosition = (visualizationDetectionHeight != null ) ? new Vector3( transform.position.x, visualizationDetectionHeight.y, transform.position.z ): transform.position;
		/* .. Find every Player within viewRadius ...*/
		Collider[] targetsInViewRadius = Physics.OverlapSphere( new Vector3(transform.position.x, visualizationDetectionHeight.y, transform.position.z), viewRadius, targetMask );

		/* If our OverlapSphere finds no colliders within the area, then get out of this function (none of the code below return will be run); */
		if ( targetsInViewRadius.Length == 0 )
			return;

		/* .. Go through the(se) targets ... */
		for ( int i = 0; i < targetsInViewRadius.Length; i++ ) {
			/* .. Get the targets transform ... */
			Transform target = targetsInViewRadius[ i ].transform;
			/* .. find out how far away from Enemy the Player(target) is ... */
			Vector3 dirToTarget = ( target.position - visualizationDetectionPosition ).normalized;
			/* .. Check that the target is within the viewAngle (Pizza-slice Enemy is seeing within) ... */
			if ( Vector3.Angle( transform.forward, dirToTarget ) < viewAngle / 2f ) {
				/* .. Find the distance to target ... */
				float dstToTarget = Vector3.Distance( visualizationDetectionPosition, target.position );

				/* .. Check if there is an obstacle between target and Enemy ... */
				if ( !Physics.Raycast( visualizationDetectionPosition, dirToTarget, dstToTarget, obstacleMask ) ) {
					/* .. If there is no obstacle, check if this target is already in the list, if not add target to visible targets. */
					if ( !visibleTargets.Contains( target ) )
						visibleTargets.Add( target );
				}
			}
		}
	}

	void DrawFieldOfView () {
		/* stepCount will explain in how many steps we are going to draw the fov ... */
		int stepCount = Mathf.RoundToInt( viewAngle * meshResolution );
		/* .. stepAngleSize tells us how big of an angle there will be between each step ... */
		float stepAngleSize = viewAngle / stepCount;
		/* .. viewPoints will contain all the points needed to build our vertices ... */
		List<Vector3> viewPoints = new List<Vector3>();
		/* .. oldViewCast will contain the previously iterated viewCast for a check we're doing ... */
		ViewCastInfo oldViewCast = new ViewCastInfo();

		/* .. our main iterative for-loop ... */
		for ( int i = 0; i <= stepCount; i++ ) {
			/* .. angle describes the next angle we're going to be working with ... */
			float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
			/* .. newViewCast contains our current ViewCast ... */
			ViewCastInfo newViewCast = ViewCast( angle );

			/* .. this will run if i is above 0 (which happens every iteration after the first) ... */
			if ( i > 0 ) {
				/* .. edgeDistanceThresholdExceeded checks if two raycast hits exceed the distance threshold set ... */
				bool edgeDistanceThresholdExceeded = Mathf.Abs( oldViewCast.dst - newViewCast.dst ) > edgeDistanceThreshold;
				/* .. If old and new ViewCast did not hit or did hit and edgeDistanceThresholdExceeded is not exceeded ... */
				if ( oldViewCast.hit != newViewCast.hit || ( oldViewCast.hit && newViewCast.hit && edgeDistanceThresholdExceeded ) ) {
					/* .. Find the edge using oldViewCast and newViewCast ... */
					EdgeInfo edge = FindEdge( oldViewCast, newViewCast );

					if ( edge.pointA != Vector3.zero ) {
						viewPoints.Add( edge.pointA );
					}
					if ( edge.pointB != Vector3.zero ) {
						viewPoints.Add( edge.pointB );
					}
				}
			}

			viewPoints.Add( newViewCast.point );

			/* .. Sets oldViewCast to newViewCast, we're using this in our next iteration ... */
			oldViewCast = newViewCast;
		}
		/* .. vertexCount, vertices, and triangles array number logic can be found at https://youtu.be/73Dc5JTCmKI?t=518 ... */
		int vertexCount = viewPoints.Count + 1;
		Vector3[] vertices = new Vector3[ vertexCount ];
		int[] triangles = new int[ ( vertexCount - 2 ) * 3 ];

		/* .. Our first vertice is going to be Vector3.zero ... */
		vertices[ 0 ] = Vector3.zero;

		/* .. vertexCount - 1 because we have made the first vertex already. ... */
		for ( int i = 0; i < vertexCount - 1; i++ ) {
			/* .. [i + 1] we're skipping making the first vertice, since we already have it (Vertex3.zero) ... */
			vertices[ i + 1 ] = transform.InverseTransformPoint( viewPoints[ i ] );

			if ( i < vertexCount - 2 ) {
				/* .. First triangle starts at the origin vertex (zero)  ... */
				triangles[ i * 3 ] = 0;
				/* ..  ... */
				triangles[ i * 3 + 1 ] = i + 1;
				triangles[ i * 3 + 2 ] = i + 2;
			}
		}

		/* .. clear viewMesh to get rid of old data ... */
		viewMesh.Clear();
		/* .. give viewMesh our vertices ... */
		viewMesh.vertices = vertices;
		/* .. give viewMesh our triangles ... */
		viewMesh.triangles = triangles;


		/* .. have viewMesh recalculate normals (render the new fov) ... */
		viewMesh.RecalculateNormals();
	}

	EdgeInfo FindEdge ( ViewCastInfo minViewCast, ViewCastInfo maxViewCast ) {
		float minAngle = minViewCast.angle;
		float maxAngle = maxViewCast.angle;
		Vector3 minPoint = Vector3.zero;
		Vector3 maxPoint = Vector3.zero;

		for ( int i = 0; i < edgeResolveIterations; i++ ) {
			float angle = ( minAngle + maxAngle ) / 2;
			ViewCastInfo newViewCast = ViewCast( angle );
			bool edgeDistanceThresholdExceeded = Mathf.Abs( minViewCast.dst - newViewCast.dst ) > edgeDistanceThreshold;
			if ( newViewCast.hit == minViewCast.hit && !edgeDistanceThresholdExceeded ) {
				minAngle = angle;
				minPoint = newViewCast.point;
			} else {
				maxAngle = angle;
				maxPoint = newViewCast.point;
			}
		}
		return new EdgeInfo( minPoint, maxPoint );
	}

	public Vector3 DirFromAngle ( float angleFromDegrees, bool angleIsGlobal ) {
		if ( !angleIsGlobal ) {
			angleFromDegrees += transform.eulerAngles.y;
		}
		return new Vector3( Mathf.Sin( angleFromDegrees * Mathf.Deg2Rad ), 0, Mathf.Cos( angleFromDegrees * Mathf.Deg2Rad ) );
	}

	ViewCastInfo ViewCast ( float globalAngle ) {

		Vector3 dir = DirFromAngle( globalAngle, true );
		RaycastHit hit;

		if ( Physics.Raycast( transform.position, dir, out hit, viewRadius, obstacleMask ) ) {
			return new ViewCastInfo( true, hit.point, hit.distance, globalAngle );
		} else {
			return new ViewCastInfo( false, transform.position + dir * viewRadius, viewRadius, globalAngle );
		}
	}

	public struct ViewCastInfo {
		public bool hit;
		public Vector3 point;
		public float dst;
		public float angle;

		public ViewCastInfo ( bool _hit, Vector3 _point, float _dst, float _angle ) {
			hit = _hit;
			point = _point;
			dst = _dst;
			angle = _angle;
		}
	}

	public struct EdgeInfo {
		public Vector3 pointA;
		public Vector3 pointB;
		public EdgeInfo ( Vector3 _pointA, Vector3 _pointB ) {
			pointA = _pointA;
			pointB = _pointB;

		}
	}
}
