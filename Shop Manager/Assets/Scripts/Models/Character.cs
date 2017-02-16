using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// A character is an abstract thing. It can either be an employee or a character.
/// </summary>
public abstract class Character {

	public float X //Character's X Coordinate
	{ 
		get
		{
			return Mathf.Lerp( m_currTile.X, m_nextTile.X, m_movementPercentage );
		}
	} 
	public float Y //Character's Y Coordinate
	{
		get
		{
			return Mathf.Lerp( m_currTile.Y, m_nextTile.Y, m_movementPercentage );
		}
	}

	public string m_name { get; protected set; } //Character's Name

	public Tile m_currTile {get; protected set;}
	protected Tile m_destTile;
	protected Tile m_nextTile; //The next tile in the pathfinding sequence
	protected Path_AStar m_pathAStar;
	protected float m_movementPercentage;

	protected Stock m_stock;

	//Tiles per second
	protected float m_maxSpeed = 4f; //Character's default speed
	public float m_currentSpeed;

	Furniture m_pushingFurniture;
	Furniture m_pullingFurniture;

	Action<Character> cbCharacterChanged;

	//Spawns a character on a designated tile.
	public Character ( string _name, int _maxCarryWeight, Tile _tile )
	{
		m_currTile = m_destTile = m_nextTile = _tile; //Set all three tile variables to tile the character will spawn on.
		m_currentSpeed = m_maxSpeed;

		m_name = _name;
		//TODO: Add default values here such as stock carried or jobs assigned.
	}

	public void Update( float _deltaTime )
	{
		Update_DoMovement( _deltaTime );
		Update_DoThink();
	}

	void Update_DoMovement ( float _deltaTime )
	{
		float distToTravel;

		if ( m_currTile == m_destTile )
		{
			m_pathAStar = null;
			m_currTile.IsEnterable ();
			return;	// We're already where we want to be.
		}

		if ( m_nextTile == null || m_nextTile == m_currTile )
		{
			// Get the next tile from the pathfinder.
			if ( m_pathAStar == null || m_pathAStar.Length () == 0 )
			{
				// Generate a path to our destination
				m_pathAStar = new Path_AStar ( WorldController.instance.m_world, m_currTile, m_destTile );	// This will calculate a path from curr to dest.
				if ( m_pathAStar.Length () == 0 )
				{
					Debug.LogError ( "Path_AStar returned no path to destination!" );
					m_pathAStar = null;
					return;
				}
			}

			// Grab the next waypoint from the pathing system!
			m_nextTile = m_pathAStar.Dequeue ();

			if ( m_nextTile == m_currTile )
			{
				Debug.LogError ( "Update_DoMovement - nextTile is currTile?" );
			}
		}

		// At this point we should have a valid nextTile to move to.

		//Calculates the distance from point A to point B
		distToTravel = Mathf.Sqrt (
			Mathf.Pow ( m_currTile.X - m_nextTile.X, 2 ) +
			Mathf.Pow ( m_currTile.Y - m_nextTile.Y, 2 )
		);

		if ( m_nextTile.IsEnterable () == ENTERABILITY.Never )
		{
			Debug.LogError ( "FIXME: A character is trying to enter an unwalkable tile." );
			m_nextTile = null;
			m_pathAStar = null;
			return;
		}
		else if ( m_nextTile.IsEnterable () == ENTERABILITY.Soon )
		{
			//We can't enter the tile now, but should be able to in the future, like a door.
			return;
		}
		else
		{
			//The tile is enterable.	

			if ( m_nextTile.m_furniture != null && m_nextTile.m_furniture.m_movable == true )
			{
				//This tile is enterable, but has furniture on it
				//However, the furniture can be moved, so move it.
				m_pushingFurniture = m_nextTile.m_furniture;
				if ( m_pushingFurniture.m_moving == false && m_pushingFurniture.MoveFurniture ( m_currTile, "Forwards" ) == false )
				{
					m_pushingFurniture = null; //Can't push furniture, so stop trying.
					Debug.LogError ( "Failed to push furniture: " + m_nextTile.m_furniture.m_name );
					m_pathAStar = null;
				}
				else
				{
					m_currentSpeed = m_maxSpeed / m_pushingFurniture.m_movementCost;
					m_pushingFurniture.m_furnParameters [ "m_speed" ] = m_currentSpeed;
					m_pushingFurniture.m_furnParameters [ "m_movementPercentage" ] = 0;
					m_pathAStar = null;
					m_pushingFurniture.m_moving = true;
				}

				if ( m_pushingFurniture == null )
				{
					//There is a piece of furniture in the next tile, but it cannot be pushed.
					//We should try pulling it instead
					m_pullingFurniture = m_nextTile.m_furniture;
					if ( m_pullingFurniture.m_moving == false && m_pullingFurniture.MoveFurniture ( m_currTile, "Backwards" ) == false )
					{
						m_pushingFurniture = null; //Can't push furniture, so stop trying.
						Debug.LogError ( "Failed to pull furniture: " + m_nextTile.m_furniture.m_name );
						m_pathAStar = null;
					}
					else
					{
						m_currentSpeed = m_maxSpeed / m_pullingFurniture.m_movementCost;
						m_pullingFurniture.m_furnParameters [ "m_speed" ] = m_currentSpeed;
						m_pullingFurniture.m_furnParameters [ "m_movementPercentage" ] = 0;
						m_pathAStar = null;
						m_pullingFurniture.m_moving = true;
					}
					if ( m_pullingFurniture == null )
					{
						//We have tried pushing and pulling this piece of furniture but couldn't 
						//therefore it cannot be moved from this position
						Debug.LogError ( "Failed to push AND pull furniture: " + m_nextTile.m_furniture.m_name );
						return;
					}
				}
			}		

			if ( ( m_pushingFurniture != null &&
				   m_pushingFurniture.m_furnParameters [ "m_currTile.X" ] == m_pushingFurniture.m_furnParameters [ "m_destTile.X" ] &&
				   m_pushingFurniture.m_furnParameters [ "m_currTile.Y" ] == m_pushingFurniture.m_furnParameters [ "m_destTile.Y" ] ) ||
			     ( m_pullingFurniture != null &&
				   m_pullingFurniture.m_furnParameters [ "m_currTile.X" ] == m_pullingFurniture.m_furnParameters [ "m_destTile.X" ] &&
				   m_pullingFurniture.m_furnParameters [ "m_currTile.Y" ] == m_pullingFurniture.m_furnParameters [ "m_destTile.Y" ] ) )
			{
				//We are pushing or pulling a piece of furniture, but it has reached its destination. So stop pushing or pulling it, and walk at full speed.
				m_pushingFurniture = null;
				m_pullingFurniture = null;
				m_currentSpeed = m_maxSpeed;
			}
		}



		// How much distance can be travel this Update?
		float distThisFrame = m_currentSpeed / m_nextTile.m_movementCost * _deltaTime;

		// How much is that in terms of percentage to our destination?
		float percThisFrame = distThisFrame / distToTravel;

		// Add that to overall percentage travelled.
		m_movementPercentage += percThisFrame;

		if(m_movementPercentage >= 1) {
			// We have reached our destination

			m_currTile = m_nextTile;
			m_movementPercentage = 0;
			// FIXME?  Do we actually want to retain any overshot movement?
		}

		if ( cbCharacterChanged != null )
		{
			cbCharacterChanged(this);
		}
	}

	protected virtual void Update_DoThink()
	{
	}

	public void SetDestination( Tile _tile )
	{
		m_destTile = _tile;
		m_pathAStar = null;
	}

	/// <summary>
	/// the character tries to pick up any piece of stock from the specified piece of furniture.
	/// </summary>
	protected bool TryTakeAnyStock ( Furniture _furn, bool _scannedMatters )
	{
		foreach ( var stockList in _furn.m_stock )
		{
			foreach ( Stock stock in _furn.m_stock[stockList.Key] )
			{
				if ( _scannedMatters == true )
				{
					if ( stock.m_scanned == true )
					{
						continue;
					}
				}
				if ( TryTakeStock ( stock, _furn ) == false )
				{
					continue;
				}
				else
				{
					Debug.Log("Successfully picked up: " + stock.Name);
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// The character tries to pick up the specified piece of stock from the specified piece of furniture.
	/// </summary
	protected bool TryTakeStock ( Stock _stock, Furniture _furn )
	{

		if ( m_stock != null)
		{
			Debug.LogWarning("Tried to pick up stock but already carrying stock: Character: " + m_name + ", AttemptedStock: " + _stock.IDName);
			return false;
		}

		if ( _furn.TryGiveStock ( _stock.IDName ) )
		{
			m_stock = _stock;

			return true;
		}

		return false;
	}

	protected Stock TryGiveStock ( string _stock )
	{
		if ( m_stock.IDName != _stock )
		{
			Debug.LogWarning ( "Character tried to give stock they don't possess: Character: " + m_name + ", Stock: " + _stock );
			return null;
		}

		Stock stock = m_stock;

		m_stock = null;

		return stock;
	}

	public void RegisterOnChangedCallback( Action<Character> _callbackFunc )
	{
		cbCharacterChanged += _callbackFunc;
	}

	public void UnregisterOnChangedCallback( Action<Character> _callbackFunc )
	{
		cbCharacterChanged -= _callbackFunc;
	}
}
