using UnityEngine;
using System.Collections;

public class Furniture {

	Tile m_tile;

	string m_furnitureType;

	int m_width;

	int m_height;

	bool m_linksToNeighbour; //If this true then this furniture's sprite will change if there is a neighbour that needs it to change, i.e. walls.

	//Used in conjuction with the virtual clone function.
	protected Furniture ( Furniture _other )
	{
		this.m_furnitureType = _other.m_furnitureType;
		this.m_width = _other.m_width;
		this.m_height = _other.m_height;
		this.m_linksToNeighbour = _other.m_linksToNeighbour;
	}

	//When placing furniture, the actual furniture isn't being placed, a cloned version of the default furniture is. Therefore, a temp furniture needs to be cloned from the
	//original and that is what gets placed.
	virtual public Furniture Clone ()
	{
		return new Furniture(this);
	}

	public Furniture ( string _furnitureType, int _width, int _height, bool _linksToNeighbour )
	{
		this.m_furnitureType = _furnitureType;
		this.m_width = _width;
		this.m_height = _height;
		this.m_linksToNeighbour = _linksToNeighbour;
	}

	//Attempts to place a certain furniture onto a given tile, if successful, a copy of that furniture is returned.
	static public Furniture PlaceInstanceOfFurniture ( Furniture _other, Tile _tile )
	{
		Furniture furn = _other.Clone (); 
		furn.m_tile = _tile;

		if ( _tile.PlaceFurniture ( furn ) == false )
		{
			return null;
		}

		return furn;
	}
}
