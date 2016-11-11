using UnityEngine;
using System.Collections;

public class Tile {

	public World m_world;

	public int X { get; protected set; }
	public int Y { get; protected set; }

	Furniture m_furniture;

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

		if ( m_furniture != null )
		{
			Debug.LogError("Trying to place furniture on a tile that already has furniture");
		}

		m_furniture = _furn;

		return true;
	}

}
