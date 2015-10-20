using UnityEngine;
using System.Collections;
using TG.MeshTools;

public class Planetoid : MonoBehaviour {

	public Mesh mesh;

	NodeGraph graph;
	WorkingMesh workingMesh;

	void OnValidate() {
		if (mesh != null) {
			workingMesh = new WorkingMesh(mesh);
		}
		else {
			graph = null;
		}
	}

	void OnDrawGizmosSelected() {
		Gizmos.matrix = transform.localToWorldMatrix;
		if (workingMesh != null) {
			workingMesh.DrawGizmos();
		}
		if (graph != null) {
			graph.DrawGizmos();
		}
	}

}
