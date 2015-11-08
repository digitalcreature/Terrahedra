using UnityEngine;
using System.Collections;
using TG.Topology;

public class WavyMesh : MonoBehaviour {

	public Mesh mesh;
	public float frequency = 5;
	public float amplitude = .5f;
	public Vector3 offsetNoiseOffset;
	public float offsetNoiseScale = 5;
	public float offsetFactor = 5;
	public bool smoothNormals = false;

	string meshName = "~mesh";

	Graph graph;
	MeshFilter filter;

	void Awake() {
		filter = GetComponent<MeshFilter>();
	}

	void Update() {
		if (filter.mesh != null) {
			if (filter.mesh.name != meshName) {
				mesh = filter.mesh;
				graph = new Graph(mesh);
				mesh = graph.BuildMesh(true);
				filter.mesh = graph.BuildMesh(false);
				filter.mesh.name = meshName;
			}
			Vector3[] mVerts = mesh.vertices;
			Vector3[] mNorms = mesh.normals;
			for (int v = 0; v < mVerts.Length; v ++) {
				mVerts[v] = VertexTransform(mVerts[v], mNorms[v]);
			}
			filter.mesh.vertices = mVerts;
		}
		else {
			mesh = null;
			graph = null;
		}
	}

	Vector3 VertexTransform(Vector3 vertex, Vector3 normal) {
		Vector3 pos = vertex * offsetNoiseScale + offsetNoiseOffset;
		float offset = offsetFactor;
		offset *= Mathf.PerlinNoise(pos.x, pos.y);
		offset *= Mathf.PerlinNoise(pos.z, pos.x);
		offset *= Mathf.PerlinNoise(pos.y, pos.z);
		return vertex + (normal * amplitude * Mathf.Sin(offset + Time.time * frequency * Mathf.PI * 2));
	}
}
