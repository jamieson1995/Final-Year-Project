using UnityEngine;
using System;
using System.Collections.Generic;

public class Furniture {

	public Tile m_tile { get; protected set; }

	public string m_baseFurnType {get; protected set;} //This represents the catergory of furniture, i.e. wall/door/shelf/moveable etc.

	public string m_furnType {get; protected set;} //This represents the item inside the category of furniture, i.e. wooden/automatic/doublewide etc.

	public float m_movementCost { get; protected set; } //This is a multiplier. So a value of '2' here means it takes 2 times as long to walk through this tile.
														//SPECIAL: If this is 0, it means the furniture is impassible (e.g. a wall)

	public int Width { get; protected set; }

	public int Height { get; protected set; }

	public bool m_linksToNeighbour { get; protected set; } //If this true then this furniture's sprite will change if there is a neighbour that needs it to change, i.e. walls.

	public bool m_draggable { get; protected set; } //This variable determines whether the furniture can be dragged along to be placed down
													//i.e. walls

	public Action<Furniture> cbOnChanged; //After this action gets activated, whenever it gets called, a given event will trigger, i.e changing the visual of this furniture

	public Func<Furniture, ENTERABILITY> m_isEnterable;

	Func<Tile, bool> funcPositionValidation; //This runs a function and checks the given tile to see if the placement of the furniture is valid, it returns a bool.

	public Dictionary<string, float> m_furnParameters;//This is used in conjuction with the updateActions action, so that parameters can be used.

	public Action<Furniture, float> m_updateActions; //This is an action, which when activated, allows this tile to run its Update function
													 //This means only furniture that need update functions can have them run i.e. doors for opening.

	//Used in conjuction with the virtual clone function.
	protected Furniture ( Furniture _other )
	{
		this.m_baseFurnType = _other.m_baseFurnType;
		this.m_furnType = _other.m_furnType;
		this.m_movementCost = _other.m_movementCost;
		this.Width = _other.Width;
		this.Height = _other.Height;
		this.m_linksToNeighbour = _other.m_linksToNeighbour;
		this.m_draggable = _other.m_draggable;

		m_furnParameters = new Dictionary<string, float> ( _other.m_furnParameters );
		if ( _other.m_updateActions != null )
		{
			this.m_updateActions = (Action<Furniture, float>)_other.m_updateActions.Clone ();
		}

		if ( _other.funcPositionValidation != null )
		{
			this.funcPositionValidation = (Func<Tile, bool>)_other.funcPositionValidation.Clone();
		}

		this.m_isEnterable = _other.m_isEnterable;
	}

	//When placing furniture, the actual furniture isn't being placed, a cloned version of the default furniture is. Therefore, a temp furniture needs to be cloned from the
	//original and that is what gets placed.
	virtual public Furniture Clone ()
	{
		return new Furniture(this);
	}

	public Furniture ( string _furnType, string _baseFurnType, float _movementCost, int _width, int _height, bool _linksToNeighbour, bool _draggable )
	{
		this.m_baseFurnType = _baseFurnType;
		this.m_furnType = _furnType;
		this.m_movementCost = _movementCost;
		this.Width = _width;
		this.Height = _height;
		this.m_linksToNeighbour = _linksToNeighbour;
		this.m_draggable = _draggable;

		this.funcPositionValidation = this.DEFAULT_IsValidPosition;

		m_furnParameters = new Dictionary<string, float> ();
	}

	//Attempts to place a certain furniture onto a given tile, if successful, a copy of that furniture is returned.
	static public Furniture PlaceInstanceOfFurniture ( Furniture _other, Tile _tile )
	{

		if ( _other.funcPositionValidation ( _tile ) == false )
		{
			Debug.LogError( "PlaceInstance -- Position validity function returned FALSE" );
			return null;
		}

		Furniture furn = _other.Clone (); 
		furn.m_tile = _tile;

		if ( _tile.PlaceFurniture ( furn ) == false )
		{
			//For some reason, we were unable to place our furniture in this tile.
			//Probably is was already occupied.
			return null;
		}

		if ( furn.m_linksToNeighbour )
		{
			Tile[] tiles = _tile.GetNeighbours ( false );

			if ( tiles[ 0 ] != null && tiles [ 0 ].m_furniture != null )
			{
				//We have a northern neighbour that links with us. So tell it to change its visuals
				tiles[0].m_furniture.cbOnChanged(tiles[0].m_furniture);
			}
			if ( tiles[1] != null && tiles[1].m_furniture != null )
			{
				//We have a northern neighbour that links with us. So tell it to change its visuals
				tiles[1].m_furniture.cbOnChanged(tiles[1].m_furniture);
			}
			if ( tiles[2] != null && tiles[2].m_furniture != null )
			{
				//We have a northern neighbour that links with us. So tell it to change its visuals
				tiles[2].m_furniture.cbOnChanged(tiles[2].m_furniture);
			}
			if ( tiles[3] != null && tiles[3].m_furniture != null )
			{
				//We have a northern neighbour that links with us. So tell it to change its visuals
				tiles[3].m_furniture.cbOnChanged(tiles[3].m_furniture);
			}
		}

		return furn;
	}

	public void UpdateNeighbours ()
	{
		Tile[] tiles = m_tile.GetNeighbours ( false );

		if ( tiles [ 0 ] != null && tiles [ 0 ].m_furniture != null && tiles [ 0 ].m_furniture.cbOnChanged != null )
		{
			//We have a northern neighbour that links with us. So tell it to change its visuals
			tiles [ 0 ].m_furniture.cbOnChanged ( tiles [ 0 ].m_furniture );
		}
		if ( tiles [ 1 ] != null && tiles [ 1 ].m_furniture != null && tiles [ 1 ].m_furniture.cbOnChanged != null )
		{
			//We have a northern neighbour that links with us. So tell it to change its visuals
			tiles [ 1 ].m_furniture.cbOnChanged ( tiles [ 1 ].m_furniture );
		}
		if ( tiles [ 2 ] != null && tiles [ 2 ].m_furniture != null && tiles [ 2 ].m_furniture.cbOnChanged != null )
		{
			//We have a northern neighbour that links with us. So tell it to change its visuals
			tiles [ 2 ].m_furniture.cbOnChanged ( tiles [ 2 ].m_furniture );
		}
		if ( tiles [ 3 ] != null && tiles [ 3 ].m_furniture != null && tiles [ 3 ].m_furniture.cbOnChanged != null )
		{
			//We have a northern neighbour that links with us. So tell it to change its visuals
			tiles [ 3 ].m_furniture.cbOnChanged ( tiles [ 3 ].m_furniture );
		}
	}

	public void Update ( float _deltaTime )
	{
		if ( m_updateActions != null )
		{
			m_updateActions (this, _deltaTime);
		}
	}

	public bool IsValidPosition ( Tile _tile )
	{
		return funcPositionValidation(_tile);
	}

	protected bool DEFAULT_IsValidPosition ( Tile _tile )
	{
		if ( _tile == null )
		{
			return false;
		}

		//This is for if the furniture is more than 1X1, and is used to check all the tiles for validity.
		for ( int x_off = _tile.X; x_off < ( _tile.X + Width ); x_off++ )
		{
			for ( int y_off = _tile.Y; y_off < ( _tile.Y + Height ); y_off++ )
			{
				Tile t2 = WorldController.instance.m_world.GetTileAt ( x_off, y_off );
				//Make sure tile doesn't already have furniture
				if ( t2.m_furniture != null )
				{
					return false;
				}
			}
		}

		//This is used so that a door must be placed between two walls.
		if ( m_baseFurnType == "Door" )
		{
			Furniture[] neighboursFurn = _tile.GetNeighboursFurniture ( false );

			if ( neighboursFurn [ 0 ] == null || neighboursFurn [ 0 ].m_baseFurnType != "Wall" )
			{

				if ( neighboursFurn [ 1 ] == null || neighboursFurn [ 1 ].m_baseFurnType != "Wall" )
				{
					return false;
				}
				else
				{
					if ( neighboursFurn [ 3 ] == null || neighboursFurn [ 3 ].m_baseFurnType != "Wall" )
					{
						return false;
					}
				}
			}
			else
			{
				if ( neighboursFurn [ 2 ] == null || neighboursFurn [ 2 ].m_baseFurnType != "Wall" )
				{
					return false;
				}
			}

		}
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
