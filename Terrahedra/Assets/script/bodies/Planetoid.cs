using UnityEngine;
using System.Collections;
using TG.Topography;

[ExecuteInEditMode]
public class Planetoid : MonoBehaviour {

	public Mesh mesh;

	public bool smoothNormals = true;
	public Mesh output;

	MeshFilter filter;
	Graph graph;

	void OnEnable() {
		filter = GetComponent<MeshFilter>();
	}

	void OnValidate() {
		filter.sharedMesh = mesh;
		if (mesh != null) {
			graph = new Graph(mesh);
			output = graph.BuildMesh(smoothNormals);
			output.name = "test";
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
