using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System;

namespace TG.Topography {

	public class Graph {

		//vertices
		HashSet<Vertex> vertexSet;
		Dictionary<Vector3, Vertex> vertexCache;
		public IEnumerable<IVertex> vertices {
			get {
				foreach (Vertex vertex in vertexSet) {
					yield return vertex;
				}
			}
		}

		//faces
		HashSet<Face> faceSet;
		Dictionary<Triangle, Face> faceCache;
		public IEnumerable<IFace> faces {
			get {
				foreach (Face face in faceSet) {
					yield return face;
				}
			}
		}

		//edges
		HashSet<Edge> edgeSet;
		Dictionary<Segment, Edge> edgeCache;
		public IEnumerable<IEdge> edges {
			get {
				foreach (Edge edge in edgeSet) {
					yield return edge;
				}
			}
		}

		public Graph() {
			vertexSet = new HashSet<Vertex>();
			vertexCache = new Dictionary<Vector3, Vertex>();
			faceSet = new HashSet<Face>();
			faceCache = new Dictionary<Triangle, Face>();
			edgeSet = new HashSet<Edge>();
			edgeCache = new Dictionary<Segment, Edge>();
		}

		public Graph(Mesh mesh) : this() {
			AddMesh(mesh);
		}

		public void AddMesh(Mesh mesh) {
			Vector3[] mVerts = mesh.vertices;
			for (int v = 0; v < mVerts.Length; v++) {
				AddVertex(mVerts[v]);
			}
			int[] mTris = mesh.triangles;
			int t = 0;
			while (t < mTris.Length) {
				Vertex a = vertexCache[mVerts[mTris[t ++]]];
				Vertex b = vertexCache[mVerts[mTris[t ++]]];
				Vertex c = vertexCache[mVerts[mTris[t ++]]];
				AddFace(a, b, c);
			}
		}

		public IVertex AddVertex(Vector3 position) {
			Vertex vertex;
			if (vertexCache.ContainsKey(position)) {
				vertex = vertexCache[position];
			}
			else {
				vertex = new Vertex(this, position);
				vertexSet.Add(vertex);
				vertexCache[position] = vertex;
			}
			return vertex;
		}

		public IFace AddFace(IVertex vertA, IVertex vertB, IVertex vertC) {
			if (vertA.graph == this && vertB.graph == this && vertC.graph == this) {
				Vertex a = (Vertex) vertA;
				Vertex b = (Vertex) vertB;
				Vertex c = (Vertex) vertC;
				Triangle triangle = new Triangle(a, b, c);
				Face face;
				if (faceCache.ContainsKey(triangle)) {
					face = faceCache[triangle];
				}
				else {
					face = new Face(this, a, b, c);
					faceSet.Add(face);
					faceCache[triangle] = face;
				}
				return face;
			}
			throw new ArgumentException("Cannot create face: at least one vertex not belong to this graph.");
		}

		public IEdge AddEdge(IVertex vertA, IVertex vertB) {
			if (vertA.graph == this && vertB.graph == this) {
				Vertex a = (Vertex) vertA;
				Vertex b = (Vertex) vertB;
				Segment segment = new Segment(a, b);
				Edge edge;
				if (edgeCache.ContainsKey(segment)) {
					edge = edgeCache[segment];
				}
				else {
					edge = new Edge(this, a, b);
					edgeSet.Add(edge);
					edgeCache[segment] = edge;
				}
				return edge;
			}
			throw new ArgumentException("Cannot create edge: at least one vertex not belong to this graph.");
		}

		//build a graph of the faces of this graph along edges
		public Graph BuildFacesByEdgesGraph() {
			Graph graph = new Graph();
			Dictionary<Face, IVertex> faceVertices = new Dictionary<Face, IVertex>();
			foreach (Face face in faceSet) {
				IVertex faceVertex = graph.AddVertex(face.center);
				faceVertices[face] = faceVertex;
			}
			foreach (Face face in faceSet) {
				IVertex faceVertex = faceVertices[face];
				foreach (Edge edge in face.edges) {
					foreach (Face neighbor in edge.faces) {
						if (neighbor != face) {
							graph.AddEdge(faceVertex, faceVertices[neighbor]);
						}
					}
				}
			}
			return graph;
		}

		//build a graph of the faces of this graph along vertices
		public Graph BuildFacesByVerticesGraph() {
			Graph graph = new Graph();
			Dictionary<Face, IVertex> faceVertices = new Dictionary<Face, IVertex>();
			foreach (Face face in faceSet) {
				IVertex faceVertex = graph.AddVertex(face.center);
				faceVertices[face] = faceVertex;
			}
			foreach (Face face in faceSet) {
				IVertex faceVertex = faceVertices[face];
				foreach (IVertex vertex in face.vertices) {
					foreach (Face neighbor in vertex.faces) {
						if (neighbor != face) {
							graph.AddEdge(faceVertex, faceVertices[neighbor]);
						}
					}
				}
			}
			return graph;
		}

		//draw this graph with unity3d's gizmo system
		public void DrawGizmos(Color vertexColor, Color faceColor, Color edgeColor, float dotRadius = 0.005f, float normalLength = .05f) {
			Matrix4x4 mat = Handles.matrix;
			Handles.matrix = Gizmos.matrix;
			Handles.color = vertexColor;
			foreach (Vertex vertex in vertexSet) {
				Handles.DotCap(0, vertex.position, Quaternion.identity, dotRadius);
				Handles.DrawLine(vertex.position, vertex.position + vertex.normal * normalLength);
			}
			Handles.color = faceColor;
			foreach (Face face in faceSet) {
				Handles.DotCap(0, face.center, Quaternion.identity, dotRadius);
				Handles.DrawLine(face.center, face.center + face.normal * normalLength);
			}
			Handles.color = edgeColor;
			foreach (Edge edge in edgeSet) {
				Handles.DotCap(0, edge.center, Quaternion.identity, dotRadius);
				Handles.DrawLine(edge.a.position, edge.b.position);
				Handles.DrawLine(edge.center, edge.center + edge.normal * normalLength);
			}
			Handles.matrix = mat;

		}
		public void DrawGizmos(float dotRadius = 0.005f, float normalLength = .05f) {
			DrawGizmos(new Color(0.7f, 1, 1), new Color(1, 0.7f, 1), new Color(1, 1, 0.7f), dotRadius, normalLength);
		}

		class Vertex : MeshComponent, IVertex {

			HashSet<Face> faceSet;
			public IEnumerable<IFace> faces {
				get {
					foreach (Face face in faceSet) {
						yield return face;
					}
				}
			}

			HashSet<Edge> edgeSet;
			public IEnumerable<IEdge> edges {
				get {
					foreach (Edge edge in edgeSet) {
						yield return edge;
					}
				}
			}

			public Vector3 position { get; private set; }

			public Vector3 normal {
				get {
					Vector3 normal = Vector3.zero;
					foreach (Face face in faces) {
						normal += face.normal;
					}
					return normal.normalized;
				}
			}

			public Vertex(Graph graph, Vector3 position) : base(graph) {
				this.position = position;
				faceSet = new HashSet<Face>();
				edgeSet = new HashSet<Edge>();
			}

			public void AddFace(Face face) {
				faceSet.Add(face);
			}

			public void AddEdge(Edge edge) {
				edgeSet.Add(edge);
			}

			public override int GetHashCode() {
				return position.GetHashCode();
			}
		}

		class Face : MeshComponent, IFace {

			public readonly Vertex a, b, c;
			public IEnumerable<IVertex> vertices {
				get {
					yield return a;
					yield return b;
					yield return c;
				}
			}

			public readonly Edge ab, bc, ca;
			public IEnumerable<IEdge> edges {
				get {
					yield return ab;
					yield return bc;
					yield return ca;
				}
			}

			public Triangle triangle {
				get {
					return new Triangle(a.position, b.position, c.position);
				}
			}

			public Vector3 center {
				get {
					return triangle.center;
				}
			}

			public Vector3 normal {
				get {
					return triangle.normal;
				}
			}

			public Face(Graph graph, Vertex a, Vertex b, Vertex c) : base(graph) {
				this.a = a;
				this.b = b;
				this.c = c;
				ab = (Edge) graph.AddEdge(a, b);
				bc = (Edge) graph.AddEdge(b, c);
				ca = (Edge) graph.AddEdge(c, a);
				foreach (Edge edge in edges) {
					edge.AddFace(this);
				}
				foreach (Vertex vertex in vertices) {
					vertex.AddFace(this);
				}
			}

			public override int GetHashCode() {
				return triangle.GetHashCode();
			}

		}

		class Edge : MeshComponent, IEdge {

			public readonly Vertex a, b;
			public IEnumerable<IVertex> vertices {
				get {
					yield return a;
					yield return b;
				}
			}

			HashSet<Face> faceSet;
			public IEnumerable<IFace> faces {
				get {
					foreach (Face face in faceSet) {
						yield return face;
					}
				}
			}

			public Segment segment {
				get {
					return new Segment(a, b);
				}
			}

			public Vector3 center {
				get {
					return segment.center;
				}
			}

			public Vector3 normal {
				get {
					Vector3 normal = Vector3.zero;
					foreach (Face face in faceSet) {
						normal += face.normal;
					}
					return normal.normalized;
				}
			}

			public Edge(Graph graph, Vertex a, Vertex b) : base(graph) {
				this.a = a;
				this.b = b;
				faceSet = new HashSet<Face>();
				foreach (Vertex vertex in vertices) {
					vertex.AddEdge(this);
				}
			}

			public void AddFace(Face face) {
				faceSet.Add(face);
			}

			public override int GetHashCode() {
				return segment.GetHashCode();
			}

		}

		abstract class MeshComponent : IMeshComponent {

			public Graph graph { get; private set; }

			public MeshComponent(Graph graph) {
				this.graph = graph;
			}

		}

	}

	public interface IVertex : IMeshComponent {

		IEnumerable<IFace> faces { get; }

		IEnumerable<IEdge> edges { get; }

		Vector3 position { get; }

		Vector3 normal { get; }

	}

	public interface IFace : IMeshComponent {

		IEnumerable<IVertex> vertices { get; }

		IEnumerable<IEdge> edges { get; }

		Triangle triangle { get; }

		Vector3 center { get; }

		Vector3 normal { get; }

	}

	public interface IEdge : IMeshComponent {

		IEnumerable<IVertex> vertices { get; }

		IEnumerable<IFace> faces { get; }

		Segment segment { get; }

		Vector3 center { get; }

		Vector3 normal { get; }

	}

	public interface IMeshComponent {

		Graph graph { get; }

	}

	public struct Triangle {

		public Vector3 a, b, c;

		public Vector3 center {
			get {
				return (a + b + c) / 3;
			}
		}

		public Vector3 normal {
			get {
				return Vector3.Cross(b - a, c - a).normalized;
			}
		}

		public Triangle(IVertex a, IVertex b, IVertex c) : this(a.position, b.position, c.position) { }

		public Triangle(Vector3 a, Vector3 b, Vector3 c) {
			this.a = a;
			this.b = b;
			this.c = c;
		}

		public Triangle Rotate(int rotations = 1) {
			if (rotations == 0) {
				return this;
			}
			else {
				rotations %= 2;
				rotations += rotations < 0 ? 2 : 0;
				return new Triangle(c, a, b).Rotate(rotations - 1);
			}
		}

		public override bool Equals(object obj) {
			return obj is Triangle && (Triangle) obj == this;
		}

		public static bool operator ==(Triangle a, Triangle b) {
			return (
				(a.a == b.a && a.b == b.b && a.c == b.c) ||
				(a.a == b.b && a.b == b.c && a.c == b.a) ||
				(a.a == b.c && a.b == b.a && a.c == b.b)
            );

		}

		public static bool operator !=(Triangle a, Triangle b) {
			return !(a == b);
		}

		public override int GetHashCode() {
			return a.GetHashCode() ^ b.GetHashCode() ^ c.GetHashCode() ^ normal.GetHashCode();
		}

	}

	public struct Segment {

		public Vector3 a, b;

		public Vector3 center {
			get {
				return (a + b) / 2;
			}
		}

		public Segment(IVertex a, IVertex b) : this(a.position, b.position) { }

		public Segment(Vector3 a, Vector3 b) {
			this.a = a;
			this.b = b;
		}

		public override bool Equals(object obj) {
			return obj is Segment && (Segment) obj == this;
		}

		public static bool operator ==(Segment a, Segment b) {
			return (
				(a.a == b.a && a.b == b.b) ||
				(a.a == b.b && a.b == b.a)
            );
		}

		public static bool operator !=(Segment a, Segment b) {
			return !(a == b);
		}

		public override int GetHashCode() {
			return a.GetHashCode() ^ b.GetHashCode();
		}
	}

}