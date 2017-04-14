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

	/// Character's X Coordinates
	public float X
	{ 
		get
		{
			return Mathf.Lerp( m_currTile.X, m_nextTile.X, m_movementPercentage );
		}
	} 

	/// Character's Y Coordinates
	public float Y
	{
		get
		{
			return Mathf.Lerp( m_currTile.Y, m_nextTile.Y, m_movementPercentage );
		}
	}

	/// Reference to WorldController.instance.m_world
	protected World m_world;

	/// Has the character got a basket. If they do, they can carry more than on piece of stock and thier max carry weight increases.
	public bool m_basket;

	/// String that represent's the charcater's full name.
	public string m_name { get; protected set; }

	/// Reference to the tile the character is currently in.
	public Tile m_currTile {get; protected set;}

	///Reference to the final Tile in the pathfinding sequence.
	protected Tile m_destTile;

	/// Reference to the next tile in the pathfinding sequence.
	protected Tile m_nextTile;

	/// The character's curr tile must be the same as this tasktile in order for the character to be able to continue with thier task/job.
	protected Tile m_taskTile;

	/// Flag to determine if the destination has changed.
	bool m_resetDest;

	/// Reference to the current AStar path.
	protected Path_AStar m_pathAStar;

	/// Reference to the tile that the furniture this character is pushing is finally going to.
	protected Tile m_movingFurnsFinalTile;

	/// Reference to the Furniture that is currently needed for a task.
	public Furniture m_requiredFurn { get; protected set; } 

	/// Flag to determine if this character is currently picking up or putting down a piece of stock.
	public bool m_movingStock { get; protected set; }

	/// Number of seconds it will take to pick up or put down this stock.
	public float m_fullTimeToMoveStock { get; protected set; }

	/// Number of seconds into picking up or putting down this stock.
	public float m_elaspedTimeToMoveStock { get; protected set; }

	/// Number of seconds it will take to finish the current interaction.
	public float m_fullTimeInteraction { get; protected set; }

	/// Number of seconds into the interaction.
	public float m_elaspedTimeInteraction { get; protected set; }

	/// Used when interacting with other character to determine who is in charge of the interaction.
	public int m_authorityLevel;

	/// Flag to determine if this character can continue to their destination while interacting with another character.
	public bool m_walkAndTalk;

	/// All avaiable directions to move in.
	enum Direction
	{
		North,
		West,
		South,
		East,
		NONE
	}

	///Direction that the furniture is next moving in.
	Direction m_directionOfTravelWithFurn;

	///Nearest Tile that the furniture this character is moving can be at, without it obstructing us to our destination.
	protected Tile m_nearestFreeTile;

	/// The percentage this character is along thier current Tile. 1 - 100%, 0 - 0%
	protected float m_movementPercentage;

	/// Flag that determines if we are pushing a furniture, as oppose to pulling it.
	bool m_pushingFurn;

	/// Reference to the Stock this charcater is currently holding.
	protected Stock m_stock;

	protected List<Stock> m_basketContents;

	/// Flag that determines if this character should ignore thier current pathfinding sequence for thier job. For example when moving a furniture out of the way.
	protected bool m_ignoreTask;

	/// The maximum speed of this character. Tiles Per Second.
	protected float m_maxSpeed = 4f; 

	/// The current speed of this character. Different from m_maxSpeed when moving furniture for example. Tiles Per Second.
	public float m_currentSpeed;

	/// Reference to the Furniture this charcater is currently moving.
	protected Furniture m_movingFurniture;

	/// Callback for when this character changes.
	Action<Character> cbCharacterChanged;

	/// Reference to the character that this character is interacting with, if any.
	public Character m_interacting { get; protected set; }

	/// Flag to determine if this character will accept interaction from another character.
	public bool m_canInteract //{ get; protected set; }
	{
		get
		{
			return true;
		}
		set
		{
			m_canInteract = value;
		}
	}

	/// Flag to determine if this character can move from their curr tile.
	public bool m_canMove { get; protected set; }

	/// Spawns a character with the specified name, specified maxCarryWeight, on the specified Tile.
	public Character ( string _name, int _maxCarryWeight, Tile _tile )
	{
		m_world = WorldController.instance.m_world;
		m_currTile = m_destTile = m_nextTile = _tile; //Set all three tile variables to tile the character will spawn on.
		m_taskTile = null;
		m_currentSpeed = m_maxSpeed;

		m_basketContents = new List<Stock>();

		m_name = _name;
	}

	public void Update( float _deltaTime )
	{
		Update_DoMovement( _deltaTime );
		Update_DoThink( _deltaTime );
	}

	/// Sets this character's movement, and furniture variables to their default. m_pathAStar, m_movingFurniture, m_nearestFreeTile, m_currentSpeed.
	protected void ResetDefaultVariables()
	{
		m_pathAStar = null;
		//m_movingFurniture = null;
		m_nearestFreeTile = null;
		m_currentSpeed = m_maxSpeed;
	}

	void Update_DoMovement ( float _deltaTime )
	{

		if ( m_canMove == false )
		{
			//Character cannot move from this location.
			return;
		}

		if ( m_currTile == m_destTile || m_resetDest)
		{
			//We are already where we need to be, so reset all defaults, and return from function.
			ResetDefaultVariables ();
			m_resetDest = false;
			return;
		}

		if ( m_nextTile == null || m_nextTile == m_currTile || m_pathAStar == null)
		{
			//We don't have the next tile, it is the same as our current tile, or we have no path at all.
			if ( m_pathAStar == null || m_pathAStar.Length () == 0 )
			{
				//We have no path, or our path's length is 0, so get a new path.
				m_pathAStar = new Path_AStar ( m_world, m_currTile, m_destTile );
				if ( m_pathAStar.Length () == 0 )
				{
					//If we just found a new path, and its length is 0, clearly we cannot have a valid path to the destination, assumming the destTile was different from the currTile.
					Debug.LogError ( "Path_AStar returned no path to destination!" );
					m_pathAStar = null;
					return;
				}
			}

			//Set pur nextTile to the next tile in the path, and remove it from the path's queue.
			m_nextTile = m_pathAStar.Dequeue ();

			if ( m_nextTile == m_currTile )
			{
				//We just got the next tile, so if its the same as our current tile, something must have gone wrong.
				Debug.LogError ( "Update_DoMovement - nextTile is currTile?" );
			}
		}

		// At this point we should have a valid nextTile to move to.

		//Calculates the distance from point A to point B
		float distToTravel = Mathf.Sqrt (
			Mathf.Pow ( m_currTile.X - m_nextTile.X, 2 ) +
			Mathf.Pow ( m_currTile.Y - m_nextTile.Y, 2 )
		);

		if ( m_nextTile.IsEnterable () == ENTERABILITY.Never )
		{	
			//The next tile cannot be entered, so we need a new path. setting m_pathAStar to null will cause the next frame to give us a new path to our destTile.
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
							//Keep finding new tiles to move to, and keep working out if they are valid.
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
							//The furniture we are moving if not the one we need for our task, so ignore the task so that we can move this furniture to the free tile.
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

		// Work out how much we will be moving this frame.
		float distThisFrame = m_currentSpeed / m_nextTile.m_movementCost * _deltaTime;

		// Work out how much we will be moving this frame as a percentage.
		float percThisFrame = distThisFrame / distToTravel;

		// Add that percentage to our already travelled percentage.
		m_movementPercentage += percThisFrame;

		if(m_movementPercentage >= 1) {

			//If our percentage is 100% or above, we have reached the next tile, so update currTile, and reset the movement percentage.

			m_currTile = m_nextTile;
			m_movementPercentage = 0;
		}

		if ( m_movingFurnsFinalTile != null && m_movingFurniture != null && m_movingFurnsFinalTile == m_movingFurniture.m_mainTile)
		{
			//The furniture was going to a free tile, and we are where we needed to go. So stop pushing it.
			m_movingFurniture.m_moving = false;
			ResetDefaultVariables();
		}

		//Update this character's visuals.
		if ( cbCharacterChanged != null )
		{
			cbCharacterChanged(this);
		}
	}

	///Used as a placeholder for the employee and customer Update_DoThink function.
	protected virtual void Update_DoThink(float _deltaTime)
	{
	}

	/// Returns whether this character can interact with the specified character.
	public bool ReceiveInteraction ( Character _char, float _time )
	{
		if ( m_canInteract )
		{
			m_interacting = _char;
			if ( m_authorityLevel >= _char.m_authorityLevel )
			{
				//Our authority level is the same or higher than the other character's, so we need to stop moving to our task.
				m_ignoreTask = true;
			}
			BeginInteraction(_time);
			return true;

		}
		else
		{
			return false;
		}
	}

	/// Returns the outcome of requesting interaction with the specified character.
	public bool RequestInteraction ( Character _char, float _time )
	{
		if ( m_currTile.IsNeighbour ( _char.m_currTile, true, true ) == false)
		{
			//The character's aren't adjacent.
			return false;
		}
		if ( _char.ReceiveInteraction ( this, _time ) )
		{
			m_interacting = _char;
			if ( m_authorityLevel >= _char.m_authorityLevel )
			{
				//Our authority level is the same or higher than the other character's, so we need to stop moving.
				m_ignoreTask = true;
			}
			Debug.Log(m_name + " and " + _char.m_name + " are interacting");
			BeginInteraction(_time);
			return true;
		}
		else
		{
			return false;
		}
	}

	public void BeginInteraction ( float _time )
	{
		m_fullTimeInteraction = _time;
	}

	public void StopInteraction ()
	{
		m_interacting = null;
	}

	///Sets this character's destTile to the specified Tile, and sets the nextTile to this character's currTile.
	public void SetDestination ( Tile _tile )
	{
		if ( _tile == m_destTile )
		{
			//Dest tile doesn't change.
			return;
		}
		m_resetDest = true;
		m_destTile = _tile;
	}

	///Sets this character's destTile to the specified Tile with the spcified furniture.
	public void SetDestWithFurn ( Tile _characterDestTile, Furniture _furn )
	{
		if ( _characterDestTile.m_movementCost == 0 )
		{
			Debug.LogError("Moving the furniture to (" + _characterDestTile.X + ", " + _characterDestTile.Y + ") means the character will need to go to an invalid tile");
			return;
		}
		m_movingFurniture = _furn;
		SetDestination ( _characterDestTile );
	}

	/// This function will recursively check for valid tiles. It will keep calling itself as long as the character needs to walk through the furniture it is moving
	/// to get to its final destination. If this returns null, it means that if you continue moving the furniture to the given spot, the character will need to move it again
	/// and will continue to try to move it to new spots until the character blocks themselves in. If it doesn't return null, it will return the valid spot that can be used
	/// with the knowledge the character will not trap themselves.

	///Recursively finds out if the given Tiles are valid, based upon the given invalidTiles
	Tile FindValidPositionToMoveFurn ( Tile _charTile, Tile _furnTile, List<Tile> _invalidTiles )
	{
		//Flood fill, finding the nearest free tile.
		FindNearestFreeTile m_nearestFreeTileTEMP = new FindNearestFreeTile ( m_world, _furnTile, _invalidTiles.ToArray () );
		Tile m_moveFurnToTEMP = m_nearestFreeTileTEMP.m_tileFound;

		//Find a path from our current tile, to the found valid tile.
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

	///Returns true if _furnTile is in the path between _charTile and destTile
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

	/// Begins the process of moving the specified furniture to the specified tile.
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

	/// Returns a direction based upon the relation between the two tiles specified. Tiles must be adjacent.
	Direction CalculateDirectionOfMovement ( Tile _initialTile, Tile _nextTile )
	{

		if ( _initialTile.IsNeighbour ( _nextTile ) == false )
		{
			Debug.Log("The given tiles were not adjacent. _initalTile: (" + _initialTile.X + ", " + _initialTile.Y + "), _nextTile: (" + _nextTile.X + ", " + _nextTile.Y) ;
			return Direction.NONE;
		}

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

	/// Returns outcome of attempt. Attempts to take any stock from the specified furniture.
	protected bool TryTakeAnyStock ( Furniture _furn, bool _ignoreScanned = false, bool _ignoreWorked = false )
	{
		foreach ( var stockList in _furn.m_stock )
		{
			foreach ( Stock stock in _furn.m_stock[stockList.Key] )
			{
				if ( _ignoreScanned )
				{
					if ( stock.m_scanned )
					{
						continue;
					}
				}
				if ( _ignoreWorked )
				{
					if ( stock.m_triedGoingOut )
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
					return true;
				}
			}
		}

		return false;
	}

	/// Returns the outcome of attempt. Attempts to take a specified instance of stock from a specified furniture.
	protected bool TryTakeStock ( Stock _stock, Furniture _furn )
	{
		if ( m_basket == false )
		{
			if ( m_stock != null )
			{
				Debug.LogWarning ( "Tried to pick up stock but already carrying stock: Character: " + m_name + ", AttemptedStock: " + _stock.IDName );
				return false;
			}
		}

		if ( _furn.TryGiveStock ( _stock.IDName ) )
		{
			if ( m_basket )
			{
				m_basketContents.Add(_stock);
			}
			else
			{
				m_stock = _stock;
			}

			m_movingStock = true;
			m_fullTimeToMoveStock = 1f + ((float)_stock.Weight/1000f);

			return true;
		}

		return false;
	}

	/// Returns the stock the character is currently carrying, if it matches the specified IDName.
	protected Stock TryGiveStock ( string _stockIDName )
	{
		Stock ourStock;

		if ( m_basket )
		{
			foreach ( Stock s in m_basketContents.ToArray() )
			{
				if ( s.IDName == _stockIDName )
				{
					ourStock = s;
					break;
				}
			}
			//If we get here, we didn't find any stock in our basket that is required to be given.
			Debug.LogWarning ( "Character tried to give stock they don't possess: Character: " + m_name + ", Stock: " + _stockIDName );
			return null;
		}
		else
		{
			if ( m_stock.IDName != _stockIDName )
			{
				Debug.LogWarning ( "Character tried to give stock they don't possess: Character: " + m_name + ", Stock: " + _stockIDName );
				return null;
			}
			else
			{
				ourStock = m_stock;
			}
		}

		Stock stock = ourStock;

		m_movingStock = true;
		m_fullTimeToMoveStock = 1f + ( (float)stock.Weight / 1000f );

		if ( m_basket == false )
		{
			m_stock = null;
		}
		else
		{
			for ( int i = 0; i < m_basketContents.Count; i++ )
			{
				if ( ourStock == m_basketContents [ i ] )
				{
					m_basketContents.RemoveAt(i);
				}
			}
		}

		return stock;
	}

	/// Registers the OnChanged Callback
	public void RegisterOnChangedCallback( Action<Character> _callbackFunc )
	{
		cbCharacterChanged += _callbackFunc;
	}

	/// Unregisters the OnChanged Callback
	public void UnregisterOnChangedCallback( Action<Character> _callbackFunc )
	{
		cbCharacterChanged -= _callbackFunc;
	}
}
