using UnityEngine;
using System.Collections;

public class Tile {

	public World m_world;

	public int X { get; protected set; }
	public int Y { get; protected set; }

	public Tile(World _world, int _x, int _y)
	{
		this.m_world = _world;
		this.X = _x;
		this.Y = _y;
	}


}
