//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindFreeQueueTile {

	/// Reference to the tile found.
	public Tile m_tileFound;

	/// Finds a free queue tile. _first flag is to determine if we only want the front of the queue.
	public FindFreeQueueTile ( World _world, Tile _currTile, bool _first = false)
	{
		_world.m_tileGraph = new Path_TileGraph ( _world, true );
		Dictionary<Tile, Path_Node<Tile>> nodes = _world.m_tileGraph.m_nodes;

		if ( nodes.ContainsKey ( _currTile ) == false )
		{
			Debug.LogError ( "Path_AStar -- The starting tile isn't in the list of nodes." );
			return;
		}

		Queue<Path_Node<Tile>> OpenSet = new Queue<Path_Node<Tile>> ();
		List<Path_Node<Tile>> ClosedSet = new List<Path_Node<Tile>> ();

		Path_Node<Tile> start = nodes [ _currTile ];

		OpenSet.Enqueue ( start );

		while ( OpenSet.Count > 0 )
		{
			Path_Node<Tile> current = OpenSet.Dequeue ();
			foreach ( Path_Edge<Tile> edgeNeighbour in current.m_edges )
			{
				Path_Node<Tile> neighbour = edgeNeighbour.m_node;

				bool m_currTileInvalid = false;

				if ( neighbour.m_data == null )
				{
					continue;
				}
			
				if ( neighbour.m_data.m_queue )
				{	
					//This tile is a queue tile.
					if ( _first )
					{
						if ( neighbour.m_data.m_queueNum != 1 )
						{
							m_currTileInvalid = true;
						}
					}
					else
					{
						if ( neighbour.m_data.m_character != null )
						{
							//The tile is occupied.
							m_currTileInvalid = true;
						}
					}
				}
				else
				{
					m_currTileInvalid = true;
				}

				if ( m_currTileInvalid == false )
				{
					//If we get here then the current tile is valid.
					m_tileFound = neighbour.m_data;
					return;
				}

				if ( ClosedSet.Contains ( neighbour ) == false )
				{
					ClosedSet.Add ( neighbour );
					OpenSet.Enqueue ( neighbour );
				}
			}

		}
		//If we get here, then all the queue tiles have been occupied.
		//We we need to just go the the nearest queue tile, and stand next to it.

		OpenSet.Enqueue ( start );
		ClosedSet = new List<Path_Node<Tile>> ();

		while ( OpenSet.Count > 0 )
		{
			Path_Node<Tile> current = OpenSet.Dequeue ();
			foreach ( Path_Edge<Tile> edgeNeighbour in current.m_edges )
			{
				Path_Node<Tile> neighbour = edgeNeighbour.m_node;

				bool m_currTileInvalid = false;

				if ( neighbour.m_data == null )
				{
					continue;
				}
			
				if (neighbour.m_data.m_queue == false)
				{
					m_currTileInvalid = true;
				}

				if ( m_currTileInvalid == false )
				{
					//If we get here then the current tile is valid.
					m_tileFound = neighbour.m_data;
					return;
				}

				if ( ClosedSet.Contains ( neighbour ) == false )
				{
					ClosedSet.Add ( neighbour );
					OpenSet.Enqueue ( neighbour );
				}
			}

		}

	}
}
