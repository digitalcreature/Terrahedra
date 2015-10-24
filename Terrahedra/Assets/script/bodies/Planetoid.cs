using UnityEngine;
using System.Collections;
using TG.Topography;

[ExecuteInEditMode]
public class Planetoid : MonoBehaviour {

	public Mesh mesh;

	public bool smoothNormals = true;
	public Mesh output;

	public MeshFilter sky;

	Graph graph;

	void OnValidate() {
		MeshFilter filter = GetComponent<MeshFilter>();
		filter.sharedMesh = mesh;
		if (mesh != null) {
			graph = new Graph(mesh);
			output = graph.BuildInvertedGraph().BuildMesh(smoothNormals);
			output.name = "test";
			if (sky != null) {
				sky.sharedMesh = output;
			}

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
