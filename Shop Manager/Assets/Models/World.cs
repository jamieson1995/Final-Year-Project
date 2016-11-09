using UnityEngine;
using System.Collections;

public class World {

	Tile[,] m_tiles;

	int m_height;
	int m_width;

	public World(int _height = 50, int _width = 50)
	{
		for (int x = 0; x < _width; x++) {
			for (int y = 0; y < _height; y++) {
				m_tiles [x, y] = new Tile (x, y);
			}
		}
	}
}
