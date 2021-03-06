﻿using UnityEngine;
using System.Collections.Generic;
using Priority_Queue;
using System.Linq;

public class Path_AStar {

	/// Reference to the current and inital paths.
	Queue<Tile> m_path;
	Queue<Tile> m_initalPath;

	/// Finds a path from the specifed _tileStart, to thespecifed _tileEnd. Flag to determine if the pathfinder ignores characters as obstacles.
	public Path_AStar ( World _world, Tile _tileStart, Tile _tileEnd, bool _ignoreCharacters = true )
	{

		if ( _tileStart == _tileEnd || _tileEnd == null )
		{
			//We don't need to go anywhere.
			m_path = new Queue<Tile> ();
			m_path.Enqueue ( _tileStart );
			m_initalPath = new Queue<Tile> ( m_path );
			return;
		}

		_world.m_tileGraph = new Path_TileGraph ( _world, _ignoreCharacters, _tileStart);


		Dictionary<Tile, Path_Node<Tile>> nodes = _world.m_tileGraph.m_nodes;

		if ( nodes.ContainsKey ( _tileStart ) == false )
		{
			Debug.LogWarning ( "Path_AStar -- The starting tile isn't in the list of nodes." );
			return;
		}
		if ( nodes.ContainsKey ( _tileEnd ) == false )
		{
			Debug.LogWarning ( "Path_AStar -- The ending tile isn't in the list of nodes." );
			return;
		}

		Path_Node<Tile> start = nodes [ _tileStart ];
		Path_Node<Tile> goal = nodes [ _tileEnd ];

		List<Path_Node<Tile>> ClosedSet = new List<Path_Node<Tile>> ();

		SimplePriorityQueue<Path_Node<Tile>> OpenSet = new SimplePriorityQueue<Path_Node<Tile>> ();

		OpenSet.Enqueue ( start, 0 ); 

		Dictionary<Path_Node<Tile>, Path_Node<Tile>> m_cameFrom = new Dictionary<Path_Node<Tile>, Path_Node<Tile>> ();

		Dictionary<Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float> ();
		foreach ( Path_Node<Tile> n in nodes.Values )
		{
			g_score [ n ] = Mathf.Infinity;
		}
		g_score [ start ] = 0;

		Dictionary<Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float> ();
		foreach ( Path_Node<Tile> n in nodes.Values )
		{
			f_score [ n ] = Mathf.Infinity;
		}
		f_score [ start ] = heuristicCostEstimate ( start, goal );

		while ( OpenSet.Count > 0 )
		{
			Path_Node<Tile> current = OpenSet.Dequeue ();

			if ( current == goal )
			{
				ReconstructPath ( m_cameFrom, current );
				return;
			}

			ClosedSet.Add ( current );

			foreach ( Path_Edge<Tile> edgeNeighbour in current.m_edges )
			{
				Path_Node<Tile> neighbour = edgeNeighbour.m_node;
				if ( ClosedSet.Contains ( neighbour ) )
				{
					continue;
				}

				float movementCostToNeighbour = neighbour.m_data.m_movementCost * DistBetween ( current, neighbour );

				float tentative_g_Score = g_score [ current ] + movementCostToNeighbour;

				if ( OpenSet.Contains ( neighbour ) && tentative_g_Score >= g_score [ neighbour ] )
				{
					continue;
				}

				m_cameFrom [ neighbour ] = current;
				g_score [ neighbour ] = tentative_g_Score;
				f_score [ neighbour ] = g_score [ neighbour ] + heuristicCostEstimate ( neighbour, goal );

				if ( OpenSet.Contains ( neighbour ) == false )
				{
					f_score [ neighbour ] = g_score [ neighbour ];
					OpenSet.Enqueue ( neighbour, f_score [ neighbour ] );
				}
				else
				{
					OpenSet.UpdatePriority ( neighbour, f_score [ neighbour ] );
				}
			}
		}


	}

	/// Recontructs the found path and puts it into the m_initialPath variable.
	void ReconstructPath ( Dictionary<Path_Node<Tile>, Path_Node<Tile>> _cameFrom, Path_Node<Tile> current )
	{
		Queue<Tile> total_path = new Queue<Tile> ();
		total_path.Enqueue(current.m_data);

		while ( _cameFrom.ContainsKey ( current) )
		{
			current = _cameFrom [ current ];
			total_path.Enqueue ( current.m_data );

			m_path = new Queue<Tile> ( total_path.Reverse () );
			m_path.Dequeue();
			m_initalPath = new Queue<Tile> ( m_path );
		}
	}

	/// Estimate of the length from and to the specifed tile nodes.
	float heuristicCostEstimate( Path_Node<Tile> _start,  Path_Node<Tile> _goal ){
		return Mathf.Sqrt(
			Mathf.Pow(_start.m_data.X - _goal.m_data.X, 2) + 
			Mathf.Pow(_start.m_data.Y - _goal.m_data.Y, 2)
		);
	}

	/// The length between the specifed tile nodes.
	float DistBetween ( Path_Node<Tile> _start, Path_Node<Tile> _goal )
	{

		if ( Mathf.Abs ( _start.m_data.X - _goal.m_data.X ) + Mathf.Abs ( _start.m_data.Y - _goal.m_data.Y ) == 1 )
		{ 
			return 1f;
		}

		if ( Mathf.Abs ( _start.m_data.X - _goal.m_data.X ) == 1 && Mathf.Abs ( _start.m_data.Y - _goal.m_data.Y ) == 1 )
		{ 
			return 1.41421356237f;
		}

		return Mathf.Sqrt(
			Mathf.Pow(_start.m_data.X - _goal.m_data.X, 2) + 
			Mathf.Pow(_start.m_data.Y - _goal.m_data.Y, 2)
		);
	}

	/// Returns the next tile in the queue.
	public Tile Dequeue ()
	{
		return m_path.Dequeue ();
	}

	/// Returns the current path as an array of tiles.
	public Tile[] CurrPathToArray ()
	{
		if ( m_path == null )
		{
			return null;
		}

		return m_path.ToArray();
	}

	/// Returns the initial path as an array of tiles.
	public Tile[] InitialPathToArray ()
	{
		if ( m_initalPath == null )
		{
			return null;
		}

		return m_initalPath.ToArray();
	}

	/// Returns the number of entries in the current path.
	public int Length ()
	{
		if ( m_path == null )
			return 0;

		return m_path.Count;
	}
}
