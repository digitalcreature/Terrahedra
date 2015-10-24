using UnityEngine;
using System.Collections;
using TG.Topography;

[ExecuteInEditMode]
public class Planetoid : MonoBehaviour {

	public Mesh mesh;

	public bool smoothNormals = true;

	public MeshFilter water;
	public float waterGrow = .25f;


	Graph graph;

	void OnValidate() {
		MeshFilter filter = GetComponent<MeshFilter>();
		filter.sharedMesh = mesh;
		if (mesh != null) {
			graph = new Graph(mesh);
			if (water != null) {
				water.sharedMesh = graph.BuildMesh(smoothNormals,
					(vertex) => {
						return vertex.position + vertex.normal * waterGrow;
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
