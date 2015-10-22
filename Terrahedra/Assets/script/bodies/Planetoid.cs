using UnityEngine;
using System.Collections;
using TG.Topography;

[ExecuteInEditMode]
public class Planetoid : MonoBehaviour {

	public Mesh mesh;

	public bool smoothNormals = true;
	public Mesh output;

	Graph graph;

	void OnValidate() {
		MeshFilter filter = GetComponent<MeshFilter>();
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
