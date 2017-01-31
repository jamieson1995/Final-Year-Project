using UnityEngine;
using System.Collections.Generic;
using System;

public class World {

	Tile[,] m_tiles;

	public List<Character> m_characters { get; protected set; } //List of all characters in the world.

	public int m_width  {get; protected set;}

	public int m_height {get; protected set;}

	public Path_TileGraph m_tileGraph;

	public List<Furniture> m_furnitures { get; protected set; }

	public Dictionary<string, List<Furniture> > m_furnitureInMap;

	public Dictionary<string, Furniture> m_furniturePrototypes { get; protected set; }

	public Action<Furniture> cbFurnitureCreated;
	public Action<Character> cbCharacterCreated;

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

		CreateFurniturePrototypes ();

		m_furnitures = new List<Furniture> ();
		m_furnitureInMap = new Dictionary<string, List<Furniture> > ();
		m_characters = new List<Character> ();

		CreateCharacter( GetTileAt( m_width/2, m_height/2 ) );
	}

	//Returns a Tile at a certain coordinate
	public Tile GetTileAt ( int _x, int _y )
	{
		//Need to check to see if the tile that is being asked for is actually within the bounds of the world.
		if ( _x >= m_width || _x < 0 || _y >= m_height || _y < 0 )
		{
			//Debug.LogError("Asked for a tile that is outside the world area");
			return null;
		}

		return m_tiles[_x,_y];
	}

	public void Update ( float _deltaTime )
	{
		foreach ( Character c in m_characters )
		{
			c.Update(_deltaTime);
		}
		foreach ( Furniture f in m_furnitures )
		{
			f.Update(_deltaTime);
		}
	}

	public Character CreateCharacter ( Tile _tile )
	{
		Character c = new Character ( _tile );	
		if ( cbCharacterCreated != null )
		{
			cbCharacterCreated(c);
		}

		m_characters.Add(c);

		return c;
	}

	void CreateFurniturePrototypes ()
	{

		m_furniturePrototypes = new Dictionary<string, Furniture> ();

		m_furniturePrototypes.Add ( "Wall_Plain", 
			new Furniture (
				"Plain",
				"Wall",
				0, 		//Impassable
				1, 		// Width
				1,  	// Height
				true,   // Links to neighbour
				true	//Draggable	
			) 
		);

		m_furniturePrototypes.Add ( "Door_Sliding", 
			new Furniture (
				"Sliding",
				"Door",
				1, 		//Pathfinding Cost
				1, 		// Width
				1,  	// Height
				false, 	// Links to neighbour
				false	// Draggable
			) 
		);
		
		m_furniturePrototypes [ "Door_Sliding" ].m_furnParameters [ "m_openness" ] = 0;
		m_furniturePrototypes [ "Door_Sliding" ].m_furnParameters [ "m_isOpening" ] = 0;
		m_furniturePrototypes [ "Door_Sliding" ].m_updateActions += FurnitureActions.Door_UpdateAction;	
		
		m_furniturePrototypes [ "Door_Sliding" ].m_isEnterable = FurnitureActions.Door_IsEnterable;

	}

	public bool IsFurniturePlacementValid ( string _furnitureType, Tile _tile )
	{
		return m_furniturePrototypes[_furnitureType].IsValidPosition(_tile);
	}

	//Attempts to place a furniture onto a specified tile. If it fails this returns null, else it returns THE instance of the furniture
	//that got placed.
	public Furniture PlaceFurnitureInWorld ( string _furnName, Tile _tile )
	{
		if ( m_furniturePrototypes.ContainsKey ( _furnName ) == false )
		{
			Debug.LogError ( "m_furniturePrototypes doesn't contain a prototype for key: " + _furnName );
			return null;
		}

		Furniture furn = Furniture.PlaceInstanceOfFurniture ( m_furniturePrototypes [ _furnName ], _tile );

		if ( furn == null )
		{
			return null;
		}

		m_furnitures.Add ( furn );
		if ( m_furnitureInMap.ContainsKey ( furn.m_baseFurnType + "_" + furn.m_furnType ) == false )
		{
			m_furnitureInMap.Add ( furn.m_baseFurnType + "_" + furn.m_furnType, new List<Furniture> () );
		}
		m_furnitureInMap[furn.m_baseFurnType + "_" + furn.m_furnType].Add(furn);

		if ( cbFurnitureCreated != null )
		{
			cbFurnitureCreated(furn);
			InvalidateTileGraph();
		}

		return furn;
	}

	public string GetBaseFurnTypeWithName ( string _furnName )
	{
		return m_furniturePrototypes[_furnName].m_baseFurnType;
	}

	public void InvalidateTileGraph()
	{
		m_tileGraph = null;
	}

	public void RegisterFurnitureCreated ( Action<Furniture> _callbackFunc )
	{
		cbFurnitureCreated += _callbackFunc;
	}

	public void UnregisterFurnitureCreated ( Action<Furniture> _callbackFunc )
	{
		cbFurnitureCreated -= _callbackFunc;
	}

	public void RegisterCharacterCreated ( Action<Character> _callbackFunc )
	{
		cbCharacterCreated += _callbackFunc;
	}

	public void UnregisterCharacterCreated ( Action<Character> _callbackFunc )
	{
		cbCharacterCreated -= _callbackFunc;
	}
	
}
