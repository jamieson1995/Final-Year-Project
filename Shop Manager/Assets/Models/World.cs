using UnityEngine;
using System.Collections;

public class World {

	Tile[,] m_tiles;
	public int m_width  {get; protected set;}
	public int m_height {get; protected set;}

	public World (int _width = 50, int _height = 50){
		this.m_width = _width;
		this.m_height = _height;

		m_tiles = new Tile[_width, _height];

		for	( int _x = 0; _x < _width; _x++ )
		{
			for ( int _y = 0; _y < _height; _y++ )
			{
				m_tiles[_x, _y] = new Tile ( this, _x, _y);
			}
		}

		Debug.Log("World created with " + m_width * m_height + " tiles.");
	}

	//Returns a Tile at a certain coordinate
	public Tile GetTileAt ( int _x, int _y )
	{
		//Need to check to see if the tile that is being asked for is actually within the bounds of the world.
		if ( _x >= m_width || _x < 0 || _y >= m_height || _y < 0 )
		{
			Debug.LogError("Asked for a tile that is outside the world area");
			return null;
		}

		return m_tiles[_x,_y];
	}
	
}
