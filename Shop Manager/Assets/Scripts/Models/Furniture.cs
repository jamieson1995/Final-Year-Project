//////////////////////////////////////////////////////
//Copyright James Jamieson 2016/2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections.Generic;

public class Furniture {

	public float X
	{
		get
		{
			if ( m_movable == true )
			{
				return Mathf.Lerp ( m_furnParameters [ "m_currTile.X" ], m_furnParameters [ "m_destTile.X" ], m_furnParameters [ "m_movementPercentage" ] );
			}
			else
			{
				return m_tile.X;
			}
		}
	}

	public float Y
	{
		get
		{
			if ( m_movable == true )
			{
				return Mathf.Lerp ( m_furnParameters [ "m_currTile.Y" ], m_furnParameters [ "m_destTile.Y" ], m_furnParameters [ "m_movementPercentage" ] );
			}
			else
			{
				return m_tile.Y;
			}
		}
	}

	public Tile m_tile;

	public Tile m_jobTile { get; protected set; }

	public Dictionary<string, List<Stock> > m_stock { get; protected set; }

	public string m_name {get; protected set;} 

	public float m_movementCost { get; protected set; } //This is a multiplier. So a value of '2' here means it takes 2 times as long to walk through this tile.
														//SPECIAL: If this is 0, it means the furniture is impassible (e.g. a wall)

	public int Width { get; protected set; }

	public int Height { get; protected set; }

	public bool m_linksToNeighbour { get; protected set; } //If this true then this furniture's sprite will change if there is a neighbour that needs it to change, i.e. walls.

	public bool m_draggable { get; protected set; } //This variable determines whether the furniture can be dragged along to be placed down
													//i.e. walls
												
	public bool m_movable { get; protected set; }

	public int m_rotation { get; protected set; } //1 - Default - South Facing
												  //2 - East Facing
												  //3 - North Facing
												  //4 - West Facing

	public bool m_moving;

	public bool m_manned;

	public int m_maxCarryWeight { get; protected set; } //This represents the maximum amount of stock that this furniture can carry.

	public int m_weightUsed { get; protected set; } //This represents the weight used, based on the stock currently on the furniture.

	public Action<Furniture> cbOnChanged; //After this action gets activated, whenever it gets called, a given event will trigger, i.e changing the visual of this furniture

	public Func<Furniture, ENTERABILITY> m_isEnterable;

	Func<Tile, int, bool> funcPositionValidation; //This runs a function and checks the given tile to see if the placement of the furniture is valid, it returns a bool.

	public Dictionary<string, float> m_furnParameters; //This is used in conjuction with the updateActions action, so that parameters can be used.

	public Action<Furniture, float> m_updateActions; //This is an action, which when activated, allows this tile to run its Update function
													 //This means only furniture that need update functions can have them run i.e. doors for opening.


	public Furniture ()
	{

	}
								
	public Furniture ( string _name, float _movementCost, int _width, int _height, bool _linksToNeighbour, bool _draggable, int _maxCarryWeight, bool _movable)
	{
		this.m_name = _name;
		this.m_movementCost = _movementCost;
		this.Width = _width;
		this.Height = _height;
		this.m_linksToNeighbour = _linksToNeighbour;
		this.m_draggable = _draggable;
		this.m_maxCarryWeight = _maxCarryWeight;
		this.m_movable = _movable;

		m_furnParameters = new Dictionary<string, float> ();

		m_stock = new Dictionary<string, List<Stock>>();
	}

	//When placing furniture, the actual furniture isn't being placed, a cloned version of the default furniture is. 
	//Therefore, a temp furniture needs to be cloned from the original and that is what gets placed.
	public Furniture Clone ()
	{
		return new Furniture(this);
	}

	//Used in conjuction with the virtual Clone function.
	protected Furniture ( Furniture _other )
	{
		this.m_name = _other.m_name;
		this.m_movementCost = _other.m_movementCost;
		this.Width = _other.Width;
		this.Height = _other.Height;
		this.m_linksToNeighbour = _other.m_linksToNeighbour;
		this.m_draggable = _other.m_draggable;
		this.m_maxCarryWeight = _other.m_maxCarryWeight;
		this.m_movable = _other.m_movable;

		m_furnParameters = new Dictionary<string, float> ( _other.m_furnParameters );
		if ( _other.m_updateActions != null )
		{
			this.m_updateActions = (Action<Furniture, float>)_other.m_updateActions.Clone ();
		}

		this.m_isEnterable = _other.m_isEnterable;
		m_stock = new Dictionary<string, List<Stock>>();
	}


	//Attempts to place a certain furniture onto a given tile, if successful, a copy of that furniture is returned.
	static public Furniture PlaceInstanceOfFurniture ( Furniture _other, Tile _tile, int _direction = 1 )
	{

		Furniture furn = _other.Clone (); 

		//if ( furn.funcPositionValidation ( _tile, _direction ) == false )
		if ( WorldController.instance.m_world.PositionCheck(_tile, _other, _direction) == false ) 
		{
			Debug.LogError ( "PlaceInstance -- Position validity function returned FALSE" );
			return null;
		}

		furn.RotateFurniture(_direction);

		furn.m_tile = _tile;
		if ( furn.m_rotation == 1 )
		{
			furn.m_jobTile = WorldController.instance.m_world.GetTileAt ( _tile.X + ( furn.Width / 2 ), _tile.Y + ( furn.Height / 2 ) - 1 );
		}
		else if ( furn.m_rotation == 2 )
		{
			furn.m_jobTile = WorldController.instance.m_world.GetTileAt ( _tile.X + ( furn.Width / 2 ) + 1, _tile.Y + ( furn.Height / 2 ) );
		}
		else if ( furn.m_rotation == 3 )
		{
			furn.m_jobTile = WorldController.instance.m_world.GetTileAt ( _tile.X + ( furn.Width / 2 ), _tile.Y + ( furn.Height / 2 ) + 1 );
		}
		else if ( furn.m_rotation == 4 )
		{
			furn.m_jobTile = WorldController.instance.m_world.GetTileAt ( _tile.X + ( furn.Width / 2 ) - 1, _tile.Y + ( furn.Height / 2 ) );
		}

		if ( _tile.PlaceFurniture ( furn, _direction) == false )
		{
			//For some reason, we were unable to place our furniture in this tile.
			//Probably is was already occupied.
			return null;
		}

		if ( furn.m_linksToNeighbour )
		{
			Tile[] tiles = _tile.GetNeighbours ( false );

			if ( tiles[ 0 ] != null && tiles [ 0 ].m_furniture != null && tiles [ 0 ].m_furniture.m_linksToNeighbour)
			{
				//We have a northern neighbour that links with us. So tell it to change its visuals
				tiles[0].m_furniture.cbOnChanged(tiles[0].m_furniture);
			}
			if ( tiles[1] != null && tiles[1].m_furniture != null && tiles [ 1 ].m_furniture.m_linksToNeighbour)
			{
				//We have a eastern neighbour that links with us. So tell it to change its visuals
				tiles[1].m_furniture.cbOnChanged(tiles[1].m_furniture);
			}
			if ( tiles[2] != null && tiles[2].m_furniture != null && tiles [ 2 ].m_furniture.m_linksToNeighbour)
			{
				//We have a southern neighbour that links with us. So tell it to change its visuals
				tiles[2].m_furniture.cbOnChanged(tiles[2].m_furniture);
			}
			if ( tiles[3] != null && tiles[3].m_furniture != null && tiles [ 3 ].m_furniture.m_linksToNeighbour)
			{
				//We have a western neighbour that links with us. So tell it to change its visuals
				tiles[3].m_furniture.cbOnChanged(tiles[3].m_furniture);
			}
		}

		return furn;
	}

	public void Update ( float _deltaTime )
	{
		if ( m_updateActions != null )
		{
			m_updateActions (this, _deltaTime);
		}
	}

	public bool TryAddStock ( Stock _stock )
	{
		if ( _stock == null )
		{
			Debug.LogError("Trying to add null stock to furniture: Furniture: " + m_name);
			return false;
		}
		if ( m_weightUsed + _stock.Weight > m_maxCarryWeight )
		{
			Debug.LogWarning ( "Tried to add stock to furniture but it was too heavy: Stock: " + _stock.Name + ", Furniture " + m_name );
			return false;
		}

		if ( m_stock.ContainsKey ( _stock.IDName ) == false)
		{
			m_stock.Add(_stock.IDName, new List<Stock>() );
		}

		m_stock[_stock.IDName].Add(_stock);

		m_weightUsed += _stock.Weight;

		return true;
	}

	public bool TryGiveStock ( string _stock )
	{
		if ( m_stock.ContainsKey ( _stock ) )
		{
			Stock stock = m_stock [ _stock ] [ 0 ];
			m_stock [ _stock ].Remove ( stock );
			if ( m_stock [ _stock ].Count == 0 )
			{
				m_stock.Remove(_stock);
			}
			m_weightUsed -= WorldController.instance.m_world.m_stockPrototypes[_stock].Weight;
			return true;
		}

		Debug.LogError("Tried to remove stock from furniture that didn't have any: Furniture: " + m_name + ", Stock: " + _stock);
		return false;
	}

	/// <summary>
	/// Sets a furniture's rotation to the specifed direction   
	/// <para> 1 - Default - South Facing </para>
	/// <para> 2 - East Facing </para>
	/// <para> 3 - North Facing </para>
	/// <para> 4 - west Facing </para>
	/// </summary>
	public void RotateFurniture ( int _direction = 1 )
	{
		World world = WorldController.instance.m_world;
		this.m_rotation = _direction;

		if ( _direction == 1 || _direction == 3 )
		{
			this.Width = world.m_furniturePrototypes [ this.m_name ].Width;
			this.Height = world.m_furniturePrototypes [ this.m_name ].Height;
		}
		else
		{
			this.Width = world.m_furniturePrototypes [ this.m_name ].Height;
			this.Height = world.m_furniturePrototypes [ this.m_name ].Width;
		}
	}

	public bool MoveFurniture ( Tile _toTile )
	{
		if ( m_movable == false )
		{
			//Cannot move this furniture!
			return false;
		}
		m_furnParameters [ "m_movementPercentage" ] = 0;
		m_furnParameters [ "m_destTile.X" ] = _toTile.X;
		m_furnParameters [ "m_destTile.Y" ] = _toTile.Y;
		m_tile.m_world.MoveFurniture ( this, m_tile, _toTile );

		return true;
	}

	//When this function is called, the callback will be activated.
	public void RegisterOnChangedCallback( Action<Furniture> _callback )
	{
		cbOnChanged += _callback; //When this activates, the furniture knows it needs to re-evaluate its properties and update itself, such as change its visuals.
	}
	public void UnRegisterOnChangedCallback( Action<Furniture> _callback )
	{
		cbOnChanged -= _callback; //This function deactivates the callback, and the callback no longer gets run when the callback is called.
								  //This is useful when, for example, a piece of furniture is destroyed.
	}
}
