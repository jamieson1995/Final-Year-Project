//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public enum ENTERABILITY { Yes, Never, Soon } //Every tile has a ENTERABILITY value based upon the furniture in the tile.
											  //Most things are Yes or Never, but some things, such are doors, are Soon.
											  //If the variable is set to soon, it means that a character cannot currently
											  //walk into the tile, but will be able to soon, for example because the door is opening.

public class Tile {

	/// Reference to WorldController.instance.m_world
	public World m_world { get; protected set; }

	/// This tile's X coordinate.
	public int X { get; protected set; }

	/// This tile's Y coordinate.
	public int Y { get; protected set; }

	/// Reference to the furniture in this tile. Null means no furniture is in this tile.
	public Furniture m_furniture { get; protected set; }

	/// Reference to the character in this tile. Null means no characters are in this tile.
	public Character m_character { get; protected set; }

	/// Flag to determine if this tile is an outside tile.
	public bool m_outside;

	/// Flag to determine if this tile is avaiable to customer to queue with.
	public bool m_queue;

	/// How many tiles deep is this queue tile in this queue.
	public int m_queueNum;

	/// The movement cost of walking on this tile.
	public float m_movementCost
	{
		get
		{
			if ( m_furniture == null )
			{
				// If there is no furniture, the movement must be 1. 
				return 1;
			}

			// The movement cost is above 1, which means there must be furniture in this tile, so return the furniture's movement cost.
			return m_furniture.m_movementCost;
		}
	}

	/// Creates a new Tile with the specified reference to world, and the specified cooordinates.
	public Tile(World _world, int _x, int _y)
	{
		this.m_world = _world;
		this.X = _x;
		this.Y = _y;
	}

	/// Changes this tile's character flag to the specified character. Can be null.
	public void ChangeCharacterInTile ( Character _char )
	{
		m_character = _char;
	}

	/// Returns the outcome of the attempt. Attempts to place the specified furniture in this tile, with the specified rotation.
	public bool PlaceFurniture ( Furniture _furn, int _direction = 1 )
	{

		if ( _furn == null )
		{
			Debug.Log ( "Trying to place nothing" );
		}

		if ( m_world.PositionCheck(this, _furn, _direction) == false )
		{
			return false;
		}

		for ( int x_off = X; x_off < (X + _furn.Width); x_off++ )
		{
			for (int y_off = Y; y_off < (Y + _furn.Height); y_off++) {

				Tile t = m_world.GetTileAt(x_off, y_off);

				t.m_furniture = _furn;

			}
		}


		return true;
	}

	/// Sets this tile's furniture reference to null.
	public void RemoveFurniture ()
	{
		m_furniture = null;
	}

	//Returns true if the the specified tile is adjacent to this one.
    public bool IsNeighbour ( Tile _tile, bool _sameTile = false, bool _diagOK = false )
	{
		if ( this.X == _tile.X && ( this.Y == _tile.Y + 1 || this.Y == _tile.Y - 1 ) )
			return true;

		if ( this.Y == _tile.Y && ( this.X == _tile.X + 1 || this.X == _tile.X - 1 ) )
			return true;

		if ( _diagOK )
		{
			if ( this.X == _tile.X + 1 && ( this.Y == _tile.Y + 1 || this.Y == _tile.Y - 1 ) )
				return true;

			if ( this.X == _tile.X - 1 && ( this.Y == _tile.Y + 1 || this.Y == _tile.Y - 1 ) )
				return true;

		}

		if ( _sameTile )
		{
			if ( this == _tile )
			{
				return true;
			}
		}

    	return false;
    }

	/// Returns an array. The array is filled with this tile's neighbouring tiles.
	public Tile[] GetNeighbours ( bool _diagOK = false )
	{

		Tile[] neighbours; 
		
		if ( _diagOK == false )
		{
			neighbours = new Tile[4]; //Tile order: N E S W
		}
		else
		{
			neighbours = new Tile[8]; //Tile order: N E S W NE SE SW NW
		}

		Tile n;

		n = m_world.GetTileAt ( X, Y + 1 );
		neighbours [ 0 ] = n;

		n = m_world.GetTileAt ( X + 1, Y );
		neighbours [ 1 ] = n;

		n = m_world.GetTileAt ( X, Y - 1 );
		neighbours [ 2 ] = n;

		n = m_world.GetTileAt ( X - 1, Y );
		neighbours [ 3 ] = n;

		if ( _diagOK )
		{
			//North-East
			n = m_world.GetTileAt ( X + 1, Y + 1 );
			neighbours [ 4 ] = n;

			//South-East
			n = m_world.GetTileAt ( X + 1, Y - 1 );
			neighbours [ 5 ] = n;

			//South-West
			n = m_world.GetTileAt ( X - 1, Y - 1 );
			neighbours [ 6 ] = n;

			//North-West
			n = m_world.GetTileAt ( X - 1, Y + 1 );
			neighbours [ 7 ] = n;
		}

		return neighbours;

    }

	/// Returns an array. The array is filled with this tile's neighbouring tiles' furniture. If an entry is null, that tile was empty of furniture.
	public Furniture[] GetNeighboursFurniture ( bool _diagOk = false )
	{
		Tile[] neighbours = GetNeighbours ( _diagOk );
		Furniture[] neighboursFurn;

		neighboursFurn = new Furniture[neighbours.Length];

		for ( int i = 0; i < neighbours.Length; i++ )
		{
			neighboursFurn[i] = neighbours[i].m_furniture;
		}

		return neighboursFurn;
	}

	/// Returns the enterability of this tile.
	public ENTERABILITY IsEnterable ()
	{
		if ( m_movementCost == 0 )
		{
			return ENTERABILITY.Never;
		}

		if ( m_furniture != null && m_furniture.m_isEnterable != null )
		{
			// If the movement cost is above 0, and the tile has furniture, get the furniture's enterability and return it.
			return m_furniture.m_isEnterable(m_furniture);
		}

		if (m_character != null)
		{
			return ENTERABILITY.Soon;
		}

		return ENTERABILITY.Yes;
	}

	/// Returns the string version of this tile's location.
	public string GetStringLocation()
	{
		return "( " + X + ", " + Y + " )";
	}

}
