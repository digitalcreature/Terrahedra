using UnityEngine;
using System.Collections;
using TG.Topography;

[ExecuteInEditMode]
public class Planetoid : MonoBehaviour {

	public Mesh mesh;

	MeshFilter filter;
	Graph graph;

	void OnEnable() {
		filter = GetComponent<MeshFilter>();
	}

	void OnValidate() {
		filter.sharedMesh = mesh;
		if (mesh != null) {
			graph = new Graph(mesh).BuildFacesByEdgesGraph();
		}
		else {
			graph = null;
		}
	}

	void OnDrawGizmosSelected() {
		Gizmos.matrix = transform.localToWorldMatrix;
		if (graph != null) {
			graph.DrawGizmos();
		}
	}

}
