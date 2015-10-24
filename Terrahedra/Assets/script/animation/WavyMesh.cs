using UnityEngine;
using System.Collections;
using TG.Topography;

public class WavyMesh : MonoBehaviour {

	public float frequency = 5;
	public float amplitude = .5f;
	public Vector3 offsetNoiseOffset;
	public float offsetNoiseScale = 5;
	public float offsetFactor = 5;
	public bool smoothNormals = false;

	Graph graph;
	MeshFilter filter;

	void Awake() {
		filter = GetComponent<MeshFilter>();
		graph = new Graph(filter.mesh);
	}

	void Update() {
		if (filter.mesh.name != this.name) {
			graph = new Graph(filter.mesh);
		}
		filter.mesh = graph.BuildMesh(smoothNormals, 
			(vertex) => {
				Vector3 pos = vertex.position * offsetNoiseScale + offsetNoiseOffset;
				float offset = offsetFactor;
				offset *= Mathf.PerlinNoise(pos.x, pos.y);
				offset *= Mathf.PerlinNoise(pos.x, pos.z);
				offset *= Mathf.PerlinNoise(pos.y, pos.z);
				return vertex.position + (vertex.normal * amplitude * Mathf.Sin(offset + Time.time * frequency * Mathf.PI * 2));
			}
		);
		filter.mesh.name = this.name;
	}
}
