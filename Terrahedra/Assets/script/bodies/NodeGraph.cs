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

	//remove the node from the graph
	//returns true if successful, fals otherwise
	public bool RemoveNode(Node node) {
		return node.graph == this && _nodes.Remove(node);
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

	//constructor: DO NOT CALL DIRECTLY
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
			return null;
		}
	}

}

//a represantation of the connection between two nodes
public class Edge {

	//the nodes at either end of this edge
	public readonly Node a, b;

	//the square length of this edge
	public float sqrLength {
		get {
			return (a.position - b.position).sqrMagnitude;
		}
	}

	//the lenght of this edge
	public float length {
		get {
			return Mathf.Sqrt(sqrLength);
		}
	}

	//constructor
	public Edge(Node a, Node b) {
		this.a = a;
		this.b = b;
	}

}
