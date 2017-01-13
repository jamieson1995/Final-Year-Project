using UnityEngine;
using System.Collections;

public class Path_Edge<T> {

	//This is a template class which stores information about an edge.

	public float m_cost; //Cost to traverse this edge

	public Path_Node<T> m_node;
}
