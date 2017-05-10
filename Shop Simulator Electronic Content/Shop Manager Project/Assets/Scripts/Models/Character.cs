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

	public struct Relationship
	{
		/// Reference to the charcater that is linked to this relationship instance.
		public Character character;

		//This is the level of relationship between this charcater and the ID's character.
		//The higher this value, the better friends the two characters are.
		public int relationshipLevel;

		//This is used to determine if this character has already had a conversation with the character recently.
		public bool talkedRecently;

		public Relationship(Character _character, int _relationshipLevel, bool _talkedRecently)
		{
			character = _character;
			relationshipLevel = _relationshipLevel;
			talkedRecently = _talkedRecently;
		}
	}

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

	/// The unique ID of this charcater.
	public int ID { get; protected set; }

	/// The age of this charcater.
	public int m_age { get; protected set; }

	/// Has the character got a basket. If they do, they can carry more than on piece of stock and thier max carry weight increases.
	public bool m_basket;

	///Flag to determine if we have scanned items in our basket.
	public bool m_scannedItemsInBasket;

	/// String that represent's the charcater's full name.
	public string m_name { get; protected set; }

	public static List<string> Names { get; protected set; }

	static Character ()
	{
		Names = new List<string>();
		Names.Add("James");
		Names.Add("John");
		Names.Add("Robert");
		Names.Add("Richard");
		Names.Add("Emily");
		Names.Add("Jo");
		Names.Add("Edward");
		Names.Add("Paul");
		Names.Add("Alice");
		Names.Add("Lawrence");
		Names.Add("Michael");
		Names.Add("Emma");
		Names.Add("Sarah");
		Names.Add("Sophie");
		Names.Add("Ken");
		Names.Add("Jack");
		Names.Add("Karen");
		Names.Add("Abbi");
		Names.Add("Nathan");
		Names.Add("Sam");
		Names.Add("Harry");
		Names.Add("Frank");
		Names.Add("Jennifer");
		Names.Add("Linda");
		Names.Add("Kerry");
		Names.Add("Chloe");
		Names.Add("Brogan");
		Names.Add("Jessica");
		Names.Add("Janet");
		Names.Add("Heather");
		Names.Add("Amy");
		Names.Add("Molly");
		Names.Add("Ruth");
		Names.Add("Tracy");
		Names.Add("Kim");
	}

	/// Reference to the tile the character is currently in.
	public Tile m_currTile {get; protected set;}

	///Reference to the final Tile in the pathfinding sequence.
	protected Tile m_destTile;

	/// Reference to the next tile in the pathfinding sequence.
	protected Tile m_nextTile;

	/// The character's curr tile must be the same as this tasktile in order for the character to be able to continue with thier task/job.
	protected Tile m_taskTile;

	/// Flag to determine if this character is moving.
	public bool m_moving { get; protected set; }

	/// Flag to determine if the destination has changed.
	bool m_resetDest;

	/// Reference to the current AStar path.
	protected Path_AStar m_pathAStar;

	/// Flag to determine if this charcater will ignore other charcaters when pathfinding.
	bool m_ignoreCharactersInPath = true;

	/// Flag to determine if this charcater has tried to find a path around a character.
	bool m_triedToMoveAroundCharacter;

	/// Reference to the tile that the furniture this character is pushing is finally going to.
	protected Tile m_movingFurnsFinalTile;

	/// Flag to determine if this character is in the queue.
	protected bool m_inQueue;

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

	/// The full amount of time we will wait for someone to move before asking them to move.
	float m_fullWaitingTime = 5.0f;

	/// The amount of time we have waited so far for someone to move.
	float m_elapsedWaitingTime;

	/// Flag to determine if this character is currently speaking.
	public bool m_speaking { get; protected set; }

	/// Flag to determine if we are meant to be talking. Used to determine if were just interrupted.
	public bool m_weAreMeantToBeSpeaking { get; protected set; }

	/// This is the current amount of time that has passed since we started speaking.
	float m_elapsedSpeakingTime;

	/// This is the amount of time since we started speaking until we will stop speaking.
	float m_fullSpeakingTime;

	/// Flag to determine if we are currently waiting for a charcater to move.
	public bool m_waitingForCharacterToMoveTimed { get; protected set; }

	/// Flag to determine if this character is waiting for a charcater to move.
	bool m_waitingForCharacterToMoveUnTimed;

	/// Flag to determine if this charcater is waiting for a piece of furniture to move.
	bool m_waitingForFurnitureToMoveUnTimed;

	/// Used when interacting with other character to determine who is in charge of the interaction. The lower the number, the more authority.
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

	/// These are the different traits that charcaters can have. These affect thier relationships. And thier work ethics.

	public enum PositivePersonalityTraits
	{
		Alert,
		Calm,
		Cheerful,
		Clean,
		Clever,
		Friendly,
		Helpful,
		Kind,
		Quiet,
		Understanding
	}

	public enum NegativePersonalityTraits
	{
		Abrasive,
		Annoying,
		Boring,
		Difficult,
		Dramatic,
		Greedy,
		Ignorant,
		Jealous,
		Lazy,
		Messy
	}

	public enum PositivePhysicalTraits
	{
		Athletic,
		Strong,
		Quick
	}

	public enum NegativePhysicalTraits
	{
		Clumsy,
		Slow
	}

	public List<PositivePersonalityTraits> m_positivePersonTraits;
	public List<NegativePersonalityTraits> m_negativePersonTraits;
	public List<PositivePhysicalTraits> m_positivePhysTraits;
	public List<NegativePhysicalTraits> m_negativePhysTraits;

	public enum InteractionType
	{
		General,
		AskToMove,
		AskForJob,
		SaidHello,
		TalkingAtCheckout
	}

	/// The type of conversation this characater is having.
	InteractionType m_typeOfConversation;

	/// Flag to determine if this charcater is finished speaking during a interaction.
	bool m_finishedTopic;

	///Direction that the furniture is next moving in.
	Direction m_directionOfTravelWithFurn;

	///Nearest Tile that the furniture this character is moving can be at, without it obstructing us to our destination.
	protected Tile m_nearestFreeTile;

	/// The percentage this character is along thier current Tile. 1 - 100%, 0 - 0%
	protected float m_movementPercentage;

	/// Flag that determines if we are pushing a furniture, as oppose to pulling it.
	bool m_pushingFurn;

	/// Reference to the Stock this charcater is currently holding.
	public Stock m_stock { get; protected set; }

	/// List of the stock this character has in their basket.
	public List<Stock> m_basketContents { get; protected set; }

	/// Flag that determines if this character should ignore thier current pathfinding sequence for thier job. For example when moving a furniture out of the way.
	protected bool m_ignoreTask;

	/// The maximum speed of this character. Tiles Per Second.
	protected float m_maxSpeed = 2.0f; 

	/// The current speed of this character. Different from m_maxSpeed when moving furniture for example. Tiles Per Second.
	public float m_currentSpeed;

	/// If the charcater needs to do something urgancy or needs to be somewhere else, they will be in a rush.
	public bool m_inARush;

	/// Reference to the Furniture this charcater is currently moving.
	public Furniture m_movingFurniture { get; protected set; }

	/// Callback for when this character changes.
	Action<Character> cbCharacterChanged;

	/// Dictionary of all of this character's relationships. Look up by ID of character talking to.
	public Dictionary<int, Relationship> m_relationships { get; protected set; }

	/// The relationship level this character has with the shop.
	int m_shopRelationshipLevel;

	/// Flag to determine if this character is currently in a transaction.
	protected bool inTransaction;

	/// Reference to the character that this character is interacting with, if any.
	public Character m_interactingCharacter { get; protected set; }

	/// Flag to determine if this character will accept interaction from another character.
	public bool m_canInteract { get; protected set; }

	/// Flag to determine if this character can move from their curr tile.
	public bool m_canMove { get; protected set; }

	/// Flag to determine if this charcater has been asked to move.
	protected bool m_beenAskedToMove;

	public Character ()
	{

	}

	/// Spawns a character with the specified name, specified maxCarryWeight, on the specified Tile.
	public Character ( string _name, int _age, Tile _tile = null )
	{
		m_world = WorldController.instance.m_world;

		//When a new charcater is created. Its ID will be the next ID avaiable, which is just the number of IDs + 1
		ID = m_world.m_allCharacters.Count + 1;
	
		m_world.m_allCharacters.Add ( ID, this );

		m_currTile = m_destTile = m_nextTile = _tile; //Set all three tile variables to tile the character will spawn on.
		m_taskTile = null;
		m_currentSpeed = m_maxSpeed;

		m_basketContents = new List<Stock> ();
		m_relationships = new Dictionary<int, Relationship> ();
		m_positivePersonTraits = new List<PositivePersonalityTraits> ();
		m_negativePersonTraits = new List<NegativePersonalityTraits> ();
		m_positivePhysTraits = new List<PositivePhysicalTraits> ();
		m_negativePhysTraits = new List<NegativePhysicalTraits> ();

		m_name = _name;
		m_age = _age;
		m_canInteract = true;

		if ( _tile != null )
		{
			_tile.ChangeCharacterInTile ( this );
		}

		//Randomly picks a number of traits from the number available.
		int numOfTraitsMax = UnityEngine.Random.Range ( 0, ( Enum.GetNames ( typeof(PositivePersonalityTraits) ).Length + Enum.GetNames ( typeof(NegativePersonalityTraits) ).Length + Enum.GetNames ( typeof(PositivePhysicalTraits) ).Length + Enum.GetNames ( typeof(NegativePhysicalTraits) ).Length + 1 ) );
		int numOfTraitsActual = 0;

		while ( numOfTraitsActual < numOfTraitsMax )
		{
			//Add traits randomly.
			int randNum = UnityEngine.Random.Range ( 1, ( Enum.GetNames ( typeof(PositivePersonalityTraits) ).Length + Enum.GetNames ( typeof(NegativePersonalityTraits) ).Length + Enum.GetNames ( typeof(PositivePhysicalTraits) ).Length + Enum.GetNames ( typeof(NegativePhysicalTraits) ).Length + 1 ) );
			switch (randNum)
			{
				case 1:
					m_world.m_allCharacters [ ID ].AddPositivePersonalityTrait ( PositivePersonalityTraits.Alert );
					break;
				case 2:
					m_world.m_allCharacters [ ID ].AddPositivePersonalityTrait ( PositivePersonalityTraits.Calm );
					break;
				case 3:
					m_world.m_allCharacters [ ID ].AddPositivePersonalityTrait ( PositivePersonalityTraits.Cheerful );
					break;
				case 4:
					m_world.m_allCharacters [ ID ].AddPositivePersonalityTrait ( PositivePersonalityTraits.Clean );
					break;
				case 5:
					m_world.m_allCharacters [ ID ].AddPositivePersonalityTrait ( PositivePersonalityTraits.Clever );
					break;
				case 6:
					m_world.m_allCharacters [ ID ].AddPositivePersonalityTrait ( PositivePersonalityTraits.Friendly );
					break;
				case 7:
					m_world.m_allCharacters [ ID ].AddPositivePersonalityTrait ( PositivePersonalityTraits.Helpful );
					break;
				case 8:
					m_world.m_allCharacters [ ID ].AddPositivePersonalityTrait ( PositivePersonalityTraits.Kind );
					break;
				case 9:
					m_world.m_allCharacters [ ID ].AddPositivePersonalityTrait ( PositivePersonalityTraits.Quiet );
					break;
				case 10:
					m_world.m_allCharacters [ ID ].AddPositivePersonalityTrait ( PositivePersonalityTraits.Understanding );
					break;
				case 11:
					m_world.m_allCharacters [ ID ].AddNegativePersonalityTrait ( NegativePersonalityTraits.Abrasive );
					break;
				case 12:
					m_world.m_allCharacters [ ID ].AddNegativePersonalityTrait ( NegativePersonalityTraits.Annoying );
					break;
				case 13:
					m_world.m_allCharacters [ ID ].AddNegativePersonalityTrait ( NegativePersonalityTraits.Boring );
					break;
				case 14:
					m_world.m_allCharacters [ ID ].AddNegativePersonalityTrait ( NegativePersonalityTraits.Difficult );
					break;
				case 15:
					m_world.m_allCharacters [ ID ].AddNegativePersonalityTrait ( NegativePersonalityTraits.Dramatic );
					break;
				case 16:
					m_world.m_allCharacters [ ID ].AddNegativePersonalityTrait ( NegativePersonalityTraits.Greedy );
					break;
				case 17:
					m_world.m_allCharacters [ ID ].AddNegativePersonalityTrait ( NegativePersonalityTraits.Ignorant );
					break;
				case 18:
					m_world.m_allCharacters [ ID ].AddNegativePersonalityTrait ( NegativePersonalityTraits.Jealous );
					break;
				case 19:
					m_world.m_allCharacters [ ID ].AddNegativePersonalityTrait ( NegativePersonalityTraits.Lazy );
					break;
				case 20:
					m_world.m_allCharacters [ ID ].AddNegativePersonalityTrait ( NegativePersonalityTraits.Messy );
					break;
				case 21:
					m_world.m_allCharacters [ ID ].AddPositivePhysicalTrait ( PositivePhysicalTraits.Athletic );
					break;
				case 22:
					m_world.m_allCharacters [ ID ].AddPositivePhysicalTrait ( PositivePhysicalTraits.Strong );
					break;
				case 23:
					m_world.m_allCharacters [ ID ].AddPositivePhysicalTrait ( PositivePhysicalTraits.Quick );
					break;
				case 24:
					m_world.m_allCharacters [ ID ].AddNegativePhysicalTrait ( NegativePhysicalTraits.Clumsy );
					break;
				case 25:
					m_world.m_allCharacters [ ID ].AddNegativePhysicalTrait ( NegativePhysicalTraits.Slow );
					break;
			}
			numOfTraitsActual++;
		}

		//This will allow all characters when spawned to have a random chance of having a relationship with another character that
		//has already been created.
		for ( int i = 0; i < UnityEngine.Random.Range(0, m_world.m_allCharacters.Count); i++ )
		{
			int randomNumber = UnityEngine.Random.Range(1, m_world.m_allCharacters.Count);
			CheckRelationship(m_world.m_allCharacters[randomNumber]);
		}

	}

	/// Sets all required variable to their default values.
	public void RemoveFromWorld ()
	{
		m_basketContents.Clear();
		m_pathAStar = null;
		m_scannedItemsInBasket = false;
		m_taskTile = null;
		m_moving = false;
		m_triedToMoveAroundCharacter = false;
		m_movingFurnsFinalTile = null;
		m_inQueue = false;
		m_requiredFurn = null;
		m_movingStock = false;
		m_nearestFreeTile = null;
		m_stock = null;
		m_movingFurniture = null;
		inTransaction = false;
		m_interactingCharacter = null;
		m_beenAskedToMove = false;
		m_currTile.ChangeCharacterInTile(null);
	}

	/// Returns the outcome. Tries to add the specified trait to this characater.
	public bool AddPositivePersonalityTrait ( PositivePersonalityTraits _trait )
	{
		if ( m_positivePersonTraits.Contains ( _trait ) == false )
		{

			bool addTrait = false;
			switch ( _trait )
			{
				case PositivePersonalityTraits.Cheerful:
					if ( m_negativePersonTraits.Contains ( NegativePersonalityTraits.Boring ) )
					{
						Debug.Log ( "Cannot be Cheerful and Boring. Remove Boring as a trait to add Cheerful as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				case PositivePersonalityTraits.Clean:
					if ( m_negativePersonTraits.Contains ( NegativePersonalityTraits.Messy ) )
					{
						Debug.Log ( "Cannot be Clean and Messy. Remove Messy as a trait to add Clean as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				case PositivePersonalityTraits.Clever:
					if ( m_negativePersonTraits.Contains ( NegativePersonalityTraits.Ignorant ) )
					{
						Debug.Log ( "Cannot be Clever and Ignorant. Remove Ignorant as a trait to add Clever as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				case PositivePersonalityTraits.Helpful:
					if ( m_negativePersonTraits.Contains ( NegativePersonalityTraits.Difficult ) )
					{
						Debug.Log ( "Cannot be Helpful and Difficult. Remove Difficult as a trait to add Helpful as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				case PositivePersonalityTraits.Quiet:
					if ( m_negativePersonTraits.Contains ( NegativePersonalityTraits.Dramatic ) )
					{
						Debug.Log ( "Cannot be Quiet and Dramatic. Remove Dramatic as a trait to add Quiet as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				default:
					addTrait = true;
					break;
			}

			if ( addTrait )
			{
				m_positivePersonTraits.Add ( _trait );
			}
			else
			{
				return false;
			}
		}			
		return true;
	}

	/// Returns the outcome. Tries to add the specified trait to this characater.
	public bool AddNegativePersonalityTrait ( NegativePersonalityTraits _trait )
	{
		if ( m_negativePersonTraits.Contains ( _trait ) == false )
		{
			bool addTrait = false;

			switch ( _trait )
			{
				case NegativePersonalityTraits.Boring:
					if ( m_positivePersonTraits.Contains ( PositivePersonalityTraits.Cheerful ) )
					{
						Debug.Log ( "Cannot be Boring and Cheerful. Remove Cheerful as a trait to add Boring as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				case NegativePersonalityTraits.Difficult:
					if ( m_positivePersonTraits.Contains ( PositivePersonalityTraits.Helpful ) )
					{
						Debug.Log ( "Cannot be Helpful and Difficult. Remove Helpful as a trait to add Difficult as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				case NegativePersonalityTraits.Dramatic:
					if ( m_positivePersonTraits.Contains ( PositivePersonalityTraits.Quiet ) )
					{
						Debug.Log ( "Cannot be Quiet and Dramatic. Remove Quiet as a trait to add Dramatic as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				case NegativePersonalityTraits.Ignorant:
					if ( m_positivePersonTraits.Contains ( PositivePersonalityTraits.Clever ) )
					{
						Debug.Log ( "Cannot be Clever and Ignorant. Remove Clever as a trait to add Ignorant as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				case NegativePersonalityTraits.Messy:
					if ( m_positivePersonTraits.Contains ( PositivePersonalityTraits.Clean ) )
					{
						Debug.Log ( "Cannot be Clean and Messy. Remove Clean as a trait to add Messy as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				default:
					addTrait = true;
					break;
			}

			if ( addTrait )
			{
				m_negativePersonTraits.Add ( _trait );
			}
			else
			{
				return false;
			}
		}
		return true;
	}

	/// Returns the outcome. Tries to add the specified trait to this characater.
	public bool AddPositivePhysicalTrait ( PositivePhysicalTraits _trait )
	{
		if ( m_positivePhysTraits.Contains ( _trait ) == false )
		{
			bool addTrait = false;
			switch ( _trait )
			{
				case PositivePhysicalTraits.Athletic:
					if ( m_negativePhysTraits.Contains ( NegativePhysicalTraits.Clumsy ) )
					{
						Debug.Log ( "Cannot be Athletic and Clumsy. Remove Clumsy as a trait to add Athletic as trait." );
						addTrait = false;
					}
					else if ( m_negativePhysTraits.Contains ( NegativePhysicalTraits.Slow ) )
					{
						Debug.Log ( "Cannot be Athletic and Slow. Remove Slow as a trait to add Athletic as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				case PositivePhysicalTraits.Quick:
					if ( m_negativePhysTraits.Contains ( NegativePhysicalTraits.Slow ) )
					{
						Debug.Log ( "Cannot be Quick and Slow. Remove Slow as a trait to add Quick as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				default:
					addTrait = true;
					break;
			}
			if ( addTrait )
			{
				m_positivePhysTraits.Add ( _trait );
				ModifyStatsFromPosTrait ( _trait );
			}
			else
			{
				return false;
			}
		}
		return true;
	}

	/// Returns the outcome. Tries to add the specified trait to this characater.
	public bool AddNegativePhysicalTrait ( NegativePhysicalTraits _trait )
	{
		if ( m_negativePhysTraits.Contains ( _trait ) == false )
		{
			bool addTrait = false;
			switch ( _trait )
			{
				case NegativePhysicalTraits.Clumsy:
					if ( m_positivePhysTraits.Contains ( PositivePhysicalTraits.Athletic ) )
					{
						Debug.Log ( "Cannot be Clumsy and Athletic. Remove Clumsy as a trait to add Athletic as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
				case NegativePhysicalTraits.Slow:
					if ( m_positivePhysTraits.Contains ( PositivePhysicalTraits.Athletic ) )
					{
						Debug.Log ( "Cannot be Slow and Athletic. Remove Slow as a trait to add Athletic as trait." );
						addTrait = false;
					}
					else if ( m_positivePhysTraits.Contains ( PositivePhysicalTraits.Quick ) )
					{
						Debug.Log ( "Cannot be Slow and Quick. Remove Slow as a trait to add Quick as trait." );
						addTrait = false;
					}
					else
					{
						addTrait = true;
					}
					break;
			}

			if ( addTrait )
			{
				m_negativePhysTraits.Add ( _trait );
				ModifyStatsFromNegTrait ( _trait );
			}
			else
			{
				return false;
			}
		}
		return true;
	}

	public void Update( float _deltaTime )
	{
		Update_DoMovement( _deltaTime );
		Update_DoThink( _deltaTime );
	}

	/// Modifies characater's stats if specifed trait changes it.
	void ModifyStatsFromPosTrait ( PositivePhysicalTraits _trait )
	{
		if ( _trait == PositivePhysicalTraits.Athletic )
		{
			m_maxSpeed *= 1.1f;
		}
		else if ( _trait == PositivePhysicalTraits.Quick )
		{
			m_maxSpeed *= 1.1f;
		}
		else if ( _trait == PositivePhysicalTraits.Strong )
		{

		}
	}

	/// Modifies characater's stats if specifed trait changes it.
	void ModifyStatsFromNegTrait ( NegativePhysicalTraits _trait )
	{
		if ( _trait == NegativePhysicalTraits.Clumsy )
		{
			//Makes more mess when picking up items.
		}
		else if ( _trait == NegativePhysicalTraits.Slow )
		{
			m_maxSpeed *= 0.9f;
		}
	}

	/// Sets this character's movement, and furniture variables to their default. m_pathAStar, m_movingFurniture, m_nearestFreeTile, m_currentSpeed.
	protected void ResetDefaultVariables()
	{
		m_pathAStar = null;
		m_currentSpeed = m_maxSpeed;
	}

	void Update_DoMovement ( float _deltaTime )
	{
		if ( m_canMove == false )
		{
			//Character cannot move from this location.
			return;
		}

		if ( m_currTile == m_destTile || m_resetDest )
		{
			//We are already where we need to be, so reset all defaults, and return from function.
			ResetDefaultVariables ();
			m_resetDest = false;
			return;
		}

		if ( m_nextTile == null || m_nextTile == m_currTile || m_pathAStar == null )
		{
			//We don't have the next tile, it is the same as our current tile, or we have no path at all.
			if ( m_pathAStar == null || m_pathAStar.Length () == 0 )
			{
				//We have no path, or our path's length is 0, so get a new path.
				if ( m_destTile == null )
				{
					SetDestination ( m_currTile );
					return;
				}	
				m_pathAStar = new Path_AStar ( m_world, m_currTile, m_destTile, m_ignoreCharactersInPath );
				if ( m_pathAStar.Length () == 0 )
				{
					if ( m_triedToMoveAroundCharacter == false )
					{
						//If we just found a new path, and its length is 0, clearly we cannot have a valid path to the destination, assumming the destTile was different from the currTile.
						Debug.LogError ( "Path_AStar returned no path to destination!" );
						m_pathAStar = null;
						return;
					}
					else
					{
						//We tried to find a way around the charcater but couldn't, so ask them to move.

						//We need to ask the character in our way to move.
						if ( this.GetType () == new Employee ().GetType () && m_nextTile.m_character.GetType () == new Customer ().GetType () )
						{
							return;
						}
						else
						{
							if ( m_beenAskedToMove == false && m_nextTile.m_character.m_moving == false )
							{
								Debug.Log ( "I asked someone to move" );
								Tile freeTile = new FindNearestFreeTile ( m_world, m_nextTile, m_pathAStar.CurrPathToArray (), true ).m_tileFound;
								RequestInteraction ( m_nextTile.m_character, 2, InteractionType.AskToMove, freeTile );
							}
							return;
						}
					}
				}
				else
				{
					if ( m_triedToMoveAroundCharacter )
					{
						//If we tried moving around the character, and found a good path. We must have succeeded, so change the flag.
						m_triedToMoveAroundCharacter = false;
					}
				}
			}

			//Set our nextTile to the next tile in the path, and remove it from the path's queue.
			m_nextTile = m_pathAStar.Dequeue ();

			//If we are an employee, we need to make sure a customer is not walking on this tile, or is going to be soon.
			if ( GetType () == new Employee ().GetType () )
			{
				foreach ( Tile t in m_nextTile.GetNeighbours(true) )
				{
					if ( t.m_character != null )
					{
						if ( t.m_character.GetType () == new Customer ().GetType () )
						{
							if ( t.m_character.AreYouWalkingHere ( m_nextTile ) )
							{
								//A customer is on, or going to walk onto our next tile. So wait.
								m_waitingForCharacterToMoveUnTimed = true;
								Debug.LogError ( "Waiting for customer to move." );
								return;
							}
						}
					}
				}
				//If we get here, our next tile isn't going to be moved into.
				m_waitingForCharacterToMoveUnTimed = false;
			}

			if ( m_nextTile == m_currTile )
			{
				//We just got the next tile, so if its the same as our current tile, something must have gone wrong.
				Debug.LogError ( "Update_DoMovement - nextTile is currTile?" );
			}
		}

		// At this point we should have a valid nextTile to move to.

		if ( m_waitingForCharacterToMoveUnTimed )
		{
			//If we are an employee, we need to make sure a customer is not walking on this tile, or is going to be soon.
			if ( GetType () == new Employee ().GetType () )
			{
				foreach ( Tile t in m_nextTile.GetNeighbours(true) )
				{
					if ( t.m_character != null )
					{
						if ( t.m_character.GetType () == new Customer ().GetType () )
						{
							if ( t.m_character.AreYouWalkingHere ( m_nextTile ) )
							{
								//A customer is on, or going to walk onto our next tile. So wait.
								m_waitingForCharacterToMoveUnTimed = true;
								Debug.LogError ( "STILL waiting for customer to move." );
								return;
							}
						}
					}
				}
				//If we get here, our next tile isn't going to be moved into.
				m_waitingForCharacterToMoveUnTimed = false;
			}
		}

		//Calculates the distance from point A to point B
		float distToTravel = Mathf.Sqrt (
			                     Mathf.Pow ( m_currTile.X - m_nextTile.X, 2 ) +
			                     Mathf.Pow ( m_currTile.Y - m_nextTile.Y, 2 )
		                     );

		if ( m_nextTile.IsEnterable () == ENTERABILITY.Never )
		{	
			m_moving = false;
			//The next tile cannot be entered, so we need a new path. setting m_pathAStar to null will cause the next frame to give us a new path to our destTile.
			m_nextTile = null;
			m_pathAStar = null;
			return;
		}
		else if ( m_nextTile.IsEnterable () == ENTERABILITY.Soon )
		{
			m_moving = false;
			//We can't enter the tile now, but should be able to in the future, like a door, or another character.
			if ( m_nextTile.m_character != null )
			{
				if ( m_nextTile.m_character.m_waitingForCharacterToMoveTimed || m_nextTile.m_character.m_interactingCharacter != null )
				{
					m_elapsedWaitingTime = m_fullWaitingTime;
				}

				m_waitingForCharacterToMoveTimed = true;

				if ( m_elapsedWaitingTime >= m_fullWaitingTime )
				{
					//We have waited enough time for the charcater to move.
					m_waitingForCharacterToMoveTimed = false;
					if ( m_triedToMoveAroundCharacter == false )
					{
						Debug.Log ( "Finding a way around this character that is blocking me." );
						m_ignoreCharactersInPath = false;
						m_resetDest = true;
						m_triedToMoveAroundCharacter = true;
						m_waitingForCharacterToMoveTimed = false;
					}
					else
					{
						if ( m_beenAskedToMove == false && m_nextTile.m_character.m_moving == false )
						{
							Debug.Log ( "I asked someone to move" );
							Tile freeTile = new FindNearestFreeTile ( m_world, m_nextTile, m_pathAStar.CurrPathToArray (), true ).m_tileFound;
							RequestInteraction ( m_nextTile.m_character, 2, InteractionType.AskToMove, freeTile );
						}
						return;
					}
				}
				else
				{
					//We have not waited enough time for the charcater to move.
					return;
				}
			}
			return;
		}
		else
		{
			m_moving = true;
			m_waitingForCharacterToMoveTimed = false;
			m_ignoreCharactersInPath = true;
			//The tile is enterable.
			if ( ( m_nextTile.m_furniture != null && m_nextTile.m_furniture.m_movable == true ) || m_movingFurniture != null )
			{
				//This tile is enterable, but has furniture on it
				//However, the furniture can be moved, so move it to the nearest tile that is not on the path to our destination.
				if ( m_nextTile != m_currTile && ( m_requiredFurn == null || m_nextTile != m_requiredFurn.m_mainTile ) && ( m_requiredFurn == null || m_currTile != m_requiredFurn.m_mainTile ) && m_movingFurniture == null )
				{
					if ( m_nextTile.m_furniture.m_moving )
					{
						//The furniture is moving, we should wait in case it moves.
						m_waitingForFurnitureToMoveUnTimed = true;
						return;
					}
					else
					{
						m_waitingForFurnitureToMoveUnTimed = false;
					}

					if ( m_waitingForFurnitureToMoveUnTimed )
					{
						//We are waiting for the furniture to move.
						return;
					}
					if ( m_nearestFreeTile == null )
					{
						bool foundValidTile = false;

						List<Tile> invalidTiles = new List<Tile> ();

						if ( m_pathAStar.CurrPathToArray () != null )
						{
							foreach ( Tile t in m_pathAStar.CurrPathToArray () )
							{
								invalidTiles.Add ( t );
							}
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
							//The furniture we are moving is not the one we need for our task, so ignore the task so that we can move this furniture to the free tile.
							m_ignoreTask = true;
						}
						if ( MoveFurnitureToTile ( m_nextTile.m_furniture, m_nearestFreeTile ) == false )
						{
							Debug.LogWarning ( "Tried to move a piece of furniture somewhere but it returned false. This might mean we are not next to the piece of furniture." );
							return;
						}
						else
						{
							m_movingFurnsFinalTile = m_nearestFreeTile;
						}
					}
				}
				else
				{

					//If we get here, the next tile doesn't have furniture in it.
					if ( this.GetType () == new Customer ().GetType () )
					{
						//Customers don't need to move any furniture because its not in thier way.
						m_nearestFreeTile = null;
						m_movingFurniture = null;
						m_movingFurnsFinalTile = null;
					}
				}

				if ( m_movingFurniture != null && m_movingFurniture.m_mainTile == m_movingFurnsFinalTile )
				{
					m_nearestFreeTile = null;
				}

				//Now we know where we need to be, and where the furniture needs to be.
				Tile toTile = null;	
				if ( m_pushingFurn == true )
				{
					if ( m_currTile == m_nextTile )
					{
						toTile = m_movingFurniture.m_mainTile;
					}
					else if ( m_pathAStar.Length () > 0 && m_pathAStar.CurrPathToArray () [ 0 ] != null )
					{
						toTile = m_pathAStar.CurrPathToArray () [ 0 ];
					}
					else
					{
						if ( m_movingFurnsFinalTile != null )
						{
							toTile = m_movingFurnsFinalTile;
						}
						else
						{
							bool foundValidTile = false;
							List<Tile> invalidTiles = new List<Tile> ();
							invalidTiles.Add ( m_currTile );
							while ( foundValidTile == false )
							{
								//Keep finding new tiles to move to, and keep working out if they are valid.
							
								toTile = FindValidPositionToMoveFurn ( m_currTile, m_nextTile, invalidTiles );
								Tile tryTile = new FindNearestFreeTile ( m_world, m_nextTile, invalidTiles.ToArray () ).m_tileFound;
								if ( toTile == tryTile )
								{
									foundValidTile = true;
								}
								else
								{
									invalidTiles.Add ( tryTile );
								}
							}
						}
					}
				}
				else if ( m_pushingFurn == false )
				{
					toTile = m_currTile;
				}
				if ( toTile == null )
				{
					Debug.LogError ( "We are not pushing or pulling a furniture." );
					foreach ( Tile t in m_currTile.GetNeighbours() )
					{
						if ( t.m_character == null && t.m_furniture == null )
						{
							toTile = t;
							return;
						}
					}
				}

				if ( toTile.m_character != null && toTile.m_character != this )
				{
					Debug.LogError ( "We are pushing our trolley into someone!!" );
					return;
				}

				if ( toTile.m_character == null || ( toTile.m_character != null && toTile.m_character == this ) || toTile.m_furniture == null )
				{

					if ( m_movingFurniture != null && m_movingFurniture.m_moving == false && m_movingFurniture.MoveFurniture ( toTile ) == false )
					{
						Debug.LogError ( "Failed to push furniture: " + m_nextTile.m_furniture.m_name );
						m_pathAStar = null;
					}
					else
					{
						if ( m_movingFurniture == null )
						{
							Debug.LogError ( "Moving furniture is null." );
						}
						else
						{
							m_currentSpeed = m_maxSpeed / m_movingFurniture.m_movementCost;
							m_movingFurniture.m_furnParameters [ "m_speed" ] = m_currentSpeed;

							m_movingFurniture.m_moving = true;
						}
					}
				}
			}
			else
			{
				m_waitingForFurnitureToMoveUnTimed = false;
			}
		}

		// Work out how much we will be moving this frame.
		float distThisFrame = m_currentSpeed / m_nextTile.m_movementCost * _deltaTime;

		// Work out how much we will be moving this frame as a percentage.
		float percThisFrame = distThisFrame / distToTravel;

		// Add that percentage to our already travelled percentage.
		m_movementPercentage += percThisFrame;

		if ( m_movingFurniture != null )
		{
			m_movingFurniture.m_furnParameters [ "percThisFrame" ] = percThisFrame;
		}

		if ( m_movementPercentage >= 1 )
		{

			//If our percentage is 100% or above, we have reached the next tile, so update currTile, and reset the movement percentage.
			m_currTile.ChangeCharacterInTile ( null );
			m_currTile = m_nextTile;
			m_currTile.ChangeCharacterInTile ( this );
			m_movementPercentage = 0;
			if ( m_currTile == m_destTile )
			{
				m_moving = false;
			}
		}

		if ( m_movingFurnsFinalTile != null && m_movingFurniture != null && m_movingFurnsFinalTile == m_movingFurniture.m_mainTile )
		{
			//The furniture was going to a free tile, and we are where we needed to go. So stop pushing it.
			m_movingFurniture.m_moving = false;
			m_movingFurniture = null;
			m_movingFurnsFinalTile = null;
			m_nearestFreeTile = null;
			ResetDefaultVariables ();
		}

		//Update this character's visuals.
		if ( cbCharacterChanged != null )
		{
			cbCharacterChanged ( this );
		}
	}

	///Used as a placeholder for the employee and customer Update_DoThink function.
	protected virtual void Update_DoThink ( float _deltaTime )
	{
		if ( m_waitingForCharacterToMoveTimed )
		{
			m_elapsedWaitingTime += _deltaTime;	
			return;
		}

		if ( m_interactingCharacter != null )
		{
			if ( m_walkAndTalk )
			{
				m_canMove = true;
			}
			else
			{
				m_canMove = false;
			}
			if ( m_typeOfConversation != InteractionType.TalkingAtCheckout && m_authorityLevel >= m_interactingCharacter.m_authorityLevel )
			{
				//The character we are interacting with is has a bigger authority than us. So keep up with them.
				if ( m_currTile.IsNeighbour ( m_interactingCharacter.m_currTile, true, true ) == false )
				{
					Path_AStar TempPath = new Path_AStar ( m_world, m_currTile, m_interactingCharacter.m_currTile );
					SetDestination ( TempPath.InitialPathToArray () [ TempPath.Length () - 2 ] );
				}	
			}

			if ( m_interactingCharacter.m_interactingCharacter != this )
			{
				//The character we are interacting with is not interacting with us anymore, so stop the conversation.
				m_elaspedTimeInteraction = m_fullTimeInteraction;
			}	

			m_elaspedTimeInteraction += _deltaTime;
			if ( m_elaspedTimeInteraction >= m_fullTimeInteraction )
			{
				m_canMove = true;
				m_ignoreTask = false;
				if ( m_interactingCharacter != null )
				{
					StopInteraction ();
				}
			}
			else
			{
				m_elapsedSpeakingTime += _deltaTime;
				Conversation ();
			}

			if ( m_typeOfConversation != InteractionType.TalkingAtCheckout )
			{
				return;
			}
		}
		else
		{
			m_canMove = true;
			if ( m_inQueue == false )
			{
				Character _char = CheckNeighboursforCharacters ();
				if ( _char != null )
				{
					//We found someone next to us. If we have a good enough relationship with them. Talk to them.
					if ( m_relationships.ContainsKey ( _char.ID ) && m_relationships [ _char.ID ].relationshipLevel >= 30 && m_relationships [ _char.ID ].talkedRecently == false )
					{
						SayHello ( _char );
					}
				}
			}
		}

		if ( m_currTile == m_destTile )
		{
			m_ignoreTask = false;
		}

		//If the character ignore its current task, it doesn't need to think about what to do next. It should just carry on moving to where its going.
		if ( m_ignoreTask == true )
		{
			return;
		}

		OnUpdate_DoThink ( _deltaTime );
	}

	protected virtual void OnUpdate_DoThink ( float _deltaTime )
	{
	}

	/// Returns a found character. Finds the first charcater in neighbouring tiles. Return can be null.
	Character CheckNeighboursforCharacters ()
	{
		foreach ( Tile t in m_currTile.GetNeighbours() )
		{
			if ( t != null )
			{
				if ( t.m_character != null )
				{
					return t.m_character;
				}
			}
		}

		return null;
	}

	/// Returns the outcome. Tests the specified tile with m_currTile and m_nextTile.
	public bool AreYouWalkingHere ( Tile _tile )
	{
		if ( m_currTile == _tile || m_nextTile == _tile )
		{
			return true;
		}

		return false;
	}

	/// Character says hello to the specified character.
	void SayHello ( Character _char )
	{
		if ( RequestInteraction ( _char, 1.0f, InteractionType.SaidHello ) )
		{
			ChangeRelationshipLevel(_char.ID, 1, true);

			//Choose to begin a conversation with this charcater.
			bool beginConversation = false;

			if ( m_relationships [ _char.ID ].talkedRecently == false )
			{
				if ( m_inARush == false )
				{
					if ( m_relationships [ _char.ID ].relationshipLevel >= 70 )
					{
						beginConversation = true;
					}
					else
					{
						//Always try to start a conversation if you have these traits.
						if ( m_positivePersonTraits.Contains ( PositivePersonalityTraits.Friendly )
						     || m_positivePersonTraits.Contains ( PositivePersonalityTraits.Cheerful )
						     || m_positivePersonTraits.Contains ( PositivePersonalityTraits.Kind ) )
						{
							beginConversation = true;
						}
						else if ( m_negativePersonTraits.Contains ( NegativePersonalityTraits.Annoying ) )
						{
							beginConversation = true;
						}
					}
				}
			}

			if ( beginConversation )
			{
				if ( RequestInteraction ( _char, UnityEngine.Random.Range ( m_relationships[_char.ID].relationshipLevel, m_relationships[_char.ID].relationshipLevel*2 ) ) )
				{
					m_relationships[_char.ID] = new Relationship(_char, m_relationships[_char.ID].relationshipLevel, true);
				}
			}
			
		}
		else
		{
			ChangeRelationshipLevel(_char.ID, -1, true);
			Debug.Log ( m_name + " tried to say hello to " + _char.m_name );
		}
	}

	/// Returns whether this character can interact with the specified character.
	public bool ReceiveInteraction ( Character _char, float _time, InteractionType _interType = InteractionType.General, Tile _tile = null )
	{
		if ( m_canInteract || _interType == InteractionType.TalkingAtCheckout || _interType == InteractionType.AskForJob )
		{
			m_typeOfConversation = _interType;

			CheckRelationship ( _char );

			if ( m_authorityLevel >= _char.m_authorityLevel )
			{
				//Our authority level is the same or higher than the other character's, so we need to stop moving to our task.
				m_ignoreTask = true;
			}

			if ( _interType == InteractionType.SaidHello )
			{
				ChangeRelationshipLevel ( _char.ID, 1, true );
			}

			BeginInteraction ( _char, _time, false );

			if ( _interType == InteractionType.AskToMove )
			{
				m_beenAskedToMove = true;
				Debug.Log ( "I, " + m_name + ", have been asked to move." );
				if ( _tile != null )
				{
					SetDestination ( _tile );
					if ( m_movingFurniture != null )
					{
						m_movingFurniture.m_manned = false;
						m_movingFurniture.m_moving = false;
						m_movingFurniture = null;
					}
					}
					else
					{
						Debug.LogError("FIX THIS!");
					}
			}
			return true;

		}
		else
		{
			if ( _interType == InteractionType.AskToMove )
			{
				
				CheckRelationship ( _char );

				if ( m_authorityLevel >= _char.m_authorityLevel )
				{
					//Our authority level is the same or higher than the other character's, so we need to stop moving to our task.
					m_ignoreTask = true;
				}

				if ( m_interactingCharacter != null )
				{
					StopInteraction ( true );
				}
				BeginInteraction ( _char, _time, false );
				if ( m_beenAskedToMove == false )
				{
					m_beenAskedToMove = true;
					Debug.Log ( "I, " + ID + ", have been asked to move." );
					SetDestination ( new FindNearestFreeTile ( m_world, m_currTile, null ).m_tileFound );
				}
			}
			return false;
		}
	}

	/// Returns the outcome of requesting interaction with the specified character. Time is the time it takes to complete this interaction.
	public bool RequestInteraction ( Character _char, float _time, InteractionType _interType = InteractionType.General, Tile _tile = null )
	{
		m_typeOfConversation = _interType;

		if ( _interType != InteractionType.TalkingAtCheckout && m_currTile.IsNeighbour ( _char.m_currTile, true, true ) == false )
		{
			//The character's aren't adjacent.
			return false;
		}
		if ( _char.ReceiveInteraction ( this, _time, _interType, _tile ) )
		{
			CheckRelationship ( _char );

			if ( m_authorityLevel >= _char.m_authorityLevel )
			{
				//Our authority level is the same or higher than the other character's, so we need to stop moving.
				m_ignoreTask = true;
			}
			BeginInteraction(_char, _time, true);
			return true;
		}
		else
		{
			return false;
		}
	}

	/// If character does not have a relationship with specified character, one is created.
	void CheckRelationship ( Character _char )
	{
		bool newCharacter = true;

			foreach ( Relationship relationship in m_relationships.Values )
			{
				if ( _char.ID == relationship.character.ID )
				{
					newCharacter = false;
					break;
				}
			}

			if ( newCharacter )
			{
				m_relationships.Add ( _char.ID, new Relationship ( _char, UnityEngine.Random.Range(30, 70), false ) );
			}
	}

	/// Begins an interaction with specified character, with the specified length of time and a flag to determine if this character began the conversation.
	public void BeginInteraction ( Character _char, float _time, bool _initiatedConversation )
	{
		m_interactingCharacter = _char;
		m_canInteract = false;
		m_fullTimeInteraction = _time;
		m_elaspedTimeInteraction = 0;
		m_finishedTopic = false;
		m_relationships[_char.ID] = new Relationship(m_relationships[_char.ID].character, m_relationships[_char.ID].relationshipLevel, true);

		if ( _initiatedConversation )
		{
			BeginSpeaking();
		}
	}

	/// This character begins speaking.
	void BeginSpeaking ()
	{
		m_speaking = true;
		m_fullSpeakingTime = UnityEngine.Random.Range((m_fullTimeInteraction - m_elaspedTimeInteraction)/3, m_fullTimeInteraction - m_elaspedTimeInteraction);
		m_elapsedSpeakingTime = 0;
		m_weAreMeantToBeSpeaking = true;
	}

	/// This charcater finishes speaking.
	void FinishSpeaking()
	{
		m_weAreMeantToBeSpeaking = false;
		m_speaking = false;
	}

	/// This character carries on their conversation.
	public void Conversation ()
	{
		//The character we are interacting with.
		Character character = m_interactingCharacter;

		if ( m_interactingCharacter == null )
		{
			//We are no longer talking to the person we were. 
			return;
		}

		//The relationship with the character we are interacting with.
		Relationship relationship = m_relationships [ character.ID ];

		if ( character.m_speaking == true )
		{
			if ( m_weAreMeantToBeSpeaking )
			{
				//We just got interrupted.
				if ( relationship.relationshipLevel > 60 )
				{
					//We don't mind that we got interupted.
				}
				else
				{
					if ( m_negativePersonTraits.Contains ( NegativePersonalityTraits.Difficult )
					     || m_negativePersonTraits.Contains ( NegativePersonalityTraits.Dramatic ) )
					{
						//We are annoyed that we got interupted.
						ChangeRelationshipLevel ( character.ID, -5 );
						Debug.Log ( "We just lost some relationship with another character." );
					}

				}

				FinishSpeaking ();
			}
			else
			{
				//They are talking, so we can choose to interrupt.
				if ( ( m_negativePersonTraits.Contains ( NegativePersonalityTraits.Annoying )
				     || m_negativePersonTraits.Contains ( NegativePersonalityTraits.Abrasive ) 
				     ) && m_positivePersonTraits.Contains ( PositivePersonalityTraits.Quiet ) == false )
				{
					//We will decide if we are going to interrupt.
					int num = UnityEngine.Random.Range ( 0, 100 );

					if ( num <= 20 )
					{
						//20% of the time, we will interrupt
						BeginSpeaking ();
					}
				}
			}
		}
		else
		{
			//The other character isn't speaking, so we need to start speaking.
			if ( m_speaking == false )
			{
				BeginSpeaking ();
			}
		}

		if ( m_speaking )
		{
			//We are the character that is currently speaking.
			if ( m_elapsedSpeakingTime >= m_fullSpeakingTime )
			{
				//We have finished speaking.
				//We need to tell the other character whether we said something good or bad.
				m_finishedTopic = true;
				if ( m_typeOfConversation != InteractionType.AskToMove )
				{
					ResolveRelationshipLevel ( character );
				}
			}
		}
	}

	/// Stops the interaction. If we are initiating the end of the conversation. _finishConversation needs to be true (default).
	/// Sets interactingCharacter to null, and allows this character to interact again.
	public void StopInteraction ( bool _finishConversation = true )
	{
		//We need to tell the other character that we are no longer talking to them.
		if ( _finishConversation )
		{
			if ( m_interactingCharacter != null )
			{
				m_interactingCharacter.FriendHasSaidGoodbye ();
			}
		}

		if ( m_finishedTopic == false )
		{
			//We didn't get to finish the topic we were on, so resolve the relationship level change now.
			if ( m_typeOfConversation != InteractionType.AskToMove )
			{
				ResolveRelationshipLevel ( m_interactingCharacter );
			}
		}
		m_interactingCharacter = null;
		m_canInteract = true;
	}

	/// Resolves the relationship level with the specified character.
	void ResolveRelationshipLevel ( Character _char )
	{
		int positiveInfulence = 2;

		int negativeInfluence = 2;

		if ( m_relationships [ _char.ID ].relationshipLevel > 50 )
		{	
			positiveInfulence++;
		}

		if ( m_relationships [ _char.ID ].relationshipLevel > 70 )
		{
			positiveInfulence++;
		}

		if ( m_relationships [ _char.ID ].relationshipLevel > 90 )
		{
			positiveInfulence++;
		}

		if ( m_relationships [ _char.ID ].relationshipLevel < 30 )
		{
			negativeInfluence++;
		}

		if ( m_positivePersonTraits.Count > 0 )
		{
			positiveInfulence += UnityEngine.Random.Range ( m_positivePersonTraits.Count, m_positivePersonTraits.Count * 2 );
		}

		if ( m_negativePersonTraits.Count > 0 )
		{
			negativeInfluence += UnityEngine.Random.Range ( m_negativePersonTraits.Count, m_negativePersonTraits.Count * 2 );
		}

		//This is the 'influence value' of the topic you are discussing.
		//This simulates negative or positive topics.
		int topicLevel = UnityEngine.Random.Range ( -5, 5 );

		//This is the actual relationship change. It takes into account your starting relationship level, your traits,
		//and the topic you are discussing.
		int relationshipChange = UnityEngine.Random.Range ( -negativeInfluence + topicLevel, positiveInfulence + topicLevel );

		if ( this.GetType() == new Customer().GetType() && m_world.m_allCharacters [ ID ].GetType() == new Employee ().GetType() )
		{
			//We were speaking to an employee at the checkout, so cap thier change at -3.
			//they couldn't have possibly said anything THAT bad right?
			if ( relationshipChange < -3 )
			{
				relationshipChange = -3;
			}

			if ( relationshipChange < 0 )
			{
				//An employee was rude to us, so we need to lower the shop's relationship level.
				ResolveShopRelationshopLevel(relationshipChange);
			}
		}
		m_interactingCharacter.ChangeRelationshipLevel ( ID, relationshipChange );

		FinishSpeaking ();
	}

	/// Resolves the shop relationship level by the specified amount.
	protected void ResolveShopRelationshopLevel ( int _amount )
	{
		if ( m_shopRelationshipLevel > 60 )
		{
			//We have a ok relationship with this shop, the the negative impact is lessened.
			if ( _amount < 0 )
			{
				_amount++;
			}
		}

		if ( m_shopRelationshipLevel > 80 )
		{
			//We have a good relationship with this shop, the the negative impact is lessened.
			if ( _amount < 0 )
			{
				_amount++;
			}
		}

		if ( m_shopRelationshipLevel > 100 )
		{
			//We have a excellent relationship with this shop, we can almost never lose influence.
			if ( _amount < 0 )
			{
				_amount = 0;
			}
		}

		if ( m_shopRelationshipLevel < 30 )
		{
			//We have a bad relationship with this shop, so positive impacts are lessened.
			if ( _amount > 0 )
			{
				_amount--;
			}
		}

		if ( m_shopRelationshipLevel < 20 )
		{
			//We have a REALLY bad relationship with this shop, so positive impacts are almost useless.
			if ( _amount > 0 )
			{
				_amount /= 5;
			}
		}

		if ( m_positivePersonTraits.Contains ( PositivePersonalityTraits.Kind )
		     || m_positivePersonTraits.Contains ( PositivePersonalityTraits.Understanding ) )
		{
			_amount++;
		}

		if ( m_negativePersonTraits.Contains ( NegativePersonalityTraits.Difficult )
		     || m_negativePersonTraits.Contains ( NegativePersonalityTraits.Dramatic )
		     || m_negativePersonTraits.Contains ( NegativePersonalityTraits.Ignorant ) )
		{
			_amount--;
		}

		//As the relationship level increases, the impact of positive remarks is lessened.

		if ( m_shopRelationshipLevel > 80 && _amount > 0 )
		{
			_amount /= 2;
		}
		if ( m_shopRelationshipLevel > 100 && _amount > 0)
		{
			_amount /= 3;
		}

		//But as the relationship gets worse, the negative impacts also get worse.

		if ( m_shopRelationshipLevel < 30 && _amount < 0 )
		{
			_amount *= 2;
		} 
		if ( m_shopRelationshipLevel < 10 && _amount < 0 )
		{
			_amount *= 3;
		} 

		if ( _amount < 0 )
		{
			Debug.Log("Shop relations decreased");
		}

		m_shopRelationshipLevel += _amount;
	}

	/// Changes the relationship level with the character with the specified ID, but the specified amount. A flag is needed to determine if our traits are ignored.
	public void ChangeRelationshipLevel ( int _ID, int _amount, bool _ignoreTraits = false )
	{ 
		if ( m_typeOfConversation == InteractionType.AskToMove )
		{
			return;
		}
		if ( _ignoreTraits == false )
		{
			if ( m_positivePersonTraits.Contains ( PositivePersonalityTraits.Friendly )
			     || m_positivePersonTraits.Contains ( PositivePersonalityTraits.Cheerful )
			     || m_positivePersonTraits.Contains ( PositivePersonalityTraits.Understanding ) )
			{
				if ( m_positivePersonTraits.Contains ( PositivePersonalityTraits.Understanding ) )
				{
					_amount += 3;
				}
				else
				{
					_amount += 1;
				}
			}

			if ( m_negativePersonTraits.Contains ( NegativePersonalityTraits.Difficult )
			     || m_negativePersonTraits.Contains ( NegativePersonalityTraits.Ignorant )
			     || m_negativePersonTraits.Contains ( NegativePersonalityTraits.Jealous ) )
			{
				if ( m_negativePersonTraits.Contains ( NegativePersonalityTraits.Jealous ) )
				{
					_amount -= 3;
				}
				else
				{
					_amount -= 1;
				}
			}
		}

		m_relationships[_ID] = new Relationship(m_relationships[_ID].character, m_relationships[_ID].relationshipLevel + _amount, m_relationships[_ID].talkedRecently);
	}

	/// Character that was speaking has said goodbye.
	public void FriendHasSaidGoodbye ()
	{
		if ( m_interactingCharacter != null )
		{
			StopInteraction ( false );
		}
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
		m_moving = true;
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
			Tile tile = m_pathAStarTEMP.InitialPathToArray () [ m_pathAStarTEMP.InitialPathToArray ().Length - 1 ];
			if (m_pathAStarTEMP.InitialPathToArray ().Length == 1)
			{
				tile = m_pathAStarTEMP.InitialPathToArray () [ m_pathAStarTEMP.InitialPathToArray ().Length - 1 ];
			}
			else
			{
				tile = m_pathAStarTEMP.InitialPathToArray () [ m_pathAStarTEMP.InitialPathToArray ().Length - 2 ];
			}
			if ( IsThisTileInPath ( m_moveFurnToTEMP, tile ) )
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
		bool nextToFurn = false;
		if ( m_currTile.IsNeighbour ( _furn.m_mainTile, true, true ) )
		{
			//We are neighbours with the furniture.
			nextToFurn = true;
		}
		else
		{
			//We are not neighbours with the furniture.
			nextToFurn = false;
		}

		if ( nextToFurn == false )
		{
			if ( m_destTile.IsNeighbour ( _furn.m_mainTile, true, true ) )
			{
				//We are on our way to the furniture, so we don't need to do anything.
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
				//We are already where we need to be, we should have already known this.
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

		if ( m_pathAStar.InitialPathToArray ().Length < 2 )
		{
			SetDestWithFurn ( m_pathAStar.InitialPathToArray () [ m_pathAStar.InitialPathToArray ().Length - 1 ], _furn );
			return true;
		}

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

		if ( _initialTile.IsNeighbour ( _nextTile, false, true ) == false )
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
				m_stock = _stock;
			}
			else
			{
				m_stock = _stock;
			}

			m_movingStock = true;
			m_fullTimeToMoveStock = 1.0f + ((float)_stock.Weight/1000.0f) ;

			return true;
		}

		return false;
	}

	/// Returns the stock the character is currently carrying, if it matches the specified IDName.
	protected Stock TryGiveStock ( string _stockIDName )
	{
		Stock ourStock = new Stock("Generic", "Generic", 0, 0, Temperature.Room); //This is required as this variable is used later.

		bool foundItem = false;

		if ( m_basket )
		{
			foreach ( Stock s in m_basketContents.ToArray() )
			{
				if ( s.IDName == _stockIDName )
				{
					ourStock = s;
					foundItem = true;
					break;
				}
			}
		}
		else
		{
			if (m_stock.IDName == _stockIDName)
			{
				ourStock = m_stock;
				foundItem = true;
			}
		}

		if ( foundItem == false )
		{
			//If we get here, we didn't find any stock that is required to be given.
			Debug.LogWarning ( "Character tried to give stock they don't possess: Character: " + m_name + ", Stock: " + _stockIDName );
			return null;
		}
		
		Stock stock = ourStock;

		m_movingStock = true;
		m_fullTimeToMoveStock = 1.0f + ((float)stock.Weight/1000.0f);

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
