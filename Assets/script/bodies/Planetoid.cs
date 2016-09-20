using UnityEngine;
using System.Collections.Generic;
using TG.Topology;

public class Planetoid : MonoBehaviour {

	public Mesh terrainMesh;

	public MeshFilter water;
	public float waterGrow = -.25f;

	TerrainGenerator generator;
	Graph graph;
	MeshFilter filter;
	Dictionary<IFace, Tile> terrain;


	void Start() {
		generator = GetComponent<TerrainGenerator>();
		filter = GetComponent<MeshFilter>();
		if (terrainMesh != null) {
			graph = new Graph(terrainMesh);
			terrain = generator.GenerateTerrain(graph);
			if (water != null) {
				water.sharedMesh = graph.BuildMesh(false,
					(vertex) => {
						return vertex.position + vertex.normal * waterGrow;
					}
				);
			}
			GenerateMesh();
		}
	}

	void GenerateMesh() {
		int count = generator.tileset.Length;
		Dictionary<Color, Vector2> uvs = new Dictionary<Color, Vector2>();
		Vector2 last = new Vector2((1f / count) / 2f, .5f);
		Vector2 increment = new Vector2(1f / count, 0);
		Texture2D tex = new Texture2D(count, 1);
		tex.filterMode = FilterMode.Point;
		int u = 0;
		foreach (Tile tile in generator.tileset) {
			uvs[tile.color] = last;
			last += increment;
			tex.SetPixel(u ++, 0, tile.color);
		}
		tex.Apply();
		Mesh mesh = graph.BuildMesh(false, 
			(vertex) => {
				float elevation = 0;
				int eCount = 0;
				foreach (IFace face in vertex.faces) {
					Tile tile = terrain[face];
					if (!tile.ignoreElevation) {
						elevation += tile.elevation;
						eCount++;
					}
				}
				if (eCount > 0) {
					elevation /= eCount;
				}
				return vertex.position + vertex.normal * elevation;
			},
			(face) => {
				Tile tile = terrain[face];
				Vector2 uv = uvs[tile.color];
				return new Triangle(uv, uv, uv);
			}
		);
		filter.mesh = mesh;
		GetComponent<Renderer>().material.mainTexture = tex;
	}

}
