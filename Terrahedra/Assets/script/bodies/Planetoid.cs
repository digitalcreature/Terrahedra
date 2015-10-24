using UnityEngine;
using System.Collections;
using TG.Topography;

[ExecuteInEditMode]
public class Planetoid : MonoBehaviour {

	public Mesh mesh;

	public bool smoothNormals = true;

	public MeshFilter output;
	public float grow = 0;

	Graph graph;

	void OnValidate() {
		MeshFilter filter = GetComponent<MeshFilter>();
		filter.sharedMesh = mesh;
		if (mesh != null) {
			graph = new Graph(mesh);
			if (output != null) {
				output.sharedMesh = graph.BuildMesh(smoothNormals,
					(vertex) => {
						return vertex.position + vertex.normal * grow;
					}
				);
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
