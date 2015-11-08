using UnityEngine;
using System.Collections.Generic;
using TG.Topology;
using System.Linq;

public class TerrainGenerator : MonoBehaviour {

	public Tile[] tileset;

	Dictionary<IFace, Tile> terrain;
	GameObject tiles;

	public Dictionary<IFace, Tile> GenerateTerrain(Graph graph) {
		if (tiles != null) {
			Destroy(tiles);
		}
		tiles = new GameObject("tiles");
		tiles.transform.parent = transform;
		terrain = new Dictionary<IFace, Tile>();
		foreach (IFace face in graph.faces) {
			Tile prefab = tileset[Random.Range(0, tileset.Length)];
			Tile tile = Instantiate(prefab) as Tile;
			tile.name = prefab.name;
			tile.transform.parent = tiles.transform;
			tile.transform.up = tiles.transform.TransformDirection(face.normal);
			tile.transform.localPosition = face.center;
			terrain[face] = tile;
		}
		return terrain;
	}
}
