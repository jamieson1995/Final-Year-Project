using UnityEngine;
using System.Collections.Generic;
using System;

public class Path_TileGraph{

	//This class constructs a simple path-finding compatible graph
	//of our world. Each tile is a node. Each WALKABLE neighbour 
	//from a tile is linked via an edge connection.

	/// Dictionary of the world's nodes.
	public Dictionary<Tile, Path_Node<Tile>> m_nodes { get; protected set; }

	/// Creates a new tile graph, with specified flags and tiles.
	public Path_TileGraph ( World _world, bool _ignoreCharacters = true, Tile _root = null )
	{
		//Loop through all tile of the world
		//For each tile, create a node.

		m_nodes = new Dictionary<Tile, Path_Node<Tile>> ();

		for ( int x = 0; x < _world.m_width; x++ )
		{
			for ( int y = 0; y < _world.m_height; y++ )
			{
				Tile t = _world.GetTileAt ( x, y );
				if ( t.m_movementCost > 0 )
				{
					if ( t == _root || _ignoreCharacters || ( _ignoreCharacters == false && t.m_character == null ) )
					{
						Path_Node<Tile> n = new Path_Node<Tile> ();
						n.m_data = t;
						m_nodes.Add ( t, n );
					}
				}
			}
		}

		//Now loop through all nodes again
		//and create edges for neighbours

		foreach ( Tile t in m_nodes.Keys )
		{
			Path_Node<Tile> n = m_nodes [ t ];

			List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>> ();

			//Get a list of neighbours for the tile
			Tile[] neighbours = t.GetNeighbours ( true );

			//If neighbour is walkable, create an edge to the relevant node
			for ( int i = 0; i < neighbours.Length; i++ )
			{
				if ( neighbours [ i ] != null && neighbours [ i ].m_movementCost > 0 )
				{
					if ( _ignoreCharacters || ( _ignoreCharacters == false && neighbours [ i ].m_character == null ) )
					{
						//This neighbour is walkable, so create an edge.

						//But first, mke sure we are not clipping a diagonal, or trying to squeeze unappropiately
						if ( IsClippingCorner ( t, neighbours [ i ] ) )
						{
							continue; //Skip to next neighbour without building an edge
						}

						Path_Edge<Tile> e = new Path_Edge<Tile> ();
						e.m_cost = neighbours [ i ].m_movementCost;
						e.m_node = m_nodes [ neighbours [ i ] ];
						edges.Add ( e );
					}
					}
			}

			n.m_edges = edges.ToArray();
		}
	}

	bool IsClippingCorner ( Tile _currTile, Tile _neighbour )
	{
		//If the movement from _currTile to _neighbour, then make sure we aren't clipping

		if ( Mathf.Abs ( _currTile.X - _neighbour.X ) + Mathf.Abs ( _currTile.Y - _neighbour.Y ) == 2 )
		{
			//We are diagonal
			int dX = _currTile.X - _neighbour.X;
			int dY = _currTile.Y - _neighbour.Y;

			if ( _currTile.m_world.GetTileAt ( _currTile.X + dX, _currTile.Y ) == null || _currTile.m_world.GetTileAt ( _currTile.X, _currTile.Y + dY ) == null )
			{
				return false;
			}

			if( _currTile.m_world.GetTileAt( _currTile.X - dX, _currTile.Y).m_movementCost == 0 || _currTile.m_world.GetTileAt( _currTile.X - dX, _currTile.Y).m_movementCost > 1){
				//East or West is unwalkable, or has a furniture, therefore this would be a clipped movement.
				return true;
			}
			if( _currTile.m_world.GetTileAt( _currTile.X, _currTile.Y - dY ).m_movementCost == 0 || _currTile.m_world.GetTileAt( _currTile.X, _currTile.Y - dY ).m_movementCost > 1){
				//North or South is unwalkable,or has a furniture, therefore this would be a clipped movement.
				return true;
			}
		}

		return false;
	}

}
