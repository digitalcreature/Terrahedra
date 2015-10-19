using UnityEngine;
using System.Collections;

public class Planetoid : MonoBehaviour {

	public Mesh mesh;
	public float nodeRadius = .1f;

	NodeGraph graph;

	void OnValidate() {
		if (mesh != null) {
			graph = new MeshTools(mesh).BuildFaceGraph();
		}
		else {
			graph = null;
		}
	}

	void OnDrawGizmosSelected() {
		Gizmos.matrix = transform.localToWorldMatrix;
		if (graph != null) {
			graph.DrawGizmos(nodeRadius);
		}
	}

}
