using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace TG.MeshTools {

	//util class to do things with meshes
	public class WorkingMesh {

		//the vertices of the working mesh
		List<Vertex> vertices;
		//the faces of the working mesh
		List<Face> faces;

		//constructor
		public WorkingMesh() {
			vertices = new List<Vertex>();
			faces = new List<Face>();
		}
		
		//convenience overload
		public WorkingMesh(params Mesh[] meshes) : this() {
			foreach (Mesh mesh in meshes) {
				AddMesh(mesh);
			}
		}

		//add the geometry of [mesh] to the working mesh
		public void AddMesh(Mesh mesh) {
			Dictionary<Vector3, Vertex> verts = new Dictionary<Vector3, Vertex>();
			Vector3[] mVerts = mesh.vertices;
			for (int v = 0; v < mVerts.Length; v ++) {
				if (!verts.ContainsKey(mVerts[v])) {
					Vertex vertex = new Vertex(mVerts[v]);
					verts[mVerts[v]] = vertex;
					vertices.Add(vertex);
                }
			}
			int f = 0;
			int[] mFaces = mesh.triangles;
			while (f < mFaces.Length) {
				Vertex a = verts[mVerts[mFaces[f++]]];
				Vertex b = verts[mVerts[mFaces[f++]]];
				Vertex c = verts[mVerts[mFaces[f++]]];
				Face face = new Face(a, b, c);
				faces.Add(face);
			}
		}

	}

	//vertex of a mesh
	public class Vertex : IFaceHaver, IEdgeHaver {

		//position of the vertex
		public Vector3 position;

		//faces
		HashSet<Face> faceSet;
		public IEnumerable<Face> faces {
			get {
				foreach (Face face in faceSet) {
					yield return face;
				}
			}
		}

		//edges
		HashSet<Edge> edgeSet;
		public IEnumerable<Edge> edges {
			get {
				foreach (Edge edge in edgeSet) {
					yield return edge;
				}
			}
		}

		//constructor
		public Vertex(Vector3 position) {
			this.position = position;
			position = this;
			faceSet = new HashSet<Face>();
			edgeSet = new HashSet<Edge>();
		}

		//vertices are also vectors
		public static implicit operator Vector3(Vertex v) {
			return v.position;
		}

		//adds a face to the vertex
		public bool AddFace(Face face) {
			if (face.ContainsVertex(this)) {
				return faceSet.Add(face);
			}
			return false;
		}

		//adds an edge to the vertex
		public bool AddEdge(Edge edge) {
			if (edge.ContainsVertex(this)) {
				return edgeSet.Add(edge);
			}
			return false;
		}
	}

	//face of a mesh
	public class Face : IVertexHaver, IEdgeHaver {

		//vertices
		public readonly Vertex a, b, c;
		public IEnumerable<Vertex> vertices {
			get {
				yield return a;
				yield return b;
				yield return c;
			}
		}

		//edges
		public readonly Edge ab, bc, ca;
		public IEnumerable<Edge> edges {
			get {
				yield return ab;
				yield return bc;
				yield return ca;
			}
		}

		//constructor
		public Face(Vertex a, Vertex b, Vertex c) {
			this.a = a;
			this.b = b;
			this.c = c;
			ab = new Edge(a, b);
			bc = new Edge(b, c);
			ca = new Edge(c, a);
			foreach (Vertex vertex in vertices) {
				vertex.AddFace(this);
			}
			foreach (Edge edge in edges) {
				edge.AddFace(this);
			}
		}

		//you cant add new vertices to a face
		public bool AddVertex(Vertex vertex) { return false; }
		//you cant add new edges to a face
		public bool AddEdge(Edge edge) { return false; }
	}

	//edge of a mesh
	public class Edge : IVertexHaver, IFaceHaver {

		//vertices
		public readonly Vertex a, b;
		public IEnumerable<Vertex> vertices {
			get {
				yield return a;
				yield return b;
			}
		}

		//faces
		HashSet<Face> faceSet;
		public IEnumerable<Face> faces {
			get {
				foreach(Face face in faceSet) {
					yield return face;
				}
			}
		}

		//constructor
		public Edge(Vertex a, Vertex b) {
			this.a = a;
			this.b = b;
			faceSet = new HashSet<Face>();
			foreach (Vertex vertex in vertices) {
				vertex.AddEdge(this);
			}
		}

		//you cant add new vertices to an edge
		public bool AddVertex(Vertex vertex) { return false; }

		//add a face to the edge
		public bool AddFace(Face face) {
			if (face.ContainsEdge(this)) {
				return faceSet.Add(face);
			}
			return false;
		}
	}

	//something containing vertices
	public interface IVertexHaver {

		IEnumerable<Vertex> vertices {
			get;
		}

		bool AddVertex(Vertex vertex);
	}

	//something containing faces
	public interface IFaceHaver {

		IEnumerable<Face> faces {
			get;
		}

		bool AddFace(Face face);

	}

	//something containing edges
	public interface IEdgeHaver {

		IEnumerable<Edge> edges {
			get;
		}

		bool AddEdge(Edge edge);
	}

	//haver extensions
	public static class HaverE {

		//return true if the vertex is part of this structure, false otherwise
		public static bool ContainsVertex(this IVertexHaver haver, Vertex vertex) {
			foreach (Vertex v in haver.vertices) {
				if (v == vertex) {
					return true;
				}
			}
			return false;
		}

		//return true if the face is part of this structure, false otherwise
		public static bool ContainsFace(this IFaceHaver haver, Face face) {
			foreach (Face f in haver.faces) {
				if (f == face) {
					return true;
				}
			}
			return false;
		}

		//return true if the edge is part of this structure, false otherwise
		public static bool ContainsEdge(this IEdgeHaver haver, Edge edge) {
			foreach (Edge e in haver.edges) {
				if (e == edge) {
					return true;
				}
			}
			return false;
		}
	}

}