//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

public class FurnitureSpriteController : MonoBehaviour {

	///Returns the GameObject linked to the furniture input.
	Dictionary<Furniture, GameObject> m_furnitureGameObjectMap; 

	///Returns the Sprite based on the input name.
	public Dictionary<string, Sprite> m_furnitureSprites { get; protected set; } 

	///An array of all furniture sprites.
	public Sprite[] m_sprites; 

	/// Reference to WorldController.instance.m_world
	World m_world; 

	void Start ()
	{
		m_furnitureGameObjectMap = new Dictionary<Furniture, GameObject> ();
		m_furnitureSprites = new Dictionary<string, Sprite> ();
	}

	/// Establishes the reference to WorldController.instance.m_world and sets up the callbacks and sprites.
	public void SetUpWorld ()
	{
		m_world = WorldController.instance.m_world;
		m_world.RegisterFurnitureCreated ( OnFurnitureCreated );
		m_world.RegisterFurnitureMoved( OnFurnitureMoved );
		foreach ( Sprite s in m_sprites )
		{
			m_furnitureSprites[s.name] = s;
		}
	}

	/// Callback function which runs when a furniture gets created.
	public void OnFurnitureCreated ( Furniture _furn )
	{
		GameObject furn_go = new GameObject ();

		//Door is unique as it rotates if the walls it connects to are north and south rather than east and west.
		if ( _furn.m_name == "Door" )
		{
			Tile northTile = m_world.GetTileAt ( _furn.m_mainTile.X, _furn.m_mainTile.Y + 1 );
			Tile southTile = m_world.GetTileAt ( _furn.m_mainTile.X, _furn.m_mainTile.Y - 1 );
			if ( northTile != null && southTile != null &&
			     northTile.m_furniture != null && southTile.m_furniture != null &&
			     northTile.m_furniture.m_name == "Wall" && southTile.m_furniture.m_name == "Wall" )
			{
				furn_go.transform.rotation = Quaternion.Euler ( 0, 0, 90 );
			}
		}

		m_furnitureGameObjectMap.Add ( _furn, furn_go );

		furn_go.name = _furn.m_name + "(" + _furn.m_mainTile.X + "_" + _furn.m_mainTile.Y + ")";
		furn_go.transform.position = new Vector3 ( _furn.m_mainTile.X + ( ( _furn.Width - 1 ) ) / 2f, _furn.m_mainTile.Y + ( ( _furn.Height - 1 ) / 2f ), 0 );
		//Rotates the sprite based upon the furniture rotation reference, excludes door furniture.
		if ( _furn.m_name != "Door" )
		{
			furn_go.transform.rotation = Quaternion.Euler ( 0, 0, ( _furn.m_rotation * 90 ) - 90 );
		}
		furn_go.transform.SetParent(this.transform, true);
		SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
		sr.sprite = GetSpriteForFurniture( _furn );
		//Door furniture needs to render on top of all other furniture.
		if (_furn.m_name == "Door" )
		{
			sr.sortingLayerName = "Furniture - Door";
		}
		else
		{
			sr.sortingLayerName = "Furniture";
		}

		//Registers the callback for when a furniture changes visually - position, sprite etc.
		_furn.RegisterOnChangedCallback(OnFurnitureChanged);
	}

	///Callback function which runs when a furniture changes.
	void OnFurnitureChanged ( Furniture _furn )
	{
		//Make sure the furniture's graphics get corrected.

		if ( m_furnitureGameObjectMap.ContainsKey ( _furn ) == false )
		{
			Debug.LogError ( "OnFurnitureChanged -- Trying to change visuals for furniture not in our map" );
			return;	
		}
		GameObject furn_go = m_furnitureGameObjectMap [ _furn ];
		furn_go.GetComponent<SpriteRenderer> ().sprite = GetSpriteForFurniture ( _furn );
		furn_go.transform.position = new Vector3 (_furn.m_mainTile.X, _furn.m_mainTile.Y, 0);
    }

    /// Returns the sprite required for the furniture given.
	public Sprite GetSpriteForFurniture ( Furniture _furn )
	{
		string spriteName = _furn.m_name;
		if ( _furn.m_linksToNeighbour == false )
		{
			if ( _furn.m_name == "Door" )
			{
				
				if ( _furn.m_furnParameters [ "m_openness" ] < 0.1f )
				{
					//Door is closed
					spriteName += "_EW_1";
				}
				else if ( _furn.m_furnParameters [ "m_openness" ] < 0.5f )
				{
					//Door is half open
					spriteName += "_EW_2";
				}
				else
				{
					//Door is  open
					spriteName += "_EW_3";
				}
			}

			if ( m_furnitureSprites [ spriteName ] != null )
				return m_furnitureSprites [ spriteName ];
			if ( m_furnitureSprites [ spriteName + "_" ] != null )
				return m_furnitureSprites [ spriteName ];

			Debug.LogError ( "GetSpriteForFurniture(Furniture) -- No sprite with name: " + _furn.m_name );
		}

		spriteName += "_";

		//Check for neighbour - North, East, South, West

		int x = _furn.m_mainTile.X;
		int y = _furn.m_mainTile.Y;

		Tile t;
		//Check North
		t = m_world.GetTileAt ( x, y + 1 );
		if ( t != null && t.m_furniture != null && t.m_furniture.m_name == _furn.m_name )
		{

			spriteName += "N";
		}
		//Check East
		t = m_world.GetTileAt ( x + 1, y );
		if ( t != null && t.m_furniture != null && t.m_furniture.m_name == _furn.m_name )
		{

			spriteName += "E";
			
		}
		//Check South
		t = m_world.GetTileAt ( x, y - 1 );
		if ( t != null && t.m_furniture != null && t.m_furniture.m_name == _furn.m_name )
		{


			spriteName += "S";
			
			
		}
		//Check West
		t = m_world.GetTileAt ( x - 1, y );
		if ( t != null && t.m_furniture != null && t.m_furniture.m_name == _furn.m_name )
		{

			spriteName += "W";
			

		}

		if ( m_furnitureSprites.ContainsKey ( spriteName ) == false )
		{
			Debug.LogError ( "GetSpriteForFurniture(Furniture) -- No sprite with name: " + spriteName );
			return null;
		}

		return m_furnitureSprites [ spriteName ];
	}

	/// Returns the sprite for a furniture based on the name given.
	public Sprite GetSpriteForFurniture ( string _furnName )
	{
		if ( m_furnitureSprites.ContainsKey ( _furnName ) )
		{
			return m_furnitureSprites [ _furnName ];
		}
		if ( m_furnitureSprites.ContainsKey ( _furnName + "_" ) )
		{
			return m_furnitureSprites [ _furnName + "_" ];
		}

		Debug.LogError ( "GetSpriteForFurniture(string) -- No sprite with name: " + _furnName );
		return null;
	}

	///Callback function which runs when a furniture moves.
	void OnFurnitureMoved ( Furniture _furn )
	{
		if ( m_furnitureGameObjectMap.ContainsKey ( _furn ) == false )
		{
			Debug.LogError("OnCharacterMoved -- Trying to change visuals for a furniture not in our map!");
			return;
		}

		GameObject furn_go = m_furnitureGameObjectMap[_furn];
		furn_go.transform.position = new Vector3(_furn.X, _furn.Y, 0);
	}
}
