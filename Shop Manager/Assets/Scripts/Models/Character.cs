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

	string m_name; //Character's Name

	public Tile m_currTile {get; protected set;}
	protected Tile m_destTile;
	Tile m_nextTile; //The next tile in the pathfinding sequence
	Path_AStar m_pathAStar;
	float m_movementPercentage;

	Dictionary<string, Stock> m_stock;

	//Tiles per second
	float m_maxSpeed = 4f; //Character's default speed
	public float m_currentSpeed;

	int m_maximumCarryWeight; //The maximum weight of objects the character can carry. Can be stock or other objects.

	Action<Character> cbCharacterChanged;

	//Spawns a character on a designated tile.
	public Character ( Tile _tile )
	{
		m_currTile = m_destTile = m_nextTile = _tile; //Set all three tile variables to tile the character will spawn on.
		m_currentSpeed = m_maxSpeed;
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

			if ( m_nextTile.m_furniture != null  )
			{
				//This tile has furniture on it, so adjust the current speed according to its movement cost.
				m_currentSpeed = m_maxSpeed / m_nextTile.m_furniture.m_movementCost;
			}
			else
				m_currentSpeed = m_maxSpeed;
				
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
		if (m_currTile.IsNeighbour ( _tile, true ) == false )
		{
			Debug.Log("Character::SetDestination -- Our destination tile isn't actually our neighbour.");
		}

		m_destTile = _tile;
		m_pathAStar = null;
		Debug.Log("Dest tile: (" + m_destTile.X + "," + m_destTile.Y + ")");
	}

	protected bool TryTakeStock ( Stock _stock, Furniture _furn )
	{
		if ( _furn.TryTakeStock ( _stock ) )
		{
			m_stock.Add ( _stock.m_name, _stock );
			return true;
		}

		return false;
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
