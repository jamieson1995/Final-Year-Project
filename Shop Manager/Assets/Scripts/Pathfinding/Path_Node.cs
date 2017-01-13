using UnityEngine;
using System.Collections;

public class Path_Node<T> {

	//This is a template class which stores information about a node, 
	//and the edges leading OUT of the node.

	public T m_data;

	public Path_Edge<T>[] m_edges; //Nodes leading OUT from this node
}
