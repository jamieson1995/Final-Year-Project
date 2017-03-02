//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// A character is an abstract entity. It can either be an employee or a customer.
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

	protected World m_world;

	public string m_name { get; protected set; } //Character's Name

	public Tile m_currTile {get; protected set;}
	protected Tile m_destTile; //The final tile in the pathfinding sequence
	protected Tile m_nextTile; //The next tile in the pathfinding sequence
	protected Path_AStar m_pathAStar;

	protected Tile m_movingFurnsFinalTile;

	public Furniture m_requiredFurn { get; protected set; } //Furniture that currently needs to be used, therefore if it is in front of where we need to go, 
															//we are probably moving it, so don't worry about trying to find a spare tile for it.

	enum Direction
	{
		North,
		West,
		South,
		East,
		NONE
	}

	Direction m_directionOfTravelWithFurn;
	protected Tile m_nearestFreeTile;
	protected float m_movementPercentage;

	bool m_pushingFurn;

	protected Stock m_stock;

	protected bool m_ignoreTask;

	//Tiles per second
	protected float m_maxSpeed = 4f; //Character's default speed
	public float m_currentSpeed;

	protected Furniture m_movingFurniture;

	Action<Character> cbCharacterChanged;

	//Spawns a character on a designated tile.
	public Character ( string _name, int _maxCarryWeight, Tile _tile )
	{
		m_world = WorldController.instance.m_world;
		m_currTile = m_destTile = m_nextTile = _tile; //Set all three tile variables to tile the character will spawn on.
		m_currentSpeed = m_maxSpeed;

		m_name = _name;
	}

	public void Update( float _deltaTime )
	{
		Update_DoMovement( _deltaTime );
		Update_DoThink();
	}

	protected void ResetDefaultVariables()
	{
		m_pathAStar = null;
		m_movingFurniture = null;
		m_nearestFreeTile = null;
		m_currentSpeed = m_maxSpeed;
	}

	void Update_DoMovement ( float _deltaTime )
	{
		float distToTravel;

		if ( m_currTile == m_destTile )
		{
			ResetDefaultVariables ();
			m_currTile.IsEnterable ();
			return;	// We're already where we want to be.
		}

		if ( m_nextTile == null || m_nextTile == m_currTile || m_pathAStar == null)
		{
			// Get the next tile from the pathfinder.
			if ( m_pathAStar == null || m_pathAStar.Length () == 0 )
			{
				// Generate a path to our destination
				m_pathAStar = new Path_AStar ( m_world, m_currTile, m_destTile );	// This will calculate a path from curr to dest.
				if ( m_pathAStar.Length () == 0 )
				{
					Debug.LogError ( "Path_AStar returned no path to destination!" );
					m_pathAStar = null;
					return;
				}
			}

			// Grab the next waypoint from the pathing system.
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
			if ( m_nextTile.m_furniture != null && m_nextTile.m_furniture.m_movable == true || m_movingFurniture != null )
			{
				//This tile is enterable, but has furniture on it
				//However, the furniture can be moved, so move it to the nearest tile that is not on the path to our destination.
				if ( m_nextTile != m_currTile && m_nextTile != m_requiredFurn.m_mainTile && m_currTile != m_requiredFurn.m_mainTile && m_movingFurniture == null )
				{
					if ( m_nearestFreeTile == null )
					{
						bool foundValidTile = false;

						List<Tile> invalidTiles = new List<Tile> ();
						foreach ( Tile t in m_pathAStar.CurrPathToArray () )
						{
							invalidTiles.Add ( t );
						}

						for ( int x = 0; x < m_world.m_width; x++ )
						{
							for ( int y = 0; y < m_world.m_height; y++ )
							{
								if ( m_world.GetTileAt ( x, y ).m_outside == true )
								{
									invalidTiles.Add ( m_world.GetTileAt ( x, y ) );
								}
							}
						}

						invalidTiles.Add ( m_currTile );

						while ( foundValidTile == false )
						{
							m_nearestFreeTile = FindValidPositionToMoveFurn ( m_currTile, m_nextTile, invalidTiles );
							Tile tryTile = new FindNearestFreeTile ( m_world, m_nextTile, invalidTiles.ToArray () ).m_tileFound;
							if ( m_nearestFreeTile == tryTile )
							{
								foundValidTile = true;
							}
							else
							{
								invalidTiles.Add ( tryTile );
							}
						}
						if ( m_nextTile.m_furniture != m_requiredFurn )
						{
							m_ignoreTask = true;
						}
						if ( MoveFurnitureToTile ( m_nextTile.m_furniture, m_nearestFreeTile ) == false )
						{
							Debug.LogError ( "Tried to move a piece of furniture somewhere but it returned false. This might mean we are not next to the piece of furniture." );
							return;
						}
						else
						{
							m_movingFurnsFinalTile = m_nearestFreeTile;
						}
					}
				}

				//Now we know where we need to be, and where the furniture needs to be.
				Tile toTile = null;
				if ( m_pushingFurn == true )
				{
					if ( m_currTile == m_nextTile )
					{
						toTile = m_movingFurniture.m_mainTile;
					}
					else if ( m_pathAStar.Length () > 0 )
					{
						toTile = m_pathAStar.CurrPathToArray () [ 0 ];
					}
					else
					{
						toTile = m_nearestFreeTile;
					}
				}
				else if ( m_pushingFurn == false )
				{
					toTile = m_currTile;
				}
				if ( toTile == null )
				{
					Debug.LogError ( "We are not pushing or pulling a furniture. Most likely the direction parameters were wrong." );
					return;
				}

				if ( m_movingFurniture != null && m_movingFurniture.m_moving == false && m_movingFurniture.MoveFurniture ( toTile ) == false )
				{
					Debug.LogError ( "Failed to push furniture: " + m_nextTile.m_furniture.m_name );
					m_pathAStar = null;
				}
				else
				{

					m_currentSpeed = m_maxSpeed / m_movingFurniture.m_movementCost;
					m_movingFurniture.m_furnParameters [ "m_speed" ] = m_currentSpeed;

					m_movingFurniture.m_moving = true;
				}
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
		}

		if ( m_movingFurnsFinalTile != null && m_movingFurniture != null && m_movingFurnsFinalTile == m_movingFurniture.m_mainTile)
		{
			//The furniture was going to a free tile, and we are where we needed to go. So stop pushing it.
			m_movingFurniture.m_moving = false;
			ResetDefaultVariables();
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
		m_nextTile = m_currTile;
		m_destTile = _tile;
	}

	public void SetDestWithFurn ( Tile _characterDestTile, Furniture _furn )
	{
		if ( _characterDestTile.m_movementCost == 0 )
		{
			Debug.LogError("Moving the furniture to (" + _characterDestTile.X + ", " + _characterDestTile.Y + ") means the character will need to go to an invalid tile");
			return;
		}
		m_nextTile = m_currTile;
		m_movingFurniture = _furn;
		SetDestination ( _characterDestTile );
	}

	/// <summary>
	/// This function will recursively check for valid tiles. It will keep calling itself as long as the character needs to walk through the furniture it is moving
	/// to get to its final destination. If this returns null, it means that if you continue moving the furniture to the given spot, the character will need to move it again
	/// and will continue to try to move it to new spots until the character blocks themselves in. If it doesn't return null, it will return the valid spot that can be used
	/// with the knowledge the character will not trap themselves.
	/// </summary>
	Tile FindValidPositionToMoveFurn ( Tile _charTile, Tile _furnTile, List<Tile> _invalidTiles )
	{
		FindNearestFreeTile m_nearestFreeTileTEMP = new FindNearestFreeTile ( m_world, _furnTile, _invalidTiles.ToArray () );
		Tile m_moveFurnToTEMP = m_nearestFreeTileTEMP.m_tileFound;
		//The nearest free tile is the one the furniture needs to get to, so the tile 1 before that is where we need to go.
		Path_AStar m_pathAStarTEMP = new Path_AStar ( m_world, _charTile, m_nearestFreeTileTEMP.m_tileFound );

		if ( m_pathAStarTEMP.Length () == 0 )
		{
			return null;
		}

						
		//We need to work out whether the character needs to push the furniture, or pull it to get it to its destination.
		//If the first entry in the m_pathAstar array contains the furniture we are moving, then we must be pushing it.
		//Otherwise, we are pulling it.
		//If we are pulling it, then the final tile we need to get to will be 1 AFTER the m_pathAStar's final tile.
		//If we are pushing it, then the final tile we need to get to will be 1 BEFORE the m_pathAStar's final tile.
		
		bool m_pushingFurnTEMP;
		if ( m_pathAStarTEMP.InitialPathToArray () [ 0 ] == _furnTile )
		{
			//The next tile we need to go to is the furn's tile. so we must be pushing it.
			m_pushingFurnTEMP = true;
		}
		else
		{
			m_pushingFurnTEMP = false;
		}

		Direction dirFromFurn = CalculateDirectionOfMovement ( _furnTile, _charTile );

		//Now we know where we need to be, and where the furniture needs to be.

		if ( m_pushingFurnTEMP != true )
		{
			Tile finalCharTile = m_pathAStarTEMP.InitialPathToArray () [ m_pathAStarTEMP.InitialPathToArray ().Length - 1 ];
			switch ( dirFromFurn )
			{
				case Direction.North:
					if ( IsThisTileInPath ( m_moveFurnToTEMP, m_world.GetTileAt ( finalCharTile.X, finalCharTile.Y + 1 ) ) )
					{
						_invalidTiles.Add ( _furnTile );
						m_moveFurnToTEMP = FindValidPositionToMoveFurn ( m_world.GetTileAt ( finalCharTile.X, finalCharTile.Y + 1 ), m_moveFurnToTEMP, _invalidTiles );
					}
					else
					{
						return m_moveFurnToTEMP;
					}
					break;
				case Direction.East:
					if ( IsThisTileInPath ( m_moveFurnToTEMP, m_world.GetTileAt ( finalCharTile.X + 1, finalCharTile.Y ) ) )
					{
						_invalidTiles.Add ( _furnTile );
						m_moveFurnToTEMP = FindValidPositionToMoveFurn ( m_world.GetTileAt ( finalCharTile.X + 1, finalCharTile.Y ), m_moveFurnToTEMP, _invalidTiles );
					}
					else
					{
						return m_moveFurnToTEMP;
					}
					break;
				case Direction.South:
					if ( IsThisTileInPath ( m_moveFurnToTEMP, m_world.GetTileAt ( finalCharTile.X, finalCharTile.Y - 1 ) ) )
					{
						_invalidTiles.Add ( _furnTile );
						m_moveFurnToTEMP = FindValidPositionToMoveFurn ( m_world.GetTileAt ( finalCharTile.X, finalCharTile.Y - 1 ), m_moveFurnToTEMP, _invalidTiles );
					}
					else
					{
						return m_moveFurnToTEMP;
					}
					break;
				case Direction.West:
					if ( IsThisTileInPath ( m_moveFurnToTEMP, m_world.GetTileAt ( finalCharTile.X - 1, finalCharTile.Y ) ) )
					{
						_invalidTiles.Add ( _furnTile );
						m_moveFurnToTEMP = FindValidPositionToMoveFurn ( m_world.GetTileAt ( finalCharTile.X - 1, finalCharTile.Y ), m_moveFurnToTEMP, _invalidTiles );
					}
					else
					{
						return m_moveFurnToTEMP;					
					}
					break;
			}
		}
		else
		{
			//We are pushing the furniture
			if ( IsThisTileInPath ( m_moveFurnToTEMP, m_pathAStarTEMP.InitialPathToArray () [ m_pathAStarTEMP.InitialPathToArray ().Length - 2 ] ) )
			{
				_invalidTiles.Add ( _furnTile );
				m_moveFurnToTEMP = FindValidPositionToMoveFurn ( m_pathAStarTEMP.InitialPathToArray () [ m_pathAStarTEMP.InitialPathToArray ().Length - 2 ], m_moveFurnToTEMP, _invalidTiles );
			}
			else
			{
				return m_moveFurnToTEMP;
			}

		}
		if ( m_moveFurnToTEMP == null )
		{
			return null;
		}
		return m_nearestFreeTileTEMP.m_tileFound;
	}

	bool IsThisTileInPath ( Tile _furnTile, Tile _charTile )
	{
		Path_AStar TempPath = new Path_AStar ( m_world, _charTile, m_destTile );
		if ( TempPath.Length () == 0 )
		{
			return true;
		}
		foreach ( Tile t in TempPath.InitialPathToArray() )
		{
			if ( t == _furnTile )
			{
				return true;
			}
		}

		return false;
	}

	protected bool MoveFurnitureToTile ( Furniture _furn, Tile _toTile )
	{
		if ( _furn == null )
		{
			Debug.LogError ( m_name + " is trying to move a null furniture to (" + _toTile.X + ", " + _toTile.Y + ")." );
			return false;
		}

		if ( _furn.m_movable == false )
		{
			Debug.Log ( m_name + " is trying to move a " + _furn.m_name + " but cannot because it is unmovable." );
			return false;
		}

		if ( m_movingFurniture == _furn )
		{
			Tile[] neighbours = _toTile.GetNeighbours ();
			foreach ( Tile t in neighbours )
			{
				if ( m_destTile == t )
				{
					//We are already moving the furniture to where it needs to go.
					return true;
				}
			}
		}


		Direction dirFromFurn = Direction.NONE;

		//Are we next to the piece of furniture we want to move?
		Tile[] neightbours = _furn.m_mainTile.GetNeighbours (true);
		bool nextToFurn = false;
		foreach ( Tile t in neightbours )
		{
			if ( m_currTile == t )
			{
				nextToFurn = true;
				break;
			}
			if ( m_destTile == t )
			{
				nextToFurn = false;
				//We are not next to the furniture, but we are on our way there, so there's nothing we need to do.
				Debug.Log ( "Moving to furniture" );
				return false;
			}
		}

		if ( nextToFurn == false )
		{
			//We are not next to the furniture so we need to get there.
			m_pathAStar = new Path_AStar ( m_world, m_currTile, _furn.m_mainTile );
			if ( m_pathAStar.InitialPathToArray ().Length > 1 )
			{
				SetDestination ( m_pathAStar.InitialPathToArray () [ m_pathAStar.InitialPathToArray ().Length - 2 ] );
				return false;
			}
			else if ( m_pathAStar.InitialPathToArray ().Length == 1 )
			{
				//We are already where we need to be, we shoudl have already known this.
				Debug.LogError ( "We are next to the furniture we want to get to, but the previous logic told us we weren't." );
			}
			else
			{
				//Cannot reach the piece of furniture
				Debug.Log ( "Cannot reach the piece of furniture: " + _furn.m_name );
			}
		}

		//The nearest free tile is the one the furniture needs to get to, so the tile 1 before that is where we need to go.
		m_pathAStar = new Path_AStar ( m_world, m_currTile, _toTile );
		
		//We need to work out whether the character needs to push the furniture, or pull it to get it to its destination.
		//If the first entry in the m_pathAstar array contains the furniture we are moving, then we must be pushing it.
		//Otherwise, we are pulling it.
		//If we are pulling it, then the final tile we need to get to will be 1 AFTER the m_pathAStar's final tile.
		//If we are pushing it, then the final tile we need to get to will be 1 BEFORE the m_pathAStar's final tile.
		
		m_directionOfTravelWithFurn = CalculateDirectionOfMovement ( m_pathAStar.InitialPathToArray () [ 0 ], m_pathAStar.InitialPathToArray () [ 1 ] );
		m_movingFurniture = _furn;

		dirFromFurn = CalculateDirectionOfMovement ( _furn.m_mainTile, m_currTile );
		
		//Now we know where we need to be, and where the furniture needs to be.
		if ( dirFromFurn == m_directionOfTravelWithFurn )
		{
			m_pushingFurn = false;
		}
		else
		{
			m_pushingFurn = true;
		}
		
		//We need to make sure that where we are going to go will not trap oursevles, so we need to keep checking where we will
		//end up and see if there will still be a valid path to put final destination.
		if ( m_pushingFurn != true )
		{
			//We are pulling the furniture
			Tile finalTile = m_pathAStar.InitialPathToArray () [ m_pathAStar.InitialPathToArray ().Length - 1 ];
			switch ( dirFromFurn )
			{
				case Direction.North:
					SetDestWithFurn ( m_world.GetTileAt ( finalTile.X, finalTile.Y + 1 ), _furn );
					break;
				case Direction.East:
					SetDestWithFurn ( m_world.GetTileAt ( finalTile.X + 1, finalTile.Y ), _furn );
					break;
				case Direction.South:
					SetDestWithFurn ( m_world.GetTileAt ( finalTile.X, finalTile.Y - 1 ), _furn );
					break;
				case Direction.West:
					SetDestWithFurn ( m_world.GetTileAt ( finalTile.X - 1, finalTile.Y ), _furn );
					break;
				default:
					return true;
			}
		}
		else
		{
			//We are pushing the furniture
			SetDestWithFurn ( m_pathAStar.InitialPathToArray () [ m_pathAStar.InitialPathToArray ().Length - 2 ], _furn );
		}

		return true;

	}

	Direction CalculateDirectionOfMovement ( Tile _initialTile, Tile _nextTile )
	{

		if ( _initialTile.Y + 1 == _nextTile.Y )
		{
			// We are heading north
			return Direction.North;
		}
		else if ( _initialTile.X + 1 == _nextTile.X )
		{
			// We are heading east
			return Direction.East;
		}
		else if ( _initialTile.Y - 1 == _nextTile.Y )
		{
			// We are heading south
			return Direction.South;
		}
		else if ( _initialTile.X - 1 == _nextTile.X )
		{
			// We are heading west
			return Direction.West;
		}

		Debug.LogError("CalculateDirectionOfMovement -- None of the if statements triggered. Check the parameters.");
		return Direction.NONE;
	}

	/// <summary>
	/// The character tries to pick up any piece of stock from the specified piece of furniture.
	/// </summary>
	protected bool TryTakeAnyStock ( Furniture _furn, bool _scannedMatters = false)
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
