using UnityEngine;
using System.Collections.Generic;

//a graph of nodes
public class NodeGraph {

	//the nodes in the graph
	List<Node> _nodes;
	//(public accessor)
	public IEnumerable<Node> nodes {
		get {
			foreach (Node node in _nodes) {
				yield return node;
			}
		}
	}

	//constructor
	public NodeGraph() {
		_nodes = new List<Node>();
	}

	//add a node to the graph and return the node
	public Node AddNode(Vector3 position) {
		Node node = new Node(this, position);
		_nodes.Add(node);
		return node;
	}
	//zero arg overload
	public Node AddNode() {
		return AddNode(Vector3.zero);
	}

	//remove the node from the graph
	//returns true if successful, fals otherwise
	public bool RemoveNode(Node node) {
		return node.graph == this && _nodes.Remove(node);
	}

	//draw a representation of this graph using Unity's Gizmo system
	public void DrawGizmos(float nodeRadius, Color nodeColor, Color edgeColor) {
		foreach (Node node in _nodes) {
			Gizmos.color = nodeColor;
			Gizmos.DrawWireSphere(node.position, nodeRadius);
			Gizmos.color = edgeColor;
			foreach (Edge edge in node.edges) {
				Gizmos.DrawLine(node.position, edge.center);
			}
		}
	}
	//overload
	public void DrawGizmos(float nodeRadius = .1f) {
		DrawGizmos(nodeRadius, new Color(.7f, 1, 1), new Color(1, .7f, 1));
	}
}

//a node in the graph
public class Node {

	//the graph this node belongs to
	public readonly NodeGraph graph;

	//the posisiton of the node in space
	public Vector3 position;

	//the neighbors of this node
	List<Node> _neighbors;
	//(public accessor)
	public IEnumerable<Node> neighbors {
		get {
			foreach (Node neighbor in _neighbors) {
				yield return neighbor;
			}
		}
	}

	//the edges of which this node is an vertex
	public IEnumerable<Edge> edges {
		get {
			foreach (Node neighbor in _neighbors) {
				yield return GetEdge(neighbor);
			}
		}
	}

	//constructor: DO NOT CALL DIRECTLY use NodeGraph.AddNode() instead
	public Node(NodeGraph graph, Vector3 position) {
		this.position = position;
		_neighbors = new List<Node>();
	}

	//make [other] a neighbor of this node
	//returns true if a new connection was made, false otherwise
	public bool MakeNeighbor(Node other) {
		if (graph == other.graph && other != this && !_neighbors.Contains(other)) {
			_neighbors.Add(other);
			other.MakeNeighbor(this);
			return true;
		}
		return false;
	}

	//make [other] not a neighbor of this node
	//returns true if a connection was destroyed, false otherwise
	public bool RemoveNeighbor(Node other) {
		if (other != this && _neighbors.Contains(other)) {
			_neighbors.Remove(other);
			other.RemoveNeighbor(this);
			return true;
		}
		return false;
	}

	//returns true if [other] is a neighbor of this node, false otherwise
	public bool IsNeighborOf(Node other) {
		return _neighbors.Contains(other);
	}

	//if [other] is a neighbor of this node, return the edge connecting them, null otherwise
	public Edge GetEdge(Node other) {
		if (IsNeighborOf(other)) {
			return new Edge(this, other);
		}
		else {
			return new Edge(null, null);
		}
	}

}

//a representation of the connection between two nodes
public struct Edge {

	//the nodes at either end of this edge
	public readonly Node a, b;

	//the validity of this edge
	public bool isValid {
		get {
			return (a != null && b != null);
		}
	}

	//the square length of this edge
	public float sqrLength {
		get {
			return (a.position - b.position).sqrMagnitude;
		}
	}

	//the length of this edge
	public float length {
		get {
			return Mathf.Sqrt(sqrLength);
		}
	}

	//the center of this edge
	public Vector3 center {
		get {
			return (a.position + b.position) / 2;
		}
	}

	//constructor
	public Edge(Node a, Node b) {
		this.a = a;
		this.b = b;
	}

}
