﻿using UnityEngine;
using System.Collections;

public enum ENTERABILITY { Yes, Never, Soon } //Every tile has a ENTERABILITY value based upon the furniture in the tile.
											  //Most things are Yes or Never, but some things, such are doors, are Soon.
											  //If the variable is set to soon, it means that a character cannot currently
											  //walk into the tile, but will be able to soon, for example because the door is opening.

public class Tile {

	public World m_world { get; protected set; }

	public int X { get; protected set; }
	public int Y { get; protected set; }

	public Furniture m_furniture { get; protected set; }


	public Tile(World _world, int _x, int _y)
	{
		this.m_world = _world;
		this.X = _x;
		this.Y = _y;
	}


	//Tries to place a piece of furniture on this tile. If successful it will return true, and the furniture gets placed. If it cannot be placed, this returns false.
	public bool PlaceFurniture ( Furniture _furn )
	{
		if ( _furn == null )
		{
			Debug.Log ( "Trying to place nothing" );
		}

		if ( _furn.IsValidPosition ( this ) == false )
		{
			Debug.LogError ( "Trying to assign a furniture to a tile that isn't valid" );
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

	//Tells us if two tile are adjacent
    public bool IsNeighbour ( Tile _tile, bool _diagOK )
	{
		if ( this.X == _tile.X && ( this.Y == _tile.Y + 1 || this.Y == _tile.Y - 1 ) )
			return true;

		if ( this.Y == _tile.Y && ( this.X == _tile.X + 1 || this.X == _tile.X - 1 ) )
			return true;

		if ( _diagOK )
		{
			if(this.X == _tile.X+1 && (this.Y == _tile.Y+1 || this.Y == _tile.Y-1))
    		return true;

			if(this.X == _tile.X-1 && (this.Y == _tile.Y+1 || this.Y == _tile.Y-1))
    		return true;

		}

    	return false;
    }

    //Returns an array of tiles, containing the neighbouring tiles.
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

	//Returns an array of furniture, containing the neighbouring tiles' furniture.
	public Furniture[] GetNeighboursFurniture ( bool _diagOk )
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

}
