#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace TG.Topology {

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

		//constructor
		public Graph() {
			vertexSet = new HashSet<Vertex>();
			vertexCache = new Dictionary<Vector3, Vertex>();
			faceSet = new HashSet<Face>();
			faceCache = new Dictionary<Triangle, Face>();
			edgeSet = new HashSet<Edge>();
			edgeCache = new Dictionary<Segment, Edge>();
		}
		//constructor with initial mesh(es)
		public Graph(params Mesh[] meshes) : this() {
			AddMeshes(meshes);
		}

		//add mesh geometry to this graph
		public void AddMesh(Mesh mesh) {
			if (mesh != null) {
				Vector3[] mVerts = mesh.vertices;
				for (int v = 0; v < mVerts.Length; v++) {
					AddVertex(mVerts[v]);
				}
				int[] mTris = mesh.triangles;
				int t = 0;
				while (t < mTris.Length) {
					Vertex a = vertexCache[mVerts[mTris[t++]]];
					Vertex b = vertexCache[mVerts[mTris[t++]]];
					Vertex c = vertexCache[mVerts[mTris[t++]]];
					AddFace(a, b, c);
				}
			}
		}
		//add multiple meshes to this graph
		public void AddMeshes(params Mesh[] meshes) {
			foreach (Mesh mesh in meshes) {
				AddMesh(mesh);
			}
		}

		//ensure that a vertex at [position] is a part of this graph
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
		//ensure that a face between these three vertices is a part of this graph
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
		public IFace AddFace(Vector3 vertA, Vector3 vertB, Vector3 vertC) {
			return AddFace(AddVertex(vertA), AddVertex(vertB), AddVertex(vertC));
		}
		public IFace AddFace(Triangle triangle) {
			return AddFace(triangle.a, triangle.b, triangle.c);
		}
		//ensure that an edge between these two verticies is a part of this graph
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
		public IEdge AddEdge(Vector3 vertA, Vector3 vertB) {
			return AddEdge(AddVertex(vertA), AddVertex(vertB));
		}
		public IEdge AddEdge(Segment segment) {
			return AddEdge(segment.a, segment.b);
		}

		//ensure that a vertex is not a part of this graph
		public bool RemoveVertex(IVertex vertex) {
			if (vertex.graph == this) {
				foreach (Face face in vertex.faces) {
					RemoveFace(face);
				}
				foreach (Edge edge in vertex.edges) {
					RemoveEdge(edge);
				}
				vertexSet.Remove((Vertex) vertex);
				((Vertex) vertex).Destroy();
				return true;
			}
			else {
				return false;
			}
		}
		//ensure that a face is not a part of this graph
		public bool RemoveFace(IFace face) {
			if (face.graph == this) {
				foreach (Vertex vertex in face.vertices) {
					vertex.RemoveFace((Face) face);
				}
				foreach (Edge edge in face.edges) {
					edge.RemoveFace((Face) face);
				}
				faceSet.Remove((Face) face);
				faceCache.Remove(face.triangle);
				((Face) face).Destroy();
				return true;
			}
			else {
				return false;
			}
		}
		//ensure that an edge is not a part of this graph
		public bool RemoveEdge(IEdge edge) {
			if (edge.graph == this) {
				foreach (Face face in edge.faces) {
					RemoveFace(face);
				}
				edgeSet.Remove((Edge) edge);
				edgeCache.Remove(edge.segment);
				((Edge) edge).Destroy();
				return true;
			}
			else {
				return false;
			}
		}

		//get a vertex on this graph; return null if it doesnt exist
		public IVertex GetVertex(Vector3 position) {
			return vertexCache.ContainsKey(position) ? vertexCache[position] : null;
		}
		//get a face on this graph; return null if it doesnt exist
		public IFace GetFace(Triangle triangle) {
			return faceCache.ContainsKey(triangle) ? faceCache[triangle] : null;
		}
		public IFace GetFace(IVertex vertA, IVertex vertB, IVertex vertC) {
			return GetFace(new Triangle(vertA.position, vertB.position, vertC.position));
		}
		//get a edge on this graph; return null if it doesnt exist
		public IEdge GetEdge(Segment segment) {
			return edgeCache.ContainsKey(segment) ? edgeCache[segment] : null;
		}
		public IEdge GetEdge(IVertex vertA, IVertex vertB) {
			return GetEdge(new Segment(vertA.position, vertB.position));
		}

		//build a unity mesh from this graph
		//not for realtime animation (might be able to optimize but its unliekely)
		//optional delegates for simple procedural transformation
		public Mesh BuildMesh(bool smoothNormals = true, Func<IVertex, Vector3> vertexTransform = null, Func<IFace, Triangle> uvTransform = null, Func<IFace, Triangle> normalTransform = null, Mesh mesh = null) {
			int vertexCount = faceSet.Count * 3;
			Vector3[] mVerts;
			Vector3[] mNorms;
			Vector2[] mUVs;
			int[] mTris = new int[vertexCount];
			if (mesh == null) {
				mesh = new Mesh();
				mVerts = new Vector3[vertexCount];
				mNorms = new Vector3[vertexCount];
				mUVs = new Vector2[vertexCount];
				mTris = new int[vertexCount];
			}
			else {
				mVerts = mesh.vertices;
				mNorms = mesh.normals;
				mUVs = mesh.uv;
				mTris = mesh.triangles;
			}
			int v = 0;
			foreach (Face face in faceSet) {
				Triangle normals;
				if (normalTransform == null) {
					if (smoothNormals) {
						normals = new Triangle(
							face.a.normal,
							face.b.normal,
							face.c.normal
						);
					}
					else {
						normals = new Triangle(
							face.normal,
							face.normal,
							face.normal
						);
					}
				}
				else {
					normals = normalTransform(face);
				}
				mNorms[v + 0] = normals.a;
				mNorms[v + 1] = normals.b;
				mNorms[v + 2] = normals.c;
				Triangle uvs;
				if (uvTransform == null) {
					uvs = new Triangle(Vector3.zero, Vector3.zero, Vector3.zero);
				}
				else {
					uvs = uvTransform(face);
				}
				mUVs[v + 0] = uvs.a;
				mUVs[v + 1] = uvs.b;
				mUVs[v + 2] = uvs.c;
				foreach (Vertex vertex in face.vertices) {
					mVerts[v] = vertexTransform == null ? vertex.position : vertexTransform(vertex);
					mTris[v] = v;
					v++;
				}
			}
			mesh.vertices = mVerts;
			mesh.triangles = mTris;
			mesh.normals = mNorms;
			mesh.uv = mUVs;
			return mesh;
		}

		//build a copy of this graph with inverted face winding/normals
		public Graph BuildInvertedGraph() {
			Graph graph = new Graph();
			Dictionary<Vertex, IVertex> verts = new Dictionary<Vertex, IVertex>();
			foreach (Vertex vertex in vertexSet) {
				verts[vertex] = graph.AddVertex(vertex.position);
			}
			foreach (Edge edge in edgeSet) {
				graph.AddEdge(verts[edge.a], verts[edge.b]);
			}
			foreach (Face face in faceSet) {
				graph.AddFace(verts[face.b], verts[face.a], verts[face.c]);
			}
			return graph;
		}

		//draw this graph with unity3d's gizmo system
		public void DrawGizmos(Color vertexColor, Color faceColor, Color edgeColor, float dotRadius = 0.05f, float normalLength = .25f) {
#if UNITY_EDITOR
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
				Handles.DrawLine(edge.a.position, edge.b.position);
			}
			Handles.matrix = mat;
#endif
		}
		public void DrawGizmos(float dotRadius = 0.05f, float normalLength = .25f) {
			DrawGizmos(new Color(0.7f, 1, 1), new Color(1, 0.7f, 1), new Color(1, 1, 0.7f), dotRadius, normalLength);
		}

		//nested vertex implementation
		class Vertex : GraphComponent<IVertex>, IVertex {

			HashSet<Face> faceSet;
			public override IEnumerable<IFace> faces {
				get {
					foreach (Face face in faceSet) {
						yield return face;
					}
				}
			}

			HashSet<Edge> edgeSet;
			public override IEnumerable<IEdge> edges {
				get {
					foreach (Edge edge in edgeSet) {
						yield return edge;
					}
				}
			}

			public override IEnumerable<IVertex> neighborsByFaces {
				get {
					LinkedList<IVertex> visited = new LinkedList<IVertex>();
					foreach (IFace face in faces) {
						foreach (IVertex vertex in face.vertices) {
							if (vertex != this && !visited.Contains(vertex)) {
								visited.AddLast(vertex);
								yield return vertex;
							}
						}
					}
				}
			}

			public override IEnumerable<IVertex> neighborsByEdges {
				get {
					foreach (IEdge edge in edges) {
						foreach (IVertex vertex in edge.vertices) {
							if (vertex != this) {
								yield return vertex;
							}
						}
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

			public bool RemoveFace(Face face) {
				return faceSet.Remove(face);
			}

			public bool RemoveEdge(Edge edge) {
				return edgeSet.Remove(edge);
			}

			public override int GetHashCode() {
				return position.GetHashCode();
			}
		}

		//nested face implementation
		class Face : GraphComponent<IFace>, IFace {

			public readonly Vertex a, b, c;
			public override IEnumerable<IVertex> vertices {
				get {
					yield return a;
					yield return b;
					yield return c;
				}
			}

			public readonly Edge ab, bc, ca;
			public override IEnumerable<IEdge> edges {
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

			public float area {
				get {
					return triangle.area;
				}
			}

			public override IEnumerable<IFace> neighborsByVertices {
				get {
					LinkedList<IFace> visited = new LinkedList<IFace>();
					foreach (IVertex vertex in vertices) {
						foreach (IFace face in vertex.faces) {
							if (face != this && !visited.Contains(face)) {
								visited.AddLast(face);
								yield return face;
							}
						}
					}
				}
			}

			public override IEnumerable<IFace> neighborsByEdges {
				get {
					LinkedList<IFace> visited = new LinkedList<IFace>();
					foreach (IEdge edge in edges) {
						foreach (IFace face in edge.faces) {
							if (face != this && !visited.Contains(face)) {
								visited.AddLast(face);
								yield return face;
							}
						}
					}
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

		//nested edge implementation
		class Edge : GraphComponent<IEdge>, IEdge {

			public readonly Vertex a, b;
			public override IEnumerable<IVertex> vertices {
				get {
					yield return a;
					yield return b;
				}
			}

			HashSet<Face> faceSet;
			public override IEnumerable<IFace> faces {
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

			public float length {
				get {
					return segment.length;
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

			public override IEnumerable<IEdge> neighborsByVertices {
				get {
					foreach (IVertex vertex in vertices) {
						foreach (IEdge edge in vertex.edges) {
							if (edge != this) {
								yield return edge;
							}
						}
					}
				}
			}

			public override IEnumerable<IEdge> neighborsByFaces {
				get {
					LinkedList<IEdge> visited = new LinkedList<IEdge>();
					foreach (IFace face in faces) {
						foreach (IEdge edge in face.edges) {
							if (edge != this && !visited.Contains(edge)) {
								visited.AddLast(edge);
								yield return edge;
							}
						}
					}
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

			public bool RemoveFace(Face face) {
				return faceSet.Remove(face);
			}

			public override int GetHashCode() {
				return segment.GetHashCode();
			}

		}

		//graph componenent base
		abstract class GraphComponent<T> : IGraphComponent<T> where T : IGraphComponent<T> {

			public Graph graph { get; private set; }

			public GraphComponent(Graph graph) {
				this.graph = graph;
			}

			public void Destroy() {
				graph = null;
			}

			public virtual IEnumerable<IVertex> vertices {
				get {
					yield break;
				}
			}

			public virtual IEnumerable<IFace> faces {
				get {
					yield break;
				}
			}

			public virtual IEnumerable<IEdge> edges {
				get {
					yield break;
				}
			}

			public virtual IEnumerable<T> neighborsByVertices {
				get {
					yield break;
				}
			}

			public virtual IEnumerable<T> neighborsByFaces {
				get {
					yield break;
				}
			}

			public virtual IEnumerable<T> neighborsByEdges {
				get {
					yield break;
				}
			}
		}

	}

	public interface IVertex : IGraphComponent<IVertex> {

		Vector3 position { get; }

		Vector3 normal { get; }

	}

	public interface IFace : IGraphComponent<IFace> {

		Triangle triangle { get; }

		Vector3 center { get; }

		Vector3 normal { get; }

		float area { get; }

	}

	public interface IEdge : IGraphComponent<IEdge> {

		Segment segment { get; }

		Vector3 center { get; }

		Vector3 normal { get; }

		float length { get; }
	}

	public interface IGraphComponent<T> where T : IGraphComponent<T> {

		Graph graph { get; }

		IEnumerable<IVertex> vertices { get; }

		IEnumerable<IFace> faces { get; }

		IEnumerable<IEdge> edges { get; }

		IEnumerable<T> neighborsByVertices { get; }

		IEnumerable<T> neighborsByFaces { get; }

		IEnumerable<T> neighborsByEdges { get; }

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
				return cross.normalized;
			}
		}

		public float area {
			get {
				return cross.magnitude;
			}
		}

		Vector3 cross {
			get {
				return Vector3.Cross(b - a, c - a);
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
			return a.GetHashCode() ^ b.GetHashCode() ^ c.GetHashCode() ^ cross.GetHashCode();
		}

	}

	public struct Segment {

		public Vector3 a, b;

		public Vector3 center {
			get {
				return (a + b) / 2;
			}
		}

		public float length {
			get {
				return (a - b).magnitude;
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