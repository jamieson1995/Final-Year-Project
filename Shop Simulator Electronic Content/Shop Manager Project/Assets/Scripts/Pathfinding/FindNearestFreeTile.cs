﻿//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Find the nearest free tile. A free tile is one without furniture and the character does not need to go through it to reach thier destination.
/// </summary>
public class FindNearestFreeTile {

	/// Reference to the tile found.
	public Tile m_tileFound { get; protected set; }

	/// Finds the nearest free tile. Flag to determine if the root node should always be added to the list of nodes.
	public FindNearestFreeTile ( World _world, Tile _root, Tile[] _tilesNeededForMovement, bool _alwaysAddRoot = false )
	{
		if ( _tilesNeededForMovement == null )
		{
			_tilesNeededForMovement = new Tile[1];
			_tilesNeededForMovement[0] = _root;
		}


		if ( _alwaysAddRoot )
		{
			_world.m_tileGraph = new Path_TileGraph ( _world, false, _root );
		}
		else
		{
			_world.m_tileGraph = new Path_TileGraph ( _world, false );
		}

		Dictionary<Tile, Path_Node<Tile>> nodes = _world.m_tileGraph.m_nodes;

		if ( nodes.ContainsKey ( _root ) == false )
		{
			Debug.LogError ( "Path_AStar -- The starting tile isn't in the list of nodes." );
			return;
		}

		Queue<Path_Node<Tile>> OpenSet = new Queue<Path_Node<Tile>> ();
		List<Path_Node<Tile>> ClosedSet = new List<Path_Node<Tile>> ();

		Path_Node<Tile> start = nodes [ _root ];

		OpenSet.Enqueue ( start );

		while ( OpenSet.Count > 0 )
		{
			Path_Node<Tile> current = OpenSet.Dequeue ();
			foreach ( Path_Edge<Tile> edgeNeighbour in current.m_edges )
			{
				Path_Node<Tile> neighbour = edgeNeighbour.m_node;

				if ( neighbour.m_data == null )
				{
					continue;
				}

				bool m_currTileInvalid = false;

				foreach ( Tile t in _tilesNeededForMovement )
				{
					if ( neighbour.m_data == t )
					{
						m_currTileInvalid = true;
					}
				}

				if ( neighbour.m_data.m_furniture != null )
				{
					m_currTileInvalid = true;
				}

				if ( neighbour.m_data.m_character != null )
				{
					m_currTileInvalid = true;
				}

				if ( m_currTileInvalid == false )
				{
					m_tileFound = neighbour.m_data;
					return;
				}



				if ( ClosedSet.Contains(neighbour) == false)
				{
					ClosedSet.Add(neighbour);
					OpenSet.Enqueue(neighbour);
				}
			}

		}
	}

	/// Returns the reversed version of the specifed array.
	Array ReverseArrayOrder(Array _array)
	{
		Array.Reverse(_array);
		return _array;
	}
}
