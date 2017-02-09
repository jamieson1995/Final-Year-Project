using UnityEngine;
using System.Collections.Generic;
using System;

public class World {

	public bool m_worldSetUp { get; protected set; }

	Tile[,] m_tiles;

	public List<Character> m_characters { get; protected set; } //List of all characters in the world.
	public List<Employee> m_employees { get; protected set; } //List of all employees in the world.

	public int m_width  {get; protected set;}

	public int m_height {get; protected set;}

	public Path_TileGraph m_tileGraph;

	/// <summary>
	/// List of all furniture in the world, ordered by first placed.
	///	Can be used to find out if a specific piece of furniture is in the game.
	/// </summary>
	public List<Furniture> m_furnitures { get; protected set; }

	/// <summary>
	/// Dictionary of all furniture in the world, sorted and grouped by full name.
	/// </summary>
	public Dictionary<string, List<Furniture> > m_furnitureInWorld;

	/// <summary>
	/// Dictionary of all furniture avaliable in the game, look up by full name.
	/// </summary>
	/// <value>The m furniture prototypes.</value>
	public Dictionary<string, Furniture> m_furniturePrototypes { get; protected set; }

	/// <summary>
	/// Dictionary of all Stock araliable in the game, look up by full name.
	/// </summary>
	public Dictionary<string, Stock> m_stockPrototypes { get; protected set; }

	/// <summary>
	/// Dictionary of all Stock the shop currently sells, sorted and grouped by name.
	/// </summary>
	public Dictionary<string, List<Stock> > m_stockInWorld { get; protected set; } //TODO this should only be avaliable to characters if
																				   //they have access to the manager's computer with that
																				   //information on, or possibly any computer in the 
																				   //store connected to the intranet.

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

		Debug.Log("World created with " + m_width * m_height + " tiles.");

		CreateFurniturePrototypes ();
		CreateStockPrototypes();

		m_furnitures = new List<Furniture> ();
		m_furnitureInWorld = new Dictionary<string, List<Furniture> > ();
		m_characters = new List<Character> ();
		m_employees = new List<Employee> ();

		CreateEmployee( GetTileAt( m_width/2, m_height/2 ) );
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

	Employee CreateEmployee ( Tile _tile )
	{
		Employee e = new Employee ( "James", 5000, _tile, "Manager" );	
		if ( cbCharacterCreated != null )
		{
			cbCharacterCreated(e);
		}

		m_characters.Add(e);
		m_employees.Add(e);

		return e;
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
				true,	//Draggable	
				0       //Max Carry Weight
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
				false,	// Draggable
				0       //Max Carry Weight
			) 
		);
		
		m_furniturePrototypes [ "Door_Sliding" ].m_furnParameters [ "m_openness" ] = 0;
		m_furniturePrototypes [ "Door_Sliding" ].m_furnParameters [ "m_isOpening" ] = 0;
		m_furniturePrototypes [ "Door_Sliding" ].m_updateActions += FurnitureActions.Door_UpdateAction;	
		
		m_furniturePrototypes [ "Door_Sliding" ].m_isEnterable = FurnitureActions.Door_IsEnterable;

		m_furniturePrototypes.Add ( "Movable_Stockcage", 
			new Furniture (
				"Stockcage",
				"Movable",
				5, 		//Pathfinding Cost
				1, 		// Width
				1,  	// Height
				false, 	// Links to neighbour
				false,	// Draggable
				75000   //Max Carry Weight
			) 
		);

		m_furniturePrototypes.Add ( "Other_Checkout", 
			new Furniture (
				"Checkout",
				"Other",
				5, 		//Pathfinding Cost
				2, 		// Width
				1,  	// Height
				false, 	// Links to neighbour
				false,	// Draggable
				10000    //Max Carry Weight
			) 
		);

	}

	void CreateStockPrototypes ()
	{
		m_stockPrototypes = new Dictionary<string, Stock>();

		m_stockPrototypes.Add("Cola_Pepsi",
			new Stock(
				"Cola_Pepsi",    //IDName
				"Pepsi Cola",    //Name
				2000, 		     //Weight
				199, 		     //Price
				Temperature.Room //Temperature
			)

		);

		m_stockPrototypes.Add("Cheese_and_Onion_Crisps_Walkers",
			new Stock(
				"Cheese_and_Onion_Crisps_Walkers", //IDName
				"Walkers Cheese and Onion Crisps", //Name
				25,                                //Weight
				59,                                //Price
				Temperature.Room                   //Temperature
			)

		);
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
		if ( m_furnitureInWorld.ContainsKey ( furn.m_baseFurnType + "_" + furn.m_furnType ) == false )
		{
			m_furnitureInWorld.Add ( furn.m_baseFurnType + "_" + furn.m_furnType, new List<Furniture> () );
		}
		m_furnitureInWorld[furn.m_baseFurnType + "_" + furn.m_furnType].Add(furn);

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

	//Attempts to place a furniture onto a specified tile with specified stock. If it fails this returns null, else it returns THE instance of the furniture
	//that got placed.
	public Furniture PlaceFurnitureInWorldWithStock ( string _furnName, Tile _tile, List<Stock> _stock )
	{
		Furniture furn = PlaceFurnitureInWorld ( _furnName, _tile );

		foreach ( Stock stock in _stock )
		{
			if ( furn.TryAddStock ( stock ) == false )
			{
				Debug.LogError("World tried to add stock to furniture but failed: Stock: " + stock.Name + ", Furniture " + furn.m_furnType);
				continue;
			}
		}

		return furn;
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
