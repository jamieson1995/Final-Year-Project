//////////////////////////////////////////////////////
//Copyright James Jamieson 2016/2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

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
				if ( y == 0 || y == 1 || y == 2 || y == 22 || y == 21 || y == 20 || y == 19 || y == 18 || y == 17 || y == 16 || y == 15 || y == 14 || y == 13 )
				{
					sr.sprite = m_concreteFloor;
					tileData.m_outside = true;

					if ( ( y == 19 || y == 18 || y == 17 || y == 16 || y == 15 ) && ( x == 0 || x == 1 || x == 2 || x == 3 || x == 4 || x == 5 || x == 6) )
					{
						sr.sprite = m_tiledFloor;
						tileData.m_outside = false;
					}

					if ( ( y == 14 || y == 13 || y == 12 ) && (x == 0 || x == 1 || x == 2 || x == 3 || x == 4 || x == 5 || x == 6 || x == 7 || x == 8 || x == 9) )
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

		List<Stock> stockToAdd = new List<Stock> ();

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
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CocaColaDiet1750" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Lemonade2000" ] );
						}
					}

					if ( y == 16 )
					{
						for ( int i = 0; i < 25; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChoppedTomatoes4" ] );
						}

						for ( int i = 0; i < 25; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "TunaChucks4" ] );
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

				if ( y == 5 && ( x == 13 || x == 17 ) )
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
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Bananas5Pack" ] );
						}
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Apples5Pack" ] );
						}
					}

					if ( y == 7 )
					{
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GreenSeedlessGrapes500" ] );
						}
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "RedSeedlessGrapes500" ] );
						}
						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LimeSingle" ] );
						}
						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LemonSingle" ] );
						}
					}

					if ( y == 8 )
					{
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MangoSingle" ] );
						}
						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PearSingle" ] );
						}
						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "OrangeSingle" ] );
						}
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Lemon5Pack" ] );
						}
					}

					if ( y == 9 )
					{
						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PineappleSingle" ] );
						}
						for ( int i = 0; i < 2; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "WatermelonSingle" ] );
						}
					}

					if ( y == 10 )
					{
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PassionFruit3Pack" ] );
						}
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PineappleChucks400" ] );
						}
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Mango3Pack" ] );
						}
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Pears550" ] );
						}
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Strawberries300" ] );
						}
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PineappleFingers500" ] );
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
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenBreast950" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BeefSteakMince500" ] );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SmokedBackBacon10" ] );
						}


					}

					if ( x == 4 )
					{
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "UnsmokedBackBacon10" ] );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LargeWholeChicken" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ThickPorkSausages12" ] );
						}
					}

					if ( x == 6 )
					{
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicChickenKiev2" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BreadedChickenGoujons270" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "DicedBeef600" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BeefSteakBurgers4" ] );
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
							stockToAdd.Add ( m_world.m_stockPrototypes [ "WatermelonPack380" ] );
						}
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PlumSingle" ] );
						}
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CarrotSingle" ] );
						}
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "WhiteOnionSingle" ] );
						}
						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MushroomPack300" ] );
						}
						for ( int i = 0; i < 15; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SpringOnionPack100" ] );
						}
					}

					if ( y == 6 )
					{
						for ( int i = 0; i < 3; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PotatoesPack2500" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "RedOnionLoose" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BakingPotatoSingle" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CauliflowerSingle" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BroccoliSingle" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SweetPotatoSingle" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ParsnipSingle" ] );
						}
					}

					if ( y == 7 )
					{
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GreenBeans220" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BabyButtonMushrooms250" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LeekSingle" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicSingle" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CourgetteLoose" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BabySpinach240" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "RootGingerSingle" ] );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CucumberSingle" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MixedPeppers3Pack" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SaladTomatoes360" ] );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "IcebergLettuceSingle" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Chillies60" ] );
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
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PuffPastryRolled320" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GooseFat200" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CornishCustard500" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicBaguettes2" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PepperoniPizza309" ] );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CheeseFeastPizza341" ] );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "HamPineapplePizza310" ] );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenTikkaMasalaPilauRice450" ] );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BeefLasagne450" ] );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CottagePie450" ] );
						}

					}

					if ( y == 7 )
					{
						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "OrangeJuice2000" ] );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "OrangeJuice1000" ] );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "AppleJuice2000" ] );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "AppleJuice1000" ] );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CranberryJuice1000" ] );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PineappleJuice1000" ] );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "AppleJuice3" ] );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "OrangeJuice3" ] );
						}

						for ( int i = 0; i < 12; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PineappleJuice3" ] );
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
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CocaCola1750" ] );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CocaColaDiet1750" ] );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Lemonade2000" ] );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LemonadeDiet2000" ] );
						}

						for ( int i = 0; i < 3; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CocaCola24" ] );
						}

						for ( int i = 0; i < 3; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CocaColaDiet24" ] );
						}
					}

					if ( x == 13 )
					{
						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "FizzyOrange2000" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "FizzyOrange6" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Lemonade12" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LemonadeDiet12" ] );
						}
					}

					if ( x == 14 )
					{
						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "StillWater12" ] );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SparklingWater2000" ] );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "StillWater2000" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BakedBeans4" ] );
						}
					}

					if ( x == 15 )
					{
						for ( int i = 0; i < 60; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "TomatoSoup400" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChoppedTomatoes4" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "TunaChucks4" ] );
						}
					}

					if ( x == 16 )
					{
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChocolateBiscuit8" ] );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "JaffaCakes2" ] );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChocolateDigestives500" ] );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MilkChocolateBar200" ] );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MilkChocolateButtons119" ] );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MarsBars4" ] );
						}

						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MinstrelsPouch130" ] );
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
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SMilk4pt" ] );
						}

						for ( int i = 0; i < 22; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SSMilk4pt" ] );
						}

						for ( int i = 0; i < 19; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "WMilk4pt" ] );
						}
					}

					if ( y == 7 )
					{
						for ( int i = 0; i < 40; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SMilk1pt" ] );
						}

						for ( int i = 0; i < 40; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SSMilk1pt" ] );
						}

						for ( int i = 0; i < 40; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "WMilk1pt" ] );
						}

						for ( int i = 0; i < 19; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SMilk2pt" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SSMilk2pt" ] );
						}

						for ( int i = 0; i < 17; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "WMilk2pt" ] );
						}
					}

					if ( x == 17 )
					{
						for ( int i = 0; i < 30; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PuffPastryRolled320" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GooseFat200" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CornishCustard500" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicBaguettes2" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PepperoniPizza309" ] );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CheeseFeastPizza341" ] );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "HamPineapplePizza310" ] );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenTikkaMasalaPilauRice450" ] );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BeefLasagne450" ] );
						}

						for ( int i = 0; i < 6; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CottagePie450" ] );
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
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MediumEggs6" ] );
						}

						for ( int i = 0; i < 19; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MediumEggs12" ] );
						}

						for ( int i = 0; i < 35; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LargeEggs6" ] );
						}

						for ( int i = 0; i < 21; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "LargeEggs12" ] );
						}

						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SaltedButter250" ] );
						}

						for ( int i = 0; i < 50; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "UnsaltedButter250" ] );
						}
					}

					if ( y == 7 )
					{
						for ( int i = 0; i < 10; i++ )
						{
								stockToAdd.Add ( m_world.m_stockPrototypes [ "DoubleCream300" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
								stockToAdd.Add ( m_world.m_stockPrototypes [ "DoubleCream600" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
								stockToAdd.Add ( m_world.m_stockPrototypes [ "DoubleCream600" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SlightySaltedSpreadable500" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "OriginalSpread500" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "DoubleCream150" ] );
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
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenDippers42" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenBreastFillet1000" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SouthernFriedChicken2" ] );
						}

						for ( int i = 0; i < 5; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PorkSausages20" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicButterBreadedChickenKievs4" ] );
						}
					}

					if ( x == 12 )
					{
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "CrispyChicken5" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenQuarterPounders4" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BeefBurgers4" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenFingers14" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "HomeChips1500" ] );
						}	

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "OvenChips1500" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "PotatoWaffles10" ] );
						}
					}

					if ( x == 14 )
					{
						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "ExtraChunkyHomeChips1000" ] );
						}

						for ( int i = 0; i < 20; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BatteredOnionRings375" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "FourCheesePizza330" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "DoublePepperoniPizza330" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicCheesePizzaBread210" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BreadedOnionRings750" ] );
						}
					}

					if ( x == 16 )
					{
						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "GardenPeas1000" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "BroccoliFlorets900" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "DicedOnions500" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "MixedPeppers500" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "Sweetcorn907" ] );
						}

						for ( int i = 0; i < 10; i++ )
						{
							stockToAdd.Add ( m_world.m_stockPrototypes [ "SlicedCarrots1000" ] );
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

		stockToAdd.Clear();
		#region StocktoAddDefined

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "BananaMilkshake400" ] );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "ChocolateMilkshake400" ] );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "StrawberryMilkshake400" ] );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "StillWater500" ] );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "CokeCola500" ] );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "CokeColaDiet500" ] );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "FizzyOrange500" ] );
		}

		#endregion
		m_world.PlaceFurnitureInWorldWithStock ( "BigFridge", m_world.GetTileAt ( 19, 8 ), stockToAdd, 4 );
		stockToAdd.Clear();
		#region StockToAddDefined

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "MatureCheddarBlock350" ] );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "MatureCheddarBlock550" ] );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "GratedCheddar500" ] );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "MozzarellaBlock150" ] );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "GratedMozzarella250" ] );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "MilkChocolateMousse6" ] );
		}

		for ( int i = 0; i < 5; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "StrawberryTrifle3" ] );
		}

		for ( int i = 0; i < 5; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "VanillaCheesecake540" ] );
		}

		for ( int i = 0; i < 5; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "ChocolateCheesecake540" ] );
		}

		for ( int i = 0; i < 5; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "TreacleTart380" ] );
		}

		for ( int i = 0; i < 5; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "LemonTart385" ] );
		}

		#endregion
		m_world.PlaceFurnitureInWorldWithStock ( "BigFridge", m_world.GetTileAt ( 18, 11 ), stockToAdd );
		stockToAdd.Clear();
		#region StockToAddDefined
		for ( int i = 0; i < 10; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "ChickenBreast950" ] );
		}

		for ( int i = 0; i < 10; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "BeefSteakMince500" ] );
		}

		for ( int i = 0; i < 10; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "SmokedBackBacon10" ] );
		}

		for ( int i = 0; i < 10; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "UnsmokedBackBacon10" ] );
		}

		for ( int i = 0; i < 3; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "LargeWholeChicken" ] );
		}

		for ( int i = 0; i < 6; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "GarlicBaguettes2" ] );
		}

		for ( int i = 0; i < 6; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "PepperoniPizza309" ] );
		}

		for ( int i = 0; i < 6; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "CheeseFeastPizza341" ] );
		}

		for ( int i = 0; i < 20; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "CocaCola1750" ] );
		}
		#endregion
		m_world.PlaceFurnitureInWorldWithStock ( "Stockcage", m_world.GetTileAt ( 3, 14 ), stockToAdd, 3 );
		stockToAdd.Clear();
		#region StockToAddDefined

		for ( int i = 0; i < 12; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "BakedBeans4" ] );
		}

		for ( int i = 0; i < 50; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "TomatoSoup400" ] );
		}

		for ( int i = 0; i < 50; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "ChocolateBiscuit8" ] );
		}

		for ( int i = 0; i < 50; i++ )
		{
			stockToAdd.Add ( m_world.m_stockPrototypes [ "JaffaCakes2" ] );
		}

		#endregion
		m_world.PlaceFurnitureInWorldWithStock ( "Stockcage", m_world.GetTileAt ( 4, 17 ), stockToAdd );
		m_world.PlaceFurnitureInWorld ( "Trolley", m_world.GetTileAt ( 6, 4), 2 );
		m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( 6, 5 ) );
		m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( 7, 5 ) );
		m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( 3, 5 ) );
		m_world.PlaceFurnitureInWorld ( "Wall", m_world.GetTileAt ( 2, 5 ) );
		m_world.PlaceFurnitureInWorld ( "Door", m_world.GetTileAt ( 4, 3 ) );
		m_world.PlaceFurnitureInWorld ( "Door", m_world.GetTileAt ( 4, 19 ) );
		m_world.CreateEmployee("James", 10000, m_world.GetTileAt ( 1, 1 ), "Manager");
		//m_world.CreateEmployee("John", 10000, m_world.GetTileAt ( 2, 1 ), "Assistant Manager");
	}

}
