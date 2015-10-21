using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace TG.MeshTools {

	public class MeshGraph {

		HashSet<Vertex> vertexSet;
		Dictionary<Vector3, Vertex> vertexCache;
		public IEnumerable<IVertex> vertices {
			get {
				foreach (Vertex vertex in vertexSet) {
					yield return vertex;
				}
			}
		}

		HashSet<Face> faceSet;
		Dictionary<Triangle, Face> faceCache;

		public MeshGraph() {
			vertexSet = new HashSet<Vertex>();
			vertexCache = new Dictionary<Vector3, Vertex>();
			faceSet = new HashSet<Face>();
			faceCache = new Dictionary<Triangle, Face>();
		}

		public MeshGraph(Mesh mesh) : this() {
			AddMesh(mesh);
		}

		public void AddMesh(Mesh mesh) {
			Vector3[] mVerts = mesh.vertices;
			for (int v = 0; v < mVerts.Length; v++) {
				AddVertex(mVerts[v]);
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
			}
			return vertex;
		}

		public IFace AddFace(IVertex vertA, IVertex vertB, IVertex vertC) {
			if (vertA.mesh == this && vertB.mesh == this && vertC.mesh == this) {
				Vertex a = (Vertex) vertA;
				Vertex b = (Vertex) vertB;
				Vertex c = (Vertex) vertC;
				Triangle tri = new Triangle(a, b, c);
				Face face;
				if (faceCache.ContainsKey(tri)) {
					face = faceCache[tri];
				}
				else {
					face = new Face(this, a, b, c);
					faceSet.Add(face);
				}
				return face;
			}
			throw new ArgumentException("Cannot create face: at least one vertex not belong to this mesh.");
		}

		//draw this mesh with unity3d's gizmo system
		public void DrawGizmos(Color vertexColor, Color faceColor, Color edgeColor, float vertexRadius = 0.01f, float faceRadius = 0.01f) {
			Gizmos.color = vertexColor;
			foreach (Vertex vertex in vertexSet) {
				Gizmos.DrawWireSphere(vertex.position, vertexRadius);
			}

		}
		public void DrawGizmos(float vertexRadius = 0.01f, float faceRadius = 0.01f) {
			DrawGizmos(new Color(0.7f, 1, 1), new Color(1, 0.7f, 1), new Color(1, 1, 0.7f), vertexRadius, faceRadius);
		}

		class Vertex : MeshComponent, IVertex {

			public Vector3 position { get; private set; }

			public Vertex(MeshGraph mesh, Vector3 position) : base(mesh) {
				this.position = position;
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

			public Triangle triangle {
				get {
					return new Triangle(a.position, b.position, c.position);
				}
			}

			public Vector3 center {
				get {
					return (a.position + b.position + c.position) / 3;
				}
			}

			public Vector3 normal {
				get {
					return Vector3.Cross(b.position - a.position, c.position - a.position).normalized;
				}
			}

			public Face(MeshGraph mesh, Vertex a, Vertex b, Vertex c) : base(mesh) {
				this.a = a;
				this.b = b;
				this.c = c;
			}

		}

		class Edge : MeshComponent, IEdge {

			public Edge(MeshGraph mesh) : base(mesh) {

			}

		}

		abstract class MeshComponent : IMeshComponent {

			public MeshGraph mesh { get; private set; }

			public MeshComponent(MeshGraph mesh) {
				this.mesh = mesh;
			}

		}

	}

	public interface IVertex : IMeshComponent {

		Vector3 position { get; }

	}

	public interface IFace : IMeshComponent {

		IEnumerable<IVertex> vertices { get; }

		Triangle triangle { get; }

		Vector3 center { get; }

		Vector3 normal { get; }

	}

	public interface IEdge : IMeshComponent {

	}

	public interface IMeshComponent {

		MeshGraph mesh { get; }

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
			if (a == b) return true;
			b = b.Rotate();
			if (a == b) return true;
			b = b.Rotate();
			if (a == b) return true;
			return false;
		}

		public static bool operator !=(Triangle a, Triangle b) {
			return !(a == b);
		}

		public override int GetHashCode() {
			return a.GetHashCode() ^ b.GetHashCode() ^ c.GetHashCode() ^ normal.GetHashCode();
		}

	}

}