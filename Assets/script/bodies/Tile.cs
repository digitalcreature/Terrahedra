using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	public Color color;
	public float elevation = 0;
	public bool ignoreElevation = true;

	void OnDrawGizmosSelected() {
		Gizmos.matrix = transform.localToWorldMatrix;
		float axisLength = 1 / 2f;
		Gizmos.color = Color.red;
		Gizmos.DrawLine(Vector3.right * - axisLength, Vector3.right * axisLength);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(Vector3.up * -axisLength, Vector3.up * axisLength);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(Vector3.forward * -axisLength, Vector3.forward * axisLength);
	}

}
