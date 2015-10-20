using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace TG.MeshTools {

	public class WorkingMesh {

		HashSet<Vertex> vertexSet;
		Dictionary<Vector3, Vertex> vertexCache;

		public IEnumerable<IVertex> vertices {
			get {
				foreach(Vertex vertex in vertexSet) {
					yield return vertex;
				}
			}
		}

		public WorkingMesh() {
			vertexSet = new HashSet<Vertex>();
			vertexCache = new Dictionary<Vector3, Vertex>();
		}
		public WorkingMesh(Mesh mesh) : this() {
			AddMesh(mesh);
		}

		public void AddMesh(Mesh mesh) {
			Vector3[] mVerts = mesh.vertices;
			for (int v = 0; v < mVerts.Length; v ++) {
				AddVertex(mVerts[v]);
			}
		}

		public IVertex AddVertex(Vector3 position) {
			Vertex vertex = new Vertex(this, position);
			return vertex;
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

			public Vector3 position {
				get {
					return _pos;
				}
				set {
					if (value != _pos) {
						Vector3 old = _pos;
						_pos = value;
						Cache();
						mesh.vertexCache.Remove(old);
					}
				}
			}
			Vector3 _pos;

			public Vertex(WorkingMesh mesh, Vector3 position) : base(mesh) {
				_pos = position;
				mesh.vertexSet.Add(this);
				Cache();
			}

			//place vertex in cache. if a vetex is already cached for this position, merge with it
			void Cache() {
				if (mesh.vertexCache.ContainsKey(position)) {
					Vertex old = mesh.vertexCache[position];
					//merge faces and edges here
					mesh.vertexSet.Remove(old);
				}
				mesh.vertexCache[position] = this;
			}
		}

		class Face : MeshComponent, IFace {



			public Face(WorkingMesh mesh) : base(mesh) {

			}

			public IVertex a { get; set; }
			public IVertex b { get; set; }
			public IVertex c { get; set; }

			public Vector3 center {
				get {
					return (a.position + b.position + c.position) / 3;
				}
				set {

				}
			}
		}

		class Edge : MeshComponent, IEdge {

			public Edge(WorkingMesh mesh) : base(mesh) {

			}

		}

		abstract class MeshComponent {

			public readonly WorkingMesh mesh;

			public MeshComponent(WorkingMesh mesh) {
				this.mesh = mesh;
			}

		}

	}

	public interface IVertex {

		Vector3 position { get; set; }

	}

	public interface IFace {

		IVertex a { get; }
		IVertex b { get; }
		IVertex c { get; }

		Vector3 center { get; set; }

	}

	public interface IEdge {

	}

}