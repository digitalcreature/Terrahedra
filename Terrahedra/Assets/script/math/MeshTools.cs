using UnityEngine;
using System.Collections.Generic;
using System.Collections;

//util wrapper class for doing things with meshes
public class MeshTools {

	//the mesh we are working with
	Mesh mesh;
	//the triangles of the mesh
	List<Triangle> triangles;

	//constructor
	public MeshTools(Mesh mesh) {
		this.mesh = mesh;
		Refresh();
	}

	//refresh mesh data. call after changing the mesh externally
	public void Refresh() {
		triangles = new List<Triangle>();
		int t = 0;
		while (t < mesh.triangles.Length) {
			Vector3 a = mesh.vertices[mesh.triangles[t ++]];
			Vector3 b = mesh.vertices[mesh.triangles[t ++]];
			Vector3 c = mesh.vertices[mesh.triangles[t ++]];
			triangles.Add(new Triangle(a, b, c));
		}
	}

	//builds a node graph of edge-wise connections between the faces of the mesh
	public NodeGraph BuildFaceGraph() {
		NodeGraph graph = new NodeGraph();
		foreach (Triangle triangle in triangles) {
			Node face = graph.AddNode(triangle.center);
			foreach (Node node in graph.nodes) {
				node.MakeNeighbor(face);
			}
		}
		return graph;
	}
}

//a triangle in space
public struct Triangle {
	
	//the vertices of the triangle
	public Vector3 a, b, c;

	//the area of the triangle
	public float area {
		get {
			return Vector3.Cross(a - b, a - c).magnitude / 2;
		}
	}
	
	//the normal of the triangle
	public Vector3 normal {
		get {
			return Vector3.Cross(c - a, b - a).normalized;
		}
	}

	//the center of the triangle
	public Vector3 center {
		get {
			return (a + b + c) / 3;
		}
	}

	//the edges of the triangle
	public IEnumerable<Segment> edges {
		get {
			yield return new Segment(a, b);
			yield return new Segment(b, c);
			yield return new Segment(c, a);
		}
	}

	//the vertices of the triangle
	public IEnumerable<Vector3> vertices {
		get {
			yield return a;
			yield return b;
			yield return c;
		}
	}

	//constructor
	public Triangle(Vector3 a, Vector3 b, Vector3 c) {
		this.a = a;
		this.b = b;
		this.c = c;
	}

}

//a line segment in space
public struct Segment {

	//the vertices of the segment
	public Vector3 a, b;

	//length of the segment
	public float length {
		get {
			return Mathf.Sqrt(sqrLength);
		}
	}

	//square length of the segment
	public float sqrLength {
		get {
			return (a - b).sqrMagnitude;
		}
	}

	//constructor
	public Segment(Vector3 a, Vector3 b) {
		this.a = a;
		this.b = b;
	}

}