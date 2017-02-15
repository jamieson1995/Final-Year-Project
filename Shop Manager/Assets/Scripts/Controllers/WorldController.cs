using UnityEngine;
using System.Collections.Generic;

public class WorldController : MonoBehaviour {

	public static WorldController instance {get; protected set;} //Singleton

	public World m_world {get; protected set;}

	public Sprite m_tiledFloor;
	public Sprite m_concreteFloor;

	public GameObject m_tileGameObjects; //This will be the parent of all the tile GameObjects, this allows us to keep the hierarchy uncluttered. 

	Dictionary<Tile, GameObject> tileGameObjectMap; //This is needed to track which tile is related to which GameObject.

	public GameObject m_createWorldButton;
	public GameObject m_gameUI;

	public FurnitureSpriteController FSC;
	public CharacterSpriteController CSC;

	void Awake(){

		if (instance == null){
			instance = this;
		}
		else
			Debug.Log("Second World Controller tried to be created. Cannot have more than one World Controller.");

		
	}

	void Update ()
	{
		if ( m_world == null )
		{
			return;
		}

		m_world.Update(Time.deltaTime); //TODO Speed controls?
	}

	public Tile GetTileAtWorldCoord (Vector3 _coord, bool _canBeOutOfBounds = false)
    {
        int x = Mathf.FloorToInt (_coord.x + 0.5f);
        int y = Mathf.FloorToInt (_coord.y + 0.5f);
        if (_canBeOutOfBounds == false)
        {
            if (x < 0 || x > m_world.m_width || y < 0 || y > m_world.m_height)
            {
          	  return null;
            }
        }

        return m_world.GetTileAt(x,y);
    }

    public void SetUpWorld ()
	{
		m_world = new World ( 21, 23 );

		//Instantiate our dictionary that tracks which GameObject is rendering which tile data.
		tileGameObjectMap = new Dictionary<Tile, GameObject> ();

		//With World set up, we need to instantiate the game object for each tile.

		for ( int x = 0; x < m_world.m_width; x++ )
		{
			for ( int y = 0; y < m_world.m_height; y++ )
			{
				Tile tileData = m_world.GetTileAt ( x, y );
				GameObject tileGO = new GameObject ();
				tileGameObjectMap.Add ( tileData, tileGO );
				tileGO.name = "Tile (" + x + "," + y + ")";
				tileGO.transform.position = new Vector3 ( tileData.X, tileData.Y, 0 );
				tileGO.transform.SetParent ( m_tileGameObjects.transform, true );
				SpriteRenderer sr = tileGO.AddComponent<SpriteRenderer> ();
				sr.sortingLayerName = "Tile";
				tileGO = tileGameObjectMap [ tileData ];
				if ( y == 0 || y == 1 || y == 2 || y == 22 || y == 21 || y == 20 )
				{
					sr.sprite = m_concreteFloor;
				}
				else
				{
					sr.sprite = m_tiledFloor;
				}


			}
		}
		//Center the camera in the middle of the world.
		Camera.main.transform.position = new Vector3( m_world.m_width/2, m_world.m_height/2, Camera.main.transform.position.z );
		Camera.main.orthographicSize = 10;

		m_createWorldButton.SetActive(false);
		m_gameUI.SetActive(true);

		FSC.SetUpWorld();
		CSC.SetUpWorld();
		CreateShopEnvironment();

	}

	void CreateShopEnvironment ()
	{
		for ( int x = 0; x < m_world.m_width; x++ )
		{
			for ( int y = 0; y < m_world.m_height; y++ )
			{

				if ( y == 3 && x != 4 )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 0 && y != 0 && y != 1 && y != 2 && y != 20 && y != 21 && y != 22 )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( y == 19 && x != 4 )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 20 && y != 0 && y != 1 && y != 2 && y != 20 && y != 21 && y != 22 )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( y == 12 && ( x == 1 || x == 2 || x == 3 || x == 4 || x == 5 || x == 6 || x == 7 ) )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 7 && ( y == 18 || y == 17 || y == 16 || y == 15 || y == 14 ) )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 6 && ( y == 17 || y == 16 || y == 15 ) )
				{
					m_world.PlaceFurnitureInWorld ( "BackShelf", m_world.GetTileAt ( x, y ), 4 );
				}

				if ( y == 18 && ( x == 2 || x == 3 || x == 5 ) )
				{
					m_world.PlaceFurnitureInWorld ( "BackShelf", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 1 && ( y == 17 || y == 16 || y == 15 || y == 14 ) )
				{
					m_world.PlaceFurnitureInWorld ( "BackShelf", m_world.GetTileAt ( x, y ), 2 );
				}

				if ( y == 13 && ( x == 2 || x == 3 || x == 4 ) )
				{
					m_world.PlaceFurnitureInWorld ( "BackShelf", m_world.GetTileAt ( x, y ), 3 );
				}

				if ( y == 5 && ( x == 13 || x == 17 ) )
				{
					m_world.PlaceFurnitureInWorld ( "Checkout", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 1 && ( y == 6 || y == 7 || y == 8 || y == 9 || y == 10 ) )
				{
					m_world.PlaceFurnitureInWorld ( "FrontShelf", m_world.GetTileAt ( x, y ), 2 );
				}

				if ( y == 11 && ( x == 2 || x == 3 || x == 4 || x == 5 || x == 6 || x == 7 ) )
				{
					m_world.PlaceFurnitureInWorld ( "FrontShelf", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 4 && ( y == 5 || y == 6 || y == 7 || y == 8 ) )
				{
					m_world.PlaceFurnitureInWorld ( "FrontShelf", m_world.GetTileAt ( x, y ), 4 );
				}

				if ( x == 5 && ( y == 5 || y == 6 || y == 7 || y == 8 ) )
				{
					m_world.PlaceFurnitureInWorld ( "FrontShelf", m_world.GetTileAt ( x, y ), 2 );
				}

				if ( x == 8 && ( y == 5 || y == 6 || y == 7 || y == 8 ) )
				{
					m_world.PlaceFurnitureInWorld ( "FrontShelf", m_world.GetTileAt ( x, y ), 4 );
				}

				if ( x == 9 && ( y == 5 || y == 6 || y == 7 || y == 8 ) )
				{
					m_world.PlaceFurnitureInWorld ( "FrontShelf", m_world.GetTileAt ( x, y ), 2 );
				}

				if ( x == 8 && ( y == 14 || y == 15 || y == 16 || y == 17 ) )
				{
					m_world.PlaceFurnitureInWorld ( "FrontShelf", m_world.GetTileAt ( x, y ), 2 );
				}

				if ( y == 18 && ( x == 9 || x == 10 || x == 11 || x == 12 || x == 13 || x == 14 || x == 15 || x == 16 || x == 17 || x == 18 ) )
				{
					m_world.PlaceFurnitureInWorld ( "FrontShelf", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 11 && ( y == 11 || y == 12 || y == 13 || y == 14 || y == 15 ) )
				{
					m_world.PlaceFurnitureInWorld ( "FrontShelf", m_world.GetTileAt ( x, y ), 4 );
				}

				if ( x == 12 && ( y == 11 || y == 12 || y == 13 || y == 14 || y == 15 ) )
				{
					m_world.PlaceFurnitureInWorld ( "FrontShelf", m_world.GetTileAt ( x, y ), 2 );
				}

				if ( x == 15 && ( y == 10 || y == 11 || y == 12 || y == 13 || y == 14 || y == 15 ) )
				{
					m_world.PlaceFurnitureInWorld ( "FrontShelf", m_world.GetTileAt ( x, y ), 4 );
				}

				if ( x == 16 && ( y == 9 || y == 10 || y == 11 || y == 12 || y == 13 || y == 14 || y == 15 ) )
				{
					m_world.PlaceFurnitureInWorld ( "FrontShelf", m_world.GetTileAt ( x, y ), 2 );
				}

				if ( x == 19 && ( y == 10 || y == 12 || y == 14 || y == 16 ) )
				{
					m_world.PlaceFurnitureInWorld ( "BigFreezer", m_world.GetTileAt ( x, y ), 4 );
				}
			}
		}

		m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( 15, 9 ) );
		m_world.PlaceFurnitureInWorld ( "BigFridge", m_world.GetTileAt ( 15, 8 ) );
		m_world.PlaceFurnitureInWorld ( "BigFridge", m_world.GetTileAt ( 19, 8 ), 4 );
		m_world.PlaceFurnitureInWorld ( "Stockcage", m_world.GetTileAt ( 3, 14 ), 3 );
		m_world.PlaceFurnitureInWorld ( "Stockcage", m_world.GetTileAt ( 4, 17 ) );
		m_world.PlaceFurnitureInWorld ( "Door", m_world.GetTileAt ( 7, 13 ) );
		m_world.PlaceFurnitureInWorld ( "Door", m_world.GetTileAt ( 4, 3 ) );
		m_world.PlaceFurnitureInWorld ( "Door", m_world.GetTileAt ( 4, 19 ) );
	//	m_world.CreateEmployee("James", 10000, m_world.GetTileAt ( 1, 1 ), "Manager");
	}

}
