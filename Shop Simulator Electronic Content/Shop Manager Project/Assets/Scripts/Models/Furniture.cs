//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections.Generic;

public class Furniture {

	/// Furniture's X coordinate
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
				return m_mainTile.X;
			}
		}
	}

	/// Furniture's Y coordinate
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
				return m_mainTile.Y;
			}
		}
	}

	/// Reference to this furniture's main Tile. Tile in which placement was made.
	public Tile m_mainTile;

	/// Reference to this furniture's middle Tile. If height or width is even, Tile is right-most middle tile.
	public Tile m_middleTile;

	/// Reference to this furniture's action Tile. Job tile is the one character's stand in if this furniture is used for a task
	public Tile m_actionTile { get; protected set; }

	/// List of Stock types in this Tile. Input is the Stock's name.
	public Dictionary<string, List<Stock> > m_stock { get; protected set; }

	/// Represents this furniture name.
	public string m_name {get; protected set;} 

	/// Represents this furniture movement cost of a character passing through this furniture's Tiles. 
	/// This is a mulitplier, i.e. a value of 2 would cause a character to pass through this tile 2x as slowly.
	/// If this value is 0, characters cannot pass through this furniture's Tiles.
	public float m_movementCost { get; protected set; }

	/// The Width of this furniture in Tiles
	public int Width { get; protected set; }

	/// The Height of this furniture in Tiles.
	public int Height { get; protected set; }

	/// Flag to determine if this furniture links to neighbouring furniture of the same name.
	public bool m_linksToNeighbour { get; protected set; }

	/// Flag to determine is characters can move this furniture.
	public bool m_movable { get; protected set; }

	/// The rotation of this furniture.
	public int m_rotation { get; protected set; } //1 - Default - South Facing
												  //2 - East Facing
												  //3 - North Facing
												  //4 - West Facing

	/// Flag to determine if a character is currently moving this furniture.
	public bool m_moving;

	/// Flag to determine if an employee is currently manning this furniture.
	public bool m_manned;

	/// The maximum of Stock weight allowed on this furniture.
	public int m_maxCarryWeight { get; protected set; }

	/// Current amount of Stock weight used on this furniture.
	public int m_weightUsed { get; protected set; }

	/// Flag to determine if Stock can be added to this furniture.
	public bool m_full { get; protected set; }

	/// Flag to determine if all stock on this furniture has been worked.
	public bool m_allStockWorked;

	/// Current percentage of stock faced up.
	public float m_facedUpPerc { get; protected set; }

	/// Flag that represents if this furniture has been worked.
	public bool m_worked;

	///Current amount of money in the furniture. Used mainly for checkouts.
	public int m_money;

	/// Callback for when this furniture changes.
	public Action<Furniture> cbOnChanged; //After this action gets activated, whenever it gets called, a given event will trigger, i.e changing the visual of this furniture

	/// Used to determine if this furniture's tile is currently enterable.
	public Func<Furniture, ENTERABILITY> m_isEnterable;

	/// Contain all of this furniture's parameters for its updates. Used in conjunction with updateActions.
	public Dictionary<string, float> m_furnParameters;

	/// An action that  when active allows this furniture to have its own update function.
	public Action<Furniture, float> m_updateActions;


	public Furniture ()
	{

	}

	/// Spawns a new Furniture with the specified information.					
	public Furniture ( string _name, float _movementCost, int _width, int _height, bool _linksToNeighbour, bool _draggable, int _maxCarryWeight, bool _movable)
	{
		this.m_name = _name;
		this.m_movementCost = _movementCost;
		this.Width = _width;
		this.Height = _height;
		this.m_linksToNeighbour = _linksToNeighbour;
		this.m_maxCarryWeight = _maxCarryWeight;
		this.m_movable = _movable;

		m_furnParameters = new Dictionary<string, float> ();

		m_stock = new Dictionary<string, List<Stock>>();
	}

	/// Returns a copy of this furniture
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

	/// Returns the Furniture placed in the specifed Tile. The specified Furniture should be the same as the returned one.
	/// If returns null, placement was unsuccessful.
	static public Furniture PlaceInstanceOfFurniture ( Furniture _other, Tile _tile, int _direction = 1 )
	{

		Furniture furn = _other.Clone (); 

		if ( WorldController.instance.m_world.PositionCheck(_tile, _other, _direction) == false ) 
		{
			return null;
		}

		furn.RotateFurniture(_direction);

		furn.m_mainTile = _tile;
		if ( furn.m_rotation == 1 )
		{
			furn.m_actionTile = WorldController.instance.m_world.GetTileAt ( _tile.X + ( furn.Width / 2 ), _tile.Y + ( furn.Height / 2 ) - 1 );
		}
		else if ( furn.m_rotation == 2 )
		{
			furn.m_actionTile = WorldController.instance.m_world.GetTileAt ( _tile.X + ( furn.Width / 2 ) + 1, _tile.Y + ( furn.Height / 2 ) );
		}
		else if ( furn.m_rotation == 3 )
		{
			furn.m_actionTile = WorldController.instance.m_world.GetTileAt ( _tile.X + ( furn.Width / 2 ), _tile.Y + ( furn.Height / 2 ) + 1 );
		}
		else if ( furn.m_rotation == 4 )
		{
			furn.m_actionTile = WorldController.instance.m_world.GetTileAt ( _tile.X + ( furn.Width / 2 ) - 1, _tile.Y + ( furn.Height / 2 ) );
		}

		furn.m_middleTile = WorldController.instance.m_world.GetTileAt ( _tile.X + ( furn.Width / 2 ) , _tile.Y + ( furn.Height / 2 ) );

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

	/// Runs the Update function for this furniture if relevant.
	public void Update ( float _deltaTime )
	{
		if ( m_updateActions != null )
		{
			m_updateActions (this, _deltaTime);
		}
	}

	/// Returns the attempt's outcome. Attempt to add specified stock to this furniture.
	public bool TryAddStock ( Stock _stock )
	{
		if ( _stock == null )
		{
			Debug.LogError("Trying to add null stock to furniture: Furniture: " + m_name);
			return false;
		}
		if ( m_weightUsed + _stock.Weight > m_maxCarryWeight )
		{
			Debug.LogWarning ( "Tried to add stock to furniture but it was too heavy: Stock: " + _stock.Name + ", Furniture: " + m_name );
			m_full = true;
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

	/// Returns the attempt's outcome. Attempts to find Stock based upon the specified stock's name in this furniture.
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
			m_full = false;
			return true;
		}

		Debug.LogError("Tried to remove stock from furniture that didn't have any: Furniture: " + m_name + ", Stock: " + _stock);
		return false;
	}

	/// Sets this furniture's rotation to the specified direction.
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

	/// Returns the attempt's outcome. Attempts to move this furniture to the specified Tile.
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
		if ( m_mainTile.m_world.MoveFurniture ( this, m_mainTile, _toTile ) == false)
		{
			Debug.LogError ( "Failed to move " + m_name + " to (" + _toTile.X + ", " + _toTile.Y + ")." );
			return false;
		}

		return true;
	}

	/// Changes the m_facedUpPerc variable by the specified amount, capped at 0 and S100.
	public void ChangeFaceUpPerc ( float _amount )
	{
		if ( m_facedUpPerc + _amount > 100f )
		{
			Debug.Log ( "Increasing the m_facedUpPerc by " + _amount + " will cause it to go above 100." );
			m_facedUpPerc = 100;
			m_worked = true;
			return;
		}
		if ( m_facedUpPerc + _amount < 0f )
		{
			Debug.Log ( "Decreasing the m_facedUpPerc by " + _amount + " will cause it to go below 0." );
			m_facedUpPerc = 0;
			m_worked = false;
			return;
		}
		if ( _amount < 0 )
		{
			m_worked = false;
			WorldController.instance.m_world.AllFacingUpFinished = false;
		}
		m_facedUpPerc += _amount;

	}

	/// Sets the m_facedUpPerc variable to the specified amount, capped at 100.
	public void SetFaceUpPerc ( float _amount )
	{
		if ( _amount > 100f)
		{
			Debug.Log("Cannot set m_facedUpPerc to above 100.");
			m_facedUpPerc = 100f;
			return;
		}
		m_facedUpPerc = _amount;
	}

	/// Registers the OnChanged Callback
	public void RegisterOnChangedCallback( Action<Furniture> _callback )
	{
		cbOnChanged += _callback;
	}

	/// Unregisters the OnChanged Callback
	public void UnRegisterOnChangedCallback( Action<Furniture> _callback )
	{
		cbOnChanged -= _callback;
	}
}
