//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class WorldController : MonoBehaviour {

	/// Reference the instance of this class.
	public static WorldController instance { get; protected set; }

	///Reference to the main World.
	public World m_world  { get; protected set; }

	///Reference to the m_tiledFloor Sprite
	public Sprite m_tiledFloor;

	///Reference to the m_concreteFloor Sprite.
	public Sprite m_concreteFloor;

	///This GameObject acts as the parent to the tile GameObjects. Keeps the hierarchy clean and uncluttered.
	public GameObject m_tileGameObjects;

	///Returns a Tile based onpn the given GameObject.
	Dictionary<Tile, GameObject> tileGameObjectMap;

	///Reference to the Start Scene.
	public GameObject m_startScene;

	///Reference to the Instruction Scene.
	public GameObject m_instructionScene;

	///Reference to the Game Screen	.
	public GameObject m_gameScreen;

	///Reference to the FurnitureSpriteController.
	public FurnitureSpriteController FSC;

	///Reference to the CharacterSpriteController.
	public CharacterSpriteController CSC;

	///Reference to the EventController.
	public EventController EC;

	public AudioClip m_backgroundMusicClip;

	AudioSource m_backgroundMusicSource;

	public GameObject m_pausedMessageGO;

	void Awake ()
	{

		if ( instance == null )
		{
			instance = this;
		}
		else
		{
			Debug.Log ( "Second World Controller tried to be created. Cannot have more than one World Controller." );
		}

		if ( m_backgroundMusicClip != null )
		{
			m_backgroundMusicClip.LoadAudioData ();
			m_backgroundMusicSource = gameObject.AddComponent<AudioSource> ();
			m_backgroundMusicSource.clip = m_backgroundMusicClip;
			m_backgroundMusicSource.loop = true;
			m_backgroundMusicSource.volume/=10;
		}

	}

	void Update ()
	{

		if ( m_world == null )
		{
			return;
		}

		if ( m_world.m_scenarioOver )
		{
			if ( m_backgroundMusicSource.isPlaying )
			{
				m_backgroundMusicSource.Stop();
			}
			return;
		}	

		m_world.Update(Time.deltaTime);
	}

	public void ReloadStartScene ()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	///Returns a Tile based upon given Vector3 coordinates and flag defining if coordinates can be out of bounds.
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

	///Sets up, and creates the entire world.
    public void SetUpWorld ()
	{
		m_world = new World ( 21, 23 );

		m_backgroundMusicSource.Play();

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
				if ( y == 0 || y == 1 || y == 2 || y == 22 || y == 21 || y == 20 || y == 19 || y == 18 || y == 17 || y == 16 || y == 15 || y == 14 || y == 13 )
				{
					sr.sprite = m_concreteFloor;
					tileData.m_outside = true;

					if ( ( y == 19 || y == 18 || y == 17 || y == 16 || y == 15 ) && ( x == 0 || x == 1 || x == 2 || x == 3 || x == 4 || x == 5 || x == 6 ) )
					{
						sr.sprite = m_tiledFloor;
						tileData.m_outside = false;
					}

					if ( ( y == 14 || y == 13 || y == 12 ) && ( x == 0 || x == 1 || x == 2 || x == 3 || x == 4 || x == 5 || x == 6 || x == 7 || x == 8 || x == 9 ) )
					{
						sr.sprite = m_tiledFloor;
						tileData.m_outside = false;
					}
				}
				else
				{
					sr.sprite = m_tiledFloor;
					tileData.m_outside = false;
				}

				if ( x == 16 && y == 6 )
				{
					tileData.m_queue = true;
					tileData.m_queueNum = 1;
				}
				else if (x == 17 && y == 6)
				{
					tileData.m_queue = true;
					tileData.m_queueNum = 2;
				}
				else if (x == 18 && y == 6)
				{
					tileData.m_queue = true;
					tileData.m_queueNum = 3;
				}
				else if (x == 18 && y == 7)
				{
					tileData.m_queue = true;
					tileData.m_queueNum = 4;
				}
				else if (x == 18 && y == 8)
				{
					tileData.m_queue = true;
					tileData.m_queueNum = 5;
				}
			}
		}
		//Center the camera in the middle of the world.
		Camera.main.transform.position = new Vector3( m_world.m_width/2, m_world.m_height/2, Camera.main.transform.position.z );
		Camera.main.orthographicSize = 10;

		m_startScene.SetActive(false);
		m_instructionScene.SetActive(false);
		m_gameScreen.SetActive(true);

		FSC.SetUpWorld();
		CSC.SetUpWorld();
		CreateShopEnvironment();

	}

	///This function is very messy, and hardcodes the shop needed for the testing.
	void CreateShopEnvironment ()
	{

		List<Stock> stockToAdd = new List<Stock> ();

		for ( int x = 0; x < m_world.m_width; x++ )
		{
			for ( int y = 0; y < m_world.m_height; y++ )
			{

				if ( y == 3 && x != 2 )//&& x != 11 )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 0 && y != 0 && y != 1 && y != 2 && y != 20 && y != 21 && y != 22 )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( y == 19 && ( x == 1 || x == 2 || x == 3 || x == 5 || x == 6 || x == 7 ) )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 20 && ( y == 4 || y == 5 || y == 6 || y == 7 || y == 8 || y == 9 || y == 10 || y == 11 ) )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( y == 12 && x != 8 && x != 9 )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 7 && ( y == 18 || y == 17 || y == 16 || y == 15 ) )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 10 && ( y == 13 || y == 14 ) )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( y == 15 && ( x == 8 || x == 9 || x == 10 ) )
				{
					m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 6 && ( y == 17 || y == 16 || y == 15 ) )
				{
					stockToAdd.Clear ();

					if ( y == 15 )
					{
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CocaColaDiet1750" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Lemonade2000" ].Clone() );
						}
					}

					if ( y == 16 )
					{
						for ( int i = 0; i < 25; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChoppedTomatoes4" ].Clone() );
						}

						for ( int i = 0; i < 25; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "TunaChucks4" ].Clone() );
						}
					}

					m_world.PlaceFurnitureInWorldWithStock ( "BackShelf", m_world.GetTileAt ( x, y ), stockToAdd, 4 );
				}

				if ( y == 18 && ( x == 2 || x == 3 || x == 5 ) )
				{
					stockToAdd.Clear ();
					m_world.PlaceFurnitureInWorld ( "BackShelf", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 1 && ( y == 17 || y == 16 || y == 15 || y == 14 ) )
				{
					stockToAdd.Clear ();
					m_world.PlaceFurnitureInWorld ( "BackShelf", m_world.GetTileAt ( x, y ), 2 );
				}

				if ( y == 13 && ( x == 2 || x == 3 || x == 4 ) )
				{
					stockToAdd.Clear ();
					m_world.PlaceFurnitureInWorld ( "BackShelf", m_world.GetTileAt ( x, y ), 3 );
				}

				if ( y == 5 && x == 14 )
				{
					stockToAdd.Clear ();
					m_world.PlaceFurnitureInWorld ( "Checkout", m_world.GetTileAt ( x, y ) );
				}

				if ( x == 1 && ( y == 6 || y == 7 || y == 8 || y == 9 || y == 10 ) )
				{
					stockToAdd.Clear ();
					if ( y == 6 )
					{
						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Bananas5Pack" ].Clone() );
						}
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Apples5Pack" ].Clone() );
						}
					}

					if ( y == 7 )
					{
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GreenSeedlessGrapes500" ].Clone() );
						}
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "RedSeedlessGrapes500" ].Clone() );
						}
						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LimeSingle" ].Clone() );
						}
						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LemonSingle" ].Clone() );
						}
					}

					if ( y == 8 )
					{
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MangoSingle" ].Clone() );
						}
						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PearSingle" ].Clone() );
						}
						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "OrangeSingle" ].Clone() );
						}
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Lemon5Pack" ].Clone() );
						}
					}

					if ( y == 9 )
					{
						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PineappleSingle" ].Clone() );
						}
						for ( int i = 0; i < 2; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "WatermelonSingle" ].Clone() );
						}
					}

					if ( y == 10 )
					{
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PassionFruit3Pack" ].Clone() );
						}
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PineappleChucks400" ].Clone() );
						}
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Mango3Pack" ].Clone() );
						}
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Pears550" ].Clone() );
						}
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Strawberries300" ].Clone() );
						}
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PineappleFingers500" ].Clone() );
						}

					}

					m_world.PlaceFurnitureInWorldWithStock ( "FrontShelf", m_world.GetTileAt ( x, y ), stockToAdd, 2 );
				}

				if ( y == 11 && ( x == 2 || x == 4 || x == 6 ) )
				{
					stockToAdd.Clear ();

					if ( x == 2 )
					{
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenBreast950" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BeefSteakMince500" ].Clone() );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SmokedBackBacon10" ].Clone() );
						}


					}

					if ( x == 4 )
					{
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "UnsmokedBackBacon10" ].Clone() );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LargeWholeChicken" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ThickPorkSausages12" ].Clone() );
						}
					}

					if ( x == 6 )
					{
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicChickenKiev2" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BreadedChickenGoujons270" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "DicedBeef600" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BeefSteakBurgers4" ].Clone() );
						}
					}
					m_world.PlaceFurnitureInWorldWithStock ( "BigFridge", m_world.GetTileAt ( x, y ), stockToAdd );
				}

				if ( x == 4 && ( y == 5 || y == 6 || y == 7 || y == 8 ) )
				{
					stockToAdd.Clear ();
					if ( y == 5 )
					{
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "WatermelonPack380" ].Clone() );
						}
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PlumSingle" ].Clone() );
						}
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CarrotSingle" ].Clone() );
						}
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "WhiteOnionSingle" ].Clone() );
						}
						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MushroomPack300" ].Clone() );
						}
						for ( int i = 0; i < 15; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SpringOnionPack100" ].Clone() );
						}
					}

					if ( y == 6 )
					{
						for ( int i = 0; i < 3; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PotatoesPack2500" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "RedOnionLoose" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BakingPotatoSingle" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CauliflowerSingle" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BroccoliSingle" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SweetPotatoSingle" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ParsnipSingle" ].Clone() );
						}
					}

					if ( y == 7 )
					{
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GreenBeans220" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BabyButtonMushrooms250" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LeekSingle" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicSingle" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CourgetteLoose" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BabySpinach240" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "RootGingerSingle" ].Clone() );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CucumberSingle" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MixedPeppers3Pack" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SaladTomatoes360" ].Clone() );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "IcebergLettuceSingle" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Chillies60" ].Clone() );
						}
					}
					m_world.PlaceFurnitureInWorldWithStock ( "FrontShelf", m_world.GetTileAt ( x, y ), stockToAdd, 4 );
				}

				if ( x == 5 && ( y == 5 || y == 7 ) )
				{
					stockToAdd.Clear ();
					if ( y == 5 )
					{
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PuffPastryRolled320" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GooseFat200" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CornishCustard500" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicBaguettes2" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PepperoniPizza309" ].Clone() );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CheeseFeastPizza341" ].Clone() );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "HamPineapplePizza310" ].Clone() );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenTikkaMasalaPilauRice450" ].Clone() );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BeefLasagne450" ].Clone() );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CottagePie450" ].Clone() );
						}

					}

					if ( y == 7 )
					{
						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "OrangeJuice2000" ].Clone() );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "OrangeJuice1000" ].Clone() );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "AppleJuice2000" ].Clone() );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "AppleJuice1000" ].Clone() );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CranberryJuice1000" ].Clone() );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PineappleJuice1000" ].Clone() );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "AppleJuice3" ].Clone() );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "OrangeJuice3" ].Clone() );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PineappleJuice3" ].Clone() );
						}
					}

					m_world.PlaceFurnitureInWorldWithStock ( "BigFridge", m_world.GetTileAt ( x, y ), stockToAdd, 2 );
				}

				if ( y == 8 && ( x == 12 || x == 13 || x == 14 || x == 15 || x == 16 ) )
				{
					stockToAdd.Clear ();

					if ( x == 12 )
					{
						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CocaCola1750" ].Clone() );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CocaColaDiet1750" ].Clone() );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Lemonade2000" ].Clone() );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LemonadeDiet2000" ].Clone() );
						}

						for ( int i = 0; i < 3; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CocaCola24" ].Clone() );
						}

						for ( int i = 0; i < 3; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CocaColaDiet24" ].Clone() );
						}
					}

					if ( x == 13 )
					{
						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "FizzyOrange2000" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "FizzyOrange6" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Lemonade12" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LemonadeDiet12" ].Clone() );
						}
					}

					if ( x == 14 )
					{
						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "StillWater12" ].Clone() );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SparklingWater2000" ].Clone() );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "StillWater2000" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BakedBeans4" ].Clone() );
						}
					}

					if ( x == 15 )
					{
						for ( int i = 0; i < 60; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "TomatoSoup400" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChoppedTomatoes4" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "TunaChucks4" ].Clone() );
						}
					}

					if ( x == 16 )
					{
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChocolateBiscuit8" ].Clone() );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "JaffaCakes2" ].Clone() );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChocolateDigestives500" ].Clone() );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MilkChocolateBar200" ].Clone() );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MilkChocolateButtons119" ].Clone() );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MarsBars4" ].Clone() );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MinstrelsPouch130" ].Clone() );
						}
					}

					m_world.PlaceFurnitureInWorldWithStock ( "FrontShelf", m_world.GetTileAt ( x, y ), stockToAdd, 3 );
				}

				if ( x == 8 && ( y == 5 || y == 7 ) )
				{
					stockToAdd.Clear ();


					if ( y == 5 )
					{
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SMilk4pt" ].Clone() );
						}

						for ( int i = 0; i < 22; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SSMilk4pt" ].Clone() );
						}

						for ( int i = 0; i < 19; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "WMilk4pt" ].Clone() );
						}
					}

					if ( y == 7 )
					{
						for ( int i = 0; i < 40; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SMilk1pt" ].Clone() );
						}

						for ( int i = 0; i < 40; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SSMilk1pt" ].Clone() );
						}

						for ( int i = 0; i < 40; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "WMilk1pt" ].Clone() );
						}

						for ( int i = 0; i < 19; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SMilk2pt" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SSMilk2pt" ].Clone() );
						}

						for ( int i = 0; i < 17; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "WMilk2pt" ].Clone() );
						}
					}

					if ( x == 17 )
					{
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PuffPastryRolled320" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GooseFat200" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CornishCustard500" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicBaguettes2" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PepperoniPizza309" ].Clone() );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CheeseFeastPizza341" ].Clone() );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "HamPineapplePizza310" ].Clone() );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenTikkaMasalaPilauRice450" ].Clone() );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BeefLasagne450" ].Clone() );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CottagePie450" ].Clone() );
						}

					}

					m_world.PlaceFurnitureInWorldWithStock ( "BigFridge", m_world.GetTileAt ( x, y ), stockToAdd, 4 );
				}

				if ( x == 9 && ( y == 5 || y == 7 ) )
				{
					stockToAdd.Clear ();

					if ( y == 5 )
					{
						for ( int i = 0; i < 40; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MediumEggs6" ].Clone() );
						}

						for ( int i = 0; i < 19; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MediumEggs12" ].Clone() );
						}

						for ( int i = 0; i < 35; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LargeEggs6" ].Clone() );
						}

						for ( int i = 0; i < 21; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LargeEggs12" ].Clone() );
						}

						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SaltedButter250" ].Clone() );
						}

						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "UnsaltedButter250" ].Clone() );
						}
					}

					if ( y == 7 )
					{
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "DoubleCream300" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "DoubleCream600" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "DoubleCream600" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SlightySaltedSpreadable500" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "OriginalSpread500" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "DoubleCream150" ].Clone() );
						}

					}

					m_world.PlaceFurnitureInWorldWithStock ( "BigFridge", m_world.GetTileAt ( x, y ), stockToAdd, 2 );
				}

				if ( y == 11 && ( x == 10 || x == 12 || x == 14 || x == 16 ) )
				{
					stockToAdd.Clear ();

					if ( x == 10 )
					{
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenDippers42" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenBreastFillet1000" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SouthernFriedChicken2" ].Clone() );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PorkSausages20" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicButterBreadedChickenKievs4" ].Clone() );
						}
					}

					if ( x == 12 )
					{
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CrispyChicken5" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenQuarterPounders4" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BeefBurgers4" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenFingers14" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "HomeChips1500" ].Clone() );
						}	

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "OvenChips1500" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PotatoWaffles10" ].Clone() );
						}
					}

					if ( x == 14 )
					{
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ExtraChunkyHomeChips1000" ].Clone() );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BatteredOnionRings375" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "FourCheesePizza330" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "DoublePepperoniPizza330" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicCheesePizzaBread210" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BreadedOnionRings750" ].Clone() );
						}
					}

					if ( x == 16 )
					{
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GardenPeas1000" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BroccoliFlorets900" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "DicedOnions500" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MixedPeppers500" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Sweetcorn907" ].Clone() );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SlicedCarrots1000" ].Clone() );
						}
					}

					m_world.PlaceFurnitureInWorldWithStock ( "BigFreezer", m_world.GetTileAt ( x, y ), stockToAdd );
				}
				if ( y == 18 && ( x == 9 || x == 11 || x == 13 || x == 15 || x == 17 ) )
				{
					stockToAdd.Clear ();

				}
			}
		}

		stockToAdd.Clear ();
		#region StocktoAddDefined

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "BananaMilkshake400" ].Clone() );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "ChocolateMilkshake400" ].Clone() );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "StrawberryMilkshake400" ].Clone() );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "StillWater500" ].Clone() );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "CokeCola500" ].Clone() );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "CokeColaDiet500" ].Clone() );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "FizzyOrange500" ].Clone() );
		}

		#endregion
		m_world.PlaceFurnitureInWorldWithStock ( "BigFridge", m_world.GetTileAt ( 18, 5 ), stockToAdd, 3 );
		stockToAdd.Clear ();
		#region StockToAddDefined

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "MatureCheddarBlock350" ].Clone() );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "MatureCheddarBlock550" ].Clone() );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "GratedCheddar500" ].Clone() );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "MozzarellaBlock150" ].Clone() );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "GratedMozzarella250" ].Clone() );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "MilkChocolateMousse6" ].Clone() );
		}

		for ( int i = 0; i < 5; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "StrawberryTrifle3" ].Clone() );
		}

		for ( int i = 0; i < 5; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "VanillaCheesecake540" ].Clone() );
		}

		for ( int i = 0; i < 5; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "ChocolateCheesecake540" ].Clone() );
		}

		for ( int i = 0; i < 5; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "TreacleTart380" ].Clone() );
		}

		for ( int i = 0; i < 5; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "LemonTart385" ].Clone() );
		}

		#endregion
		m_world.PlaceFurnitureInWorldWithStock ( "BigFridge", m_world.GetTileAt ( 18, 11 ), stockToAdd );
		stockToAdd.Clear ();
		#region StockToAddDefined
		for ( int i = 0; i < 10; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenBreast950" ].Clone() );
		}

		for ( int i = 0; i < 10; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "BeefSteakMince500" ].Clone() );
		}

		for ( int i = 0; i < 10; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "SmokedBackBacon10" ].Clone() );
		}

		for ( int i = 0; i < 10; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "UnsmokedBackBacon10" ].Clone() );
		}

		for ( int i = 0; i < 3; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "LargeWholeChicken" ].Clone() );
		}

		for ( int i = 0; i < 6; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicBaguettes2" ].Clone() );
		}

		for ( int i = 0; i < 6; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "PepperoniPizza309" ].Clone() );
		}

		for ( int i = 0; i < 6; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "CheeseFeastPizza341" ].Clone() );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "CocaCola1750" ].Clone() );
		}
		#endregion
		m_world.PlaceFurnitureInWorldWithStock ( "Stockcage", m_world.GetTileAt ( 3, 14 ), stockToAdd, 3 );
		stockToAdd.Clear ();
		#region StockToAddDefined

		for ( int i = 0; i < 12; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "BakedBeans4" ].Clone() );
		}

		for ( int i = 0; i < 50; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "TomatoSoup400" ].Clone() );
		}

		for ( int i = 0; i < 50; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "ChocolateBiscuit8" ].Clone() );
		}

		for ( int i = 0; i < 50; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "JaffaCakes2" ].Clone() );
		}

		#endregion
		m_world.PlaceFurnitureInWorldWithStock ( "Stockcage", m_world.GetTileAt ( 4, 18 ), stockToAdd );
		m_world.PlaceFurnitureInWorld ( "Trolley", m_world.GetTileAt ( 2, 16 ), 2 );
		m_world.PlaceFurnitureInWorld ( "Door", m_world.GetTileAt ( 2, 3 ) );
		m_world.PlaceFurnitureInWorld ( "Door", m_world.GetTileAt ( 4, 19 ) );
		Employee e = m_world.CreateEmployee ( "James", m_world.GetTileAt ( 4, 16), Title.Manager, 46 );
		m_world.InCharge = e;
		e.m_authorityLevel = 1;
		m_world.CreateEmployee("Michael", m_world.GetTileAt ( 9, 10 ), Title.AssistantManager, 24);

		m_world.m_frontFurniture.Reverse ();
		m_world.m_backFurniture.Reverse ();


		foreach ( Furniture f in m_world.m_frontFurniture )
		{
			foreach ( var key in f.m_stock.Keys )
			{
				int i = 0;
				while ( i < f.m_stock [ key ].Count )
				{
					if ( i % 2 == 0 )
					{
						f.m_stock[key][i].m_facedUp = false;
					}	
					i++;
				}
			}
		}

	}

	/// Calls the SetGameSpeed function in m_world with the specified parameter.
	public void SetGameSpeed ( float _gameSpeed )
	{
		if ( m_world != null )
		{
			m_world.SetGameSpeed ( _gameSpeed );
		}

		if ( _gameSpeed == 0 )
		{
			m_pausedMessageGO.SetActive(true);
		}
		else
		{
			m_pausedMessageGO.SetActive(false);
		}

	}

}
