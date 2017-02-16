using UnityEngine;
using System.Collections.Generic;
using System;

public class World {

	public bool m_worldSetUp { get; protected set; }

	Tile[,] m_tiles;

	public List<Character> m_characters { get; protected set; } //List of all characters in the world.
	public List<Employee> m_employees { get; protected set; } //List of all employees in the world.

	public int m_width  { get; protected set; }

	public int m_height { get; protected set; }

	public int m_customersInStore;

	public int m_customersInQueue;

	public int m_numberOfMannedTills;

	public Path_TileGraph m_tileGraph;

	/// <summary>
	/// List of all furniture in the world, ordered by first placed.
	///	Can be used to find out if a specific piece of furniture is in the game.
	/// </summary>
	public List<Furniture> m_furnitures { get; protected set; }

	/// <summary>
	/// Dictionary of all furniture in the world, sorted and grouped by full name.
	/// </summary>
	public Dictionary<string, List<Furniture> > m_furnitureInWorld;

	/// <summary>
	/// Dictionary of all furniture avaliable in the game, look up by full name.
	/// </summary>
	/// <value>The m furniture prototypes.</value>
	public Dictionary<string, Furniture> m_furniturePrototypes { get; protected set; }

	/// <summary>
	/// Dictionary of all Stock araliable in the game, look up by full name.
	/// </summary>
	public Dictionary<string, Stock> m_stockPrototypes { get; protected set; }

	/// <summary>
	/// Dictionary of all Stock the shop currently sells, sorted and grouped by name.
	/// </summary>
	public Dictionary<string, List<Stock> > m_stockInWorld { get; protected set; } //TODO this should only be avaliable to characters if
																				   //they have access to the manager's computer with that
																				   //information on, or possibly any computer in the 
																				   //store connected to the intranet.


	/// <summary>
	/// All character's that are using a piece of furniture, look up by used furniture.
	/// </summary>
	public Dictionary<Furniture, Character> m_characterFurniture; //This stores each character and which furniture they are currently using.
																  //If a character is not in this dictionary, its means they aren't using a piece of
																  //furniture.
																												

	public Action<Furniture> cbFurnitureCreated;
	public Action<Furniture> cbFurnitureMoved;
	public Action<Character> cbCharacterCreated;

	public World (int _width = 50, int _height = 50){

		this.m_width = _width;
		this.m_height = _height;

		m_tiles = new Tile[_width, _height];

		for	( int _x = 0; _x < _width; _x++ )
		{
			for ( int _y = 0; _y < _height; _y++ )
			{
				m_tiles[_x, _y] = new Tile ( this, _x, _y);
			}
		}

		CreateFurniturePrototypes ();
		CreateStockPrototypes();

		m_furnitures = new List<Furniture> ();
		m_furnitureInWorld = new Dictionary<string, List<Furniture> > ();
		m_characters = new List<Character> ();
		m_employees = new List<Employee> ();
		m_characterFurniture = new Dictionary<Furniture, Character>();

		m_numberOfMannedTills = 0;

		//Until customers are added to the game, the amount of customers in store needs to be hardcoded so the employee AI can be tested
		m_customersInStore = 3;
		m_customersInQueue = 2;
		}

	//Returns a Tile at a certain coordinate
	public Tile GetTileAt ( int _x, int _y )
	{
		//Need to check to see if the tile that is being asked for is actually within the bounds of the world.
		if ( _x >= m_width || _x < 0 || _y >= m_height || _y < 0 )
		{
			//Debug.LogError("Asked for a tile that is outside the world area");
			return null;
		}

		return m_tiles[_x,_y];
	}

	public void Update ( float _deltaTime )
	{
		foreach ( Character c in m_characters )
		{
			c.Update(_deltaTime);
		}
		foreach ( Furniture f in m_furnitures )
		{
			f.Update(_deltaTime);
		}
	}

	public Employee CreateEmployee ( string _name, int _maxCarryWeight, Tile _tile, string _title )
	{
		if ( _tile.m_furniture != null )
		{
			//Cannot spawn character here because there is a piece of furniture here.
			Debug.LogError ( "Tried to spawn character: " + _name + " on tile: (" + _tile.X + ", " + _tile.Y + "), but couldn't because there is a piece of furniture in the way: " + _tile.m_furniture.m_name );
			return null;
		}

		Employee e = new Employee ( _name, _maxCarryWeight, _tile, _title );	
		if ( cbCharacterCreated != null )
		{
			cbCharacterCreated ( e );
		}

		m_characters.Add ( e );
		m_employees.Add ( e );

		return e;
	}

	void CreateFurniturePrototypes ()
	{

		m_furniturePrototypes = new Dictionary<string, Furniture> ();

		m_furniturePrototypes.Add ( "Wall", 
			new Furniture (
				"Wall",	// Name
				0, 		// Impassable
				1, 		// Width
				1,  	// Height
				true,   // Links to neighbour
				true,	// Draggable	
				0,      // Max Carry Weight
				false	// Movable 
			) 
		);

		m_furniturePrototypes.Add ( "Door", 
			new Furniture (
				"Door", // Name
				1, 		// Pathfinding Cost
				1, 		// Width
				1,  	// Height
				false, 	// Links to neighbour
				false,	// Draggable
				0,      // Max Carry Weight
				false	// Movable
			) 
		);
		
		m_furniturePrototypes [ "Door" ].m_furnParameters [ "m_openness" ] = 0;
		m_furniturePrototypes [ "Door" ].m_furnParameters [ "m_isOpening" ] = 0;
		m_furniturePrototypes [ "Door" ].m_updateActions += FurnitureActions.Door_UpdateAction;	
		
		m_furniturePrototypes [ "Door" ].m_isEnterable = FurnitureActions.Door_IsEnterable;

		m_furniturePrototypes.Add ( "Stockcage", 
			new Furniture (
				"Stockcage", // Name
				3, 			 // Pathfinding Cost
				1, 			 // Width
				1,  		 // Height
				false, 		 // Links to neighbour
				false,		 // Draggable
				75000,   	 // Max Carry Weight 
				true		 // Movable
			) 
		);

		m_furniturePrototypes["Stockcage"].m_furnParameters["m_currTile.X"] = 0;
		m_furniturePrototypes["Stockcage"].m_furnParameters["m_currTile.Y"] = 0;
		m_furniturePrototypes["Stockcage"].m_furnParameters["m_destTile.X"] = 0;
		m_furniturePrototypes["Stockcage"].m_furnParameters["m_destTile.Y"] = 0;
		m_furniturePrototypes["Stockcage"].m_updateActions += FurnitureActions.MovableFurn_UpdateAction;

		m_furniturePrototypes.Add ( "Checkout", 
			new Furniture (
				"Checkout",		// Name
				5, 				// Pathfinding Cost
				3, 				// Width
				1,  			// Height
				false, 			// Links to neighbour
				false,			// Draggable
				10000,    		// Max Carry Weight
				false			// Movable
			) 
		);

		m_furniturePrototypes.Add ( "Trolley", 
			new Furniture (
				"Trolley",		// Name
				2, 				// Pathfinding Cost
				1, 				// Width
				1,  			// Height
				false, 			// Links to neighbour
				false,			// Draggable
				20000,    		// Max Carry Weight
				true			// Movable
			) 
		);

		m_furniturePrototypes["Trolley"].m_furnParameters["m_currTile.X"] = 0;
		m_furniturePrototypes["Trolley"].m_furnParameters["m_currTile.Y"] = 0;
		m_furniturePrototypes["Trolley"].m_furnParameters["m_destTile.X"] = 0;
		m_furniturePrototypes["Trolley"].m_furnParameters["m_destTile.Y"] = 0;
		m_furniturePrototypes["Trolley"].m_updateActions += FurnitureActions.MovableFurn_UpdateAction;

		m_furniturePrototypes.Add ( "BackShelf", 
			new Furniture (
				"BackShelf",	// Name
				0, 		    	// Pathfinding Cost
				1, 				// Width
				1,  			// Height
				false, 			// Links to neighbour
				false,			// Draggable
				100000,    		// Max Carry Weight
				false			// Movable
			) 
		);

		m_furniturePrototypes.Add ( "FrontShelf", 
			new Furniture (
				"FrontShelf",	// Name
				0, 		    	// Pathfinding Cost
				1, 				// Width
				1,  			// Height
				false, 			// Links to neighbour
				false,			// Draggable
				100000,    		// Max Carry Weight
				false			// Movable
			) 
		);

		m_furniturePrototypes.Add ( "Fridge", 
			new Furniture (
				"Fridge",	    // Name
				0, 		    	// Pathfinding Cost
				1, 				// Width
				1,  			// Height
				false, 			// Links to neighbour
				false,			// Draggable
				75000,  		// Max Carry Weight
				false			// Movable
			) 
		);

		m_furniturePrototypes.Add ( "BigFridge", 
			new Furniture (
				"BigFridge",	// Name
				0, 		    	// Pathfinding Cost
				2, 				// Width
				1,  			// Height
				false, 			// Links to neighbour
				false,			// Draggable
				150000,   		// Max Carry Weight
				false			// Movable
			) 
		);

		m_furniturePrototypes.Add ( "Freezer", 
			new Furniture (
				"Freezer",	//Name
				0, 		    	// Pathfinding Cost
				1, 				// Width
				1,  			// Height
				false, 			// Links to neighbour
				false,			// Draggable
				75000,    		// Max Carry Weight
				false			// Movable
			) 
		);

		m_furniturePrototypes.Add ( "BigFreezer", 
			new Furniture (
				"BigFreezer",	// Name
				0, 		    	// Pathfinding Cost
				2, 				// Width
				1,  			// Height
				false, 			// Links to neighbour
				false,			// Draggable
				150000,   		// Max Carry Weight
				false			// Movable
			) 
		);

	}

	void CreateStockPrototypes ()
	{
		m_stockPrototypes = new Dictionary<string, Stock>();

		#region Fruit

		m_stockPrototypes.Add("Bananas5Pack",
			new Stock(
				"Bananas5Pack",    	 //IDName
				"Bananas 5 Pack",    //Name
				100, 		         //Weight
				80, 		         //Price
				Temperature.Room     //Temperature
			)

		);

		m_stockPrototypes.Add("Apples5Pack",
			new Stock(
				"Apples5Pack",    	 //IDName
				"Apples 5 Pack",     //Name
				100, 		         //Weight
				149, 		         //Price
				Temperature.Room     //Temperature
			)

		);

		m_stockPrototypes.Add("GreenSeedlessGrapes500",
			new Stock(
				"GreenSeedlessGrapes500",    //IDName
				"Green Seedless Grapes",     //Name
				500, 		        		 //Weight
				200, 		                 //Price
				Temperature.Room             //Temperature
			)

		);

		m_stockPrototypes.Add("RedSeedlessGrapes500",
			new Stock(
				"RedSeedlessGrapes500",      //IDName
				"Red Seedless Grapes",       //Name
				500, 		        		 //Weight
				200, 		                 //Price
				Temperature.Room             //Temperature
			)

		);

		m_stockPrototypes.Add("LimeSingle",
			new Stock(
				"LimeSingle",    	//IDName
				"Lime",     		//Name
				100, 		        //Weight
				30, 		        //Price
				Temperature.Room    //Temperature
			)

		);

		m_stockPrototypes.Add("LemonSingle",
			new Stock(
				"LemonSingle",    	//IDName
				"Lemon",     		//Name
				100, 		        //Weight
				30, 		        //Price
				Temperature.Room    //Temperature
			)

		);

		m_stockPrototypes.Add("MangoSingle",
			new Stock(
				"MangoSingle",    	//IDName
				"Mango",     		//Name
				500, 		        //Weight
				100, 		        //Price
				Temperature.Room    //Temperature
			)

		);

		m_stockPrototypes.Add("PearSingle",
			new Stock(
				"PearSingle",    	//IDName
				"Pear",     		//Name
				150, 		        //Weight
				38, 		        //Price
				Temperature.Room    //Temperature
			)

		);

		m_stockPrototypes.Add("OrangeSingle",
			new Stock(
				"OrangeSingle",    	//IDName
				"Orange",     		//Name
				130, 		        //Weight
				30, 		        //Price
				Temperature.Room    //Temperature
			)

		);

		m_stockPrototypes.Add("Lemon5Pack",
			new Stock(
				"Lemon5Pack",    	//IDName
				"Lemon 5 Pack",     //Name
				500, 		        //Weight
				150, 		        //Price
				Temperature.Room    //Temperature
			)

		);

		m_stockPrototypes.Add("PineappleSingle",
			new Stock(
				"PineappleSingle",  //IDName
				"Pineapple",     	//Name
				1360, 		        //Weight
				100, 		        //Price
				Temperature.Room    //Temperature
			)

		);

		m_stockPrototypes.Add("WatermelonSingle",
			new Stock(
				"WatermelonSingle", //IDName
				"Watermelon",     	//Name
				9000, 		        //Weight
				250, 		        //Price
				Temperature.Room    //Temperature
			)

		);

		m_stockPrototypes.Add("PassionFruit3Pack",
			new Stock(
				"PassionFruit3Pack", 	//IDName
				"Passion Fruit 3 Pack", //Name
				300, 		        	//Weight
				150, 		        	//Price
				Temperature.Room    	//Temperature
			)

		);

		m_stockPrototypes.Add("PineappleChucks400",
			new Stock(
				"PineappleChucks400", 	//IDName
				"Pineapple Chucks", //Name
				400, 		        	//Weight
				200, 		        	//Price
				Temperature.Room    	//Temperature
			)

		);

		m_stockPrototypes.Add("Mango3Pack",
			new Stock(
				"Mango3Pack", 		//IDName
				"Mango 3 Pack", 	//Name
				1500, 		        //Weight
				250, 		        //Price
				Temperature.Room    //Temperature
			)

		);

		m_stockPrototypes.Add("Pears550",
			new Stock(
				"Pears550", 		//IDName
				"Pear Pack", 		//Name
				550, 		        //Weight
				225, 		        //Price
				Temperature.Room    //Temperature
			)

		);

		m_stockPrototypes.Add("Strawberries300",
			new Stock(
				"Strawberries300", 		//IDName
				"Strawberries", 		//Name
				300, 		        	//Weight
				300, 		       	 	//Price
				Temperature.Room   	 	//Temperature
			)

		);

		m_stockPrototypes.Add("PineappleFingers500",
			new Stock(
				"PineappleFingers500", 		//IDName
				"Pineapple Fingers Pack", 	//Name
				500, 		        		//Weight
				250, 		       	 		//Price
				Temperature.Room   	 		//Temperature
			)

		);

		m_stockPrototypes.Add("WatermelonPack380",
			new Stock(
				"WatermelonPack380", 		//IDName
				"Watermelon Pack", 			//Name
				380, 		        		//Weight
				200, 		       	 		//Price
				Temperature.Room   	 		//Temperature
			)

		);

		m_stockPrototypes.Add("PlumSingle",
			new Stock(
				"PlumSingle", 		//IDName
				"Plum", 			//Name
				60, 		        //Weight
				22, 		        //Price
				Temperature.Room  	//Temperature
			)

		);

		#endregion

		#region Vegetables

		m_stockPrototypes.Add("CarrotSingle",
			new Stock(
				"CarrotSingle", 		//IDName
				"Carrot", 				//Name
				120, 		        	//Weight
				6, 		        		//Price
				Temperature.Room  		//Temperature
			)

		);

		m_stockPrototypes.Add("WhiteOnionSingle",
			new Stock(
				"WhiteOnionSingle", //IDName
				"White Onion", 		//Name
				120, 		       	//Weight
				16, 		        //Price
				Temperature.Room  	//Temperature
			)

		);

		m_stockPrototypes.Add("MushroomPack300",
			new Stock(
				"MushroomPack300", 		//IDName
				"Mushroom Pack", 		//Name
				300, 		        	//Weight
				200, 		        	//Price
				Temperature.Room  		//Temperature
			)

		);

		m_stockPrototypes.Add("SpringOnionPack100",
			new Stock(
				"SpringOnionPack100", 	//IDName
				"Spring Onion Pack", 	//Name
				100, 		        	//Weight
				49, 		        	//Price
				Temperature.Room  		//Temperature
			)

		);

		m_stockPrototypes.Add("PotatoesPack2500",
			new Stock(
				"PotatoesPack2500", 	//IDName
				"Potatoes Pack", 		//Name
				2500, 		        	//Weight
				200, 		        	//Price
				Temperature.Room  		//Temperature
			)

		);

		m_stockPrototypes.Add("RedOnionLoose",
			new Stock(
				"RedOnionLoose", 	//IDName
				"Red Onion", 		//Name
				120, 		        //Weight
				17, 		        //Price
				Temperature.Room  	//Temperature
			)

		);

		m_stockPrototypes.Add("BakingPotatoSingle",
			new Stock(
				"BakingPotatoSingle", 	//IDName
				"Baking Potato", 		//Name
				150, 		        	//Weight
				30, 		        	//Price
				Temperature.Room  		//Temperature
			)

		);

		m_stockPrototypes.Add("CauliflowerSingle",
			new Stock(
				"CauliflowerSingle", 	//IDName
				"Cauliflower", 			//Name
				850, 		        	//Weight
				100, 		        	//Price
				Temperature.Room  		//Temperature
			)

		);

		m_stockPrototypes.Add("BroccoliSingle",
			new Stock(
				"BroccoliSingle", 	//IDName
				"Broccoli", 		//Name
				900, 		        //Weight
				43, 		        //Price
				Temperature.Room  	//Temperature
			)

		);

		m_stockPrototypes.Add("SweetPotatoSingle",
			new Stock(
				"SweetPotatoSingle", //IDName
				"Sweet Potato", 	 //Name
				150, 		         //Weight
				33, 		         //Price
				Temperature.Room  	 //Temperature
			)

		);

		m_stockPrototypes.Add("ParsnipSingle",
			new Stock(
				"ParsnipSingle", 	 //IDName
				"Parsnip", 			 //Name
				100, 		         //Weight
				19, 		         //Price
				Temperature.Room  	 //Temperature
			)

		);

		m_stockPrototypes.Add("GreenBeans220",
			new Stock(
				"GreenBeans220", 	 //IDName
				"Green Beans Pack",	 //Name
				220, 		         //Weight
				100, 		         //Price
				Temperature.Room  	 //Temperature
			)

		);

		m_stockPrototypes.Add("BabyButtonMushrooms250",
			new Stock(
				"BabyButtonMushrooms250", 	 	//IDName
				"Baby Button Mushrooms Pack",	//Name
				250, 		         			//Weight
				100, 		         			//Price
				Temperature.Room  	 			//Temperature
			)

		);

		m_stockPrototypes.Add("LeekSingle",
			new Stock(
				"LeekSingle", 	 	//IDName
				"Leek",				//Name
				100, 		        //Weight
				100, 		        //Price
				Temperature.Room  	//Temperature
			)

		);

		m_stockPrototypes.Add("GarlicSingle",
			new Stock(
				"GarlicSingle", 	 //IDName
				"Garlic",			 //Name
				24, 		         //Weight
				30, 		         //Price
				Temperature.Room  	 //Temperature
			)

		);

		m_stockPrototypes.Add("CourgetteLoose",
			new Stock(
				"CourgetteLoose", 	 //IDName
				"Courgette",		 //Name
				150, 		         //Weight
				38, 		         //Price
				Temperature.Room  	 //Temperature
			)

		);

		m_stockPrototypes.Add("BabySpinach240",
			new Stock(
				"BabySpinach240", 	 //IDName
				"Baby Spinach Pack", //Name
				240, 		         //Weight
				150, 		         //Price
				Temperature.Room  	 //Temperature
			)

		);

		m_stockPrototypes.Add("RootGingerSingle",
			new Stock(
				"RootGingerSingle",  //IDName
				"Root Ginger",		 //Name
				30, 		         //Weight
				135, 		         //Price
				Temperature.Room  	 //Temperature
			)

		);

		m_stockPrototypes.Add("CucumberSingle",
			new Stock(
				"CucumberSingle",  	 //IDName
				"Cucumber",			 //Name
				300, 		         //Weight
				50, 		         //Price
				Temperature.Room  	 //Temperature
			)

		);

		m_stockPrototypes.Add("MixedPeppers3Pack",
			new Stock(
				"MixedPeppers3Pack", //IDName
				"Mixed Peppers",     //Name
				1350, 		         //Weight
				96, 		         //Price
				Temperature.Room  	 //Temperature
			)

		);

		m_stockPrototypes.Add("SaladTomatoes360",
			new Stock(
				"SaladTomatoes360", 	//IDName
				"Salad Tomatoes Pack",  //Name
				360, 		         	//Weight
				150, 		         	//Price
				Temperature.Room  	 	//Temperature
			)

		);

		m_stockPrototypes.Add("IcebergLettuceSingle",
			new Stock(
				"IcebergLettuceSingle", 	//IDName
				"Iceberg Lettuce",  		//Name
				500, 		         		//Weight
				75, 		         		//Price
				Temperature.Room  	 		//Temperature
			)

		);

		m_stockPrototypes.Add("Chillies60",
			new Stock(
				"Chillies60", 				//IDName
				"Chillies Pack",  			//Name
				60, 		         		//Weight
				60, 		         		//Price
				Temperature.Room  	 		//Temperature
			)

		);

		#endregion

		#region Chilled Dairy

		m_stockPrototypes.Add("SMilk4pt",
			new Stock(
				"SMilk", 			//IDName
				"Skimmed Milk", 	//Name
				2272, 		        //Weight
				100, 		        //Price
				Temperature.Chilled	//Temperature
			)

		);

		m_stockPrototypes.Add("SSMilk4pt",
			new Stock(
				"SSMilk", 				//IDName
				"Semi Skimmed Milk", 	//Name
				2272, 		         	//Weight
				100, 		         	//Price
				Temperature.Chilled	 	//Temperature
			)

		);

		m_stockPrototypes.Add("WMilk4pt",
			new Stock(
				"WMilk", 				//IDName
				"Whole Milk", 	//Name
				2272, 		         	//Weight
				100, 		         	//Price
				Temperature.Chilled	 	//Temperature
			)

		);

		m_stockPrototypes.Add("MediumEggs6",
			new Stock(
				"MediumEggs6", 		//IDName
				"Medium Eggs", 		//Name
				378, 		        //Weight
				89, 		        //Price
				Temperature.Chilled	//Temperature
			)


		);

		m_stockPrototypes.Add("MediumEggs12",
			new Stock(
				"MediumEggs12", 	//IDName
				"Medium Eggs", 		//Name
				756, 		        //Weight
				175, 		        //Price
				Temperature.Chilled	//Temperature
			)


		);

		m_stockPrototypes.Add("LargeEggs6",
			new Stock(
				"LargeEggs6", 		//IDName
				"Large Eggs", 		//Name
				438, 		        //Weight
				110, 		        //Price
				Temperature.Chilled	//Temperature
			)


		);

		m_stockPrototypes.Add("LargeEggs12",
			new Stock(
				"LargeEggs12", 		//IDName
				"Large Eggs", 		//Name
				876, 		        //Weight
				225, 		        //Price
				Temperature.Chilled	//Temperature
			)


		);

		m_stockPrototypes.Add("SMilk2pt",
			new Stock(
				"SMilk2pt", 		//IDName
				"Skimmed Milk", 	//Name
				1136, 		        //Weight
				75, 		        //Price
				Temperature.Chilled	//Temperature
			)

		);

		m_stockPrototypes.Add("SSMilk2pt",
			new Stock(
				"SSMilk2pt", 			//IDName
				"Semi Skimmed Milk", 	//Name
				1136, 		        	//Weight
				75, 		        	//Price
				Temperature.Chilled		//Temperature
			)

		);

		m_stockPrototypes.Add("WMilk2pt",
			new Stock(
				"WMilk2pt", 		//IDName
				"Whole Milk", 		//Name
				1136, 		        //Weight
				75, 		        //Price
				Temperature.Chilled	//Temperature
			)

		);

		m_stockPrototypes.Add("SMilk1pt",
			new Stock(
				"SMilk1pt", 		//IDName
				"Skimmed Milk", 	//Name
				568, 		        //Weight
				45, 		        //Price
				Temperature.Chilled	//Temperature
			)

		);

		m_stockPrototypes.Add("SSMilk1pt",
			new Stock(
				"SSMilk1pt", 			//IDName
				"Semi Skimmed Milk", 	//Name
				568, 		        	//Weight
				45, 		        	//Price
				Temperature.Chilled		//Temperature
			)

		);

		m_stockPrototypes.Add("WMilk1pt",
			new Stock(
				"WMilk1pt", 		//IDName
				"Whole Milk", 		//Name
				568, 		        //Weight
				45, 		        //Price
				Temperature.Chilled	//Temperature
			)

		);

		m_stockPrototypes.Add("SaltedButter250",
			new Stock(
				"SaltedButter250", 	//IDName
				"Salted Butter", 	//Name
				250, 		        //Weight
				109, 		        //Price
				Temperature.Chilled	//Temperature
			)

		);

		m_stockPrototypes.Add("UnsaltedButter250",
			new Stock(
				"UnsaltedButter250", //IDName
				"Unsalted Butter", 	 //Name
				250, 		         //Weight
				109, 		         //Price
				Temperature.Chilled	 //Temperature
			)

		);

		m_stockPrototypes.Add("DoubleCream300",
			new Stock(
				"DoubleCream300", 	 //IDName
				"Double Cream", 	 //Name
				300, 		         //Weight
				95, 		         //Price
				Temperature.Chilled	 //Temperature
			)

		);

		m_stockPrototypes.Add("DoubleCream600",
			new Stock(
				"DoubleCream600", 	 //IDName
				"Double Cream", 	 //Name
				600, 		         //Weight
				160, 		         //Price
				Temperature.Chilled	 //Temperature
			)

		);

		m_stockPrototypes.Add("SlightySaltedSpreadable500",
			new Stock(
				"SlightySaltedSpreadable500", 	//IDName
				"Slighty Salted Spreadable", 	//Name
				500, 		         			//Weight
				150, 		         			//Price
				Temperature.Chilled	 			//Temperature
			)

		);

		m_stockPrototypes.Add("OriginalSpread500",
			new Stock(
				"OriginalSpread500", 	//IDName
				"Original Spread", 		//Name
				500, 		         	//Weight
				150, 		         	//Price
				Temperature.Chilled	 	//Temperature
			)

		);

		m_stockPrototypes.Add("DoubleCream150",
			new Stock(
				"DoubleCream150", 	//IDName
				"Double Cream", 	//Name
				150, 		        //Weight
				60, 		        //Price
				Temperature.Chilled	//Temperature
			)

		);

		m_stockPrototypes.Add("PuffPastryRolled320",
			new Stock(
				"PuffPastryRolled320", 	//IDName
				"Puff Pastry Rolled", 	//Name
				320, 		        	//Weight
				150, 		        	//Price
				Temperature.Chilled		//Temperature
			)

		);

		m_stockPrototypes.Add("GooseFat200",
			new Stock(
				"GooseFat200", 			//IDName
				"Goose Fat", 			//Name
				200, 		        	//Weight
				200, 		        	//Price
				Temperature.Chilled		//Temperature
			)

		);

		m_stockPrototypes.Add("CornishCustard500",
			new Stock(
				"CornishCustard500", 	//IDName
				"Cornish Custard", 		//Name
				500, 		        	//Weight
				180, 		        	//Price
				Temperature.Chilled		//Temperature
			)

		);

		m_stockPrototypes.Add("MatureCheddarBlock350",
			new Stock(
				"MatureCheddarBlock350", 	//IDName
				"Mature Cheddar Block", 		//Name
				350, 		        	//Weight
				350, 		        	//Price
				Temperature.Chilled		//Temperature
			)

		);

		m_stockPrototypes.Add("MatureCheddarBlock550",
			new Stock(
				"MatureCheddarBlock550", 	//IDName
				"Mature Cheddar Block", 		//Name
				550, 		        	//Weight
				500, 		        	//Price
				Temperature.Chilled		//Temperature
			)

		);

		m_stockPrototypes.Add("GratedCheddar500",
			new Stock(
				"GratedCheddar500", 	//IDName
				"Grated Cheddar", 		//Name
				500, 		        	//Weight
				250, 		        	//Price
				Temperature.Chilled		//Temperature
			)

		);

		m_stockPrototypes.Add("MozzarellaBlock150",
			new Stock(
				"MozzarellaBlock150", 	//IDName
				"Mozzarella Block", 	//Name
				150, 		        	//Weight
				70, 		        	//Price
				Temperature.Chilled		//Temperature
			)

		);

		m_stockPrototypes.Add("GratedMozzarella250",
			new Stock(
				"GratedMozzarella250", 	//IDName
				"Grated Mozzarella", 	//Name
				250, 		       	 	//Weight
				200, 		        	//Price
				Temperature.Chilled		//Temperature
			)

		);

		#endregion

		#region ChilledDrinks1

		m_stockPrototypes.Add("BananaMilkshake400",
			new Stock(
				"BananaMilkshake400", 	//IDName
				"Banana Milkshake", 	//Name
				400, 		        	//Weight
				100, 		        	//Price
				Temperature.Chilled		//Temperature
			)

		);

		m_stockPrototypes.Add("ChocolateMilkshake400",
			new Stock(
				"ChocolateMilkshake400", //IDName
				"Chocolate Milkshake", 	 //Name
				400, 		        	 //Weight
				100, 		        	 //Price
				Temperature.Chilled		 //Temperature
			)

		);

		m_stockPrototypes.Add("StrawberryMilkshake400",
			new Stock(
				"StrawberryMilkshake400", 	//IDName
				"Strawberry Milkshake", 	//Name
				400, 		        	 	//Weight
				100, 		        	 	//Price
				Temperature.Chilled		 	//Temperature
			)

		);

		#endregion

		#region ChilledDesserts

		m_stockPrototypes.Add("MilkChocolateMousse6",
			new Stock(
				"MilkChocolateMousse6", 	//IDName
				"Milk Chocolate Mousse Pack", 	//Name
				360, 		       	 	//Weight
				90, 		        	//Price
				Temperature.Chilled		//Temperature
			)

		);

		m_stockPrototypes.Add("StrawberryTrifle3",
			new Stock(
				"StrawberryTrifle3", 		//IDName
				"Strawberry Trifle Triple", //Name
				435, 		       	 		//Weight
				160, 		        		//Price
				Temperature.Chilled			//Temperature
			)

		);

		m_stockPrototypes.Add("VanillaCheesecake540",
			new Stock(
				"VanillaCheesecake540", 	//IDName
				"Vanilla Cheesecake",		//Name
				540, 		       	 		//Weight
				300, 		        		//Price
				Temperature.Chilled			//Temperature
			)

		);

		m_stockPrototypes.Add("ChocolateCheesecake540",
			new Stock(
				"ChocolateCheesecake540", 	//IDName
				"Chocolate Cheesecake",		//Name
				540, 		       	 		//Weight
				300, 		        		//Price
				Temperature.Chilled			//Temperature
			)

		);

		m_stockPrototypes.Add("TreacleTart380",
			new Stock(
				"TreacleTart380", 	//IDName
				"Treacle Tart",		//Name
				380, 		       	//Weight
				250, 		        //Price
				Temperature.Chilled //Temperature
			)

		);

		m_stockPrototypes.Add("LemonTart385",
			new Stock(
				"LemonTart385", 	//IDName
				"Lemon Tart",		//Name
				385, 		       	//Weight
				250, 		        //Price
				Temperature.Chilled //Temperature
			)

		);

		#endregion

		#region ChilledMeat

		m_stockPrototypes.Add("ChickenBreast950",
			new Stock(
				"ChickenBreast950", 		//IDName
				"Chicken Breast Portions",	//Name
				950, 		       			//Weight
				550, 		        		//Price
				Temperature.Chilled 		//Temperature
			)

		);

		m_stockPrototypes.Add("BeefSteakMince500",
			new Stock(
				"BeefSteakMince500", 	//IDName
				"Beef Steak Mince",		//Name
				500, 		       		//Weight
				400, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("SmokedBackBacon10",
			new Stock(
				"SmokedBackBacon10", 	//IDName
				"Smoked Back Bacon",	//Name
				300, 		       		//Weight
				200, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("UnsmokedBackBacon10",
			new Stock(
				"UnsmokedBackBacon10", 	//IDName
				"Unsmoked Back Bacon",	//Name
				300, 		       		//Weight
				200, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("LargeWholeChicken",
			new Stock(
				"LargeWholeChicken", 	//IDName
				"Large Whole Chicken",	//Name
				1650, 		       		//Weight
				450, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("ThickPorkSausages12",
			new Stock(
				"ThickPorkSausages12", 	//IDName
				"Thick Pork Sausages",	//Name
				681, 		       		//Weight
				300, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("GarlicChickenKiev2",
			new Stock(
				"GarlicChickenKiev2", 	//IDName
				"Garlic Chicke Kiev Twin",	//Name
				305, 		       		//Weight
				200, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("BreadedChickenGoujons270",
			new Stock(
				"BreadedChickenGoujons270", 	//IDName
				"Breaded Chicke Goujons",	//Name
				270, 		       		//Weight
				200, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("DicedBeef600",
			new Stock(
				"DicedBeef600", 	//IDName
				"Diced Beef",	//Name
				600, 		       		//Weight
				500, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("BeefSteakBurgers4",
			new Stock(
				"BeefSteakBurgers4", 	//IDName
				"Beef Steak Burgers",	//Name
				454, 		       		//Weight
				300, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		#endregion

		#region ChilledMealContents

		m_stockPrototypes.Add("GarlicBaguettes2",
			new Stock(
				"GarlicBaguettes2", 	//IDName
				"Garlic Baguettes Twin",	//Name
				430, 		       		//Weight
				100, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("PepperoniPizza309",
			new Stock(
				"PepperoniPizza309", 	//IDName
				"Pepperoni Pizza",	//Name
				309, 		       		//Weight
				200, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("CheeseFeastPizza341",
			new Stock(
				"CheeseFeastPizza341", 	//IDName
				"Cheese Feast Pizza",	//Name
				341, 		       		//Weight
				200, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("HamPineapplePizza310",
			new Stock(
				"HamPineapplePizza310", 	//IDName
				"Ham and Pineapple Pizza",	//Name
				310, 		       		//Weight
				200, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("ChickenTikkaMasalaPilauRice450",
			new Stock(
				"ChickenTikkaMasalaPilauRice450", 	//IDName
				"Chicken Tikka Masala with Pilau Rice",	//Name
				450, 		       		//Weight
				230, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("BeefLasagne450",
			new Stock(
				"BeefLasagne450", 	//IDName
				"Beef Lasagne",	//Name
				450, 		       		//Weight
				230, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("CottagePie450",
			new Stock(
				"CottagePie450", 	//IDName
				"Cottage Pie",	//Name
				450, 		       		//Weight
				230, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		#endregion

		#region FrozenMeat

		m_stockPrototypes.Add("ChickenDippers42",
			new Stock(
				"ChickenDippers42", 	//IDName
				"Chicken Dippers",		//Name
				770, 		       		//Weight
				400, 		        	//Price
				Temperature.Frozen 	//Temperature
			)

		);

		m_stockPrototypes.Add("ChickenBreastFillet1000",
			new Stock(
				"ChickenBreastFillet1000", 	//IDName
				"Chicken Breast Fillet",	//Name
				1000, 		       			//Weight
				250, 		        		//Price
				Temperature.Frozen 			//Temperature
			)

		);

		m_stockPrototypes.Add("SouthernFriedChicken2",
			new Stock(
				"SouthernFriedChicken2", 		//IDName
				"Southern Fried Chicken Twin",	//Name
				200, 		       				//Weight
				150, 		        			//Price
				Temperature.Frozen 				//Temperature
			)	

		);

		m_stockPrototypes.Add("PorkSausages20",
			new Stock(
				"PorkSausages20", 		//IDName
				"Pork Sausages",		//Name
				1000, 		       		//Weight
				109, 		        	//Price
				Temperature.Frozen 		//Temperature
			)	

		);

		m_stockPrototypes.Add("GarlicButterBreadedChickenKievs4",
			new Stock(
				"GarlicButterBreadedChickenKievs4", 	//IDName
				"Garlic Butter Breaded Chicken Kievs",	//Name
				500, 		       						//Weight
				200, 		        					//Price
				Temperature.Frozen 						//Temperature
			)	

		);

		m_stockPrototypes.Add("CrispyChicken5",
			new Stock(
				"CrispyChicken5", 	//IDName
				"Crispy Chicken",	//Name
				450, 		       	//Weight
				300, 		        //Price
				Temperature.Frozen 	//Temperature
			)	

		);

		m_stockPrototypes.Add("ChickenQuarterPounders4",
			new Stock(
				"ChickenQuarterPounders4", 	//IDName
				"Chicken Quarter Pounders",	//Name
				454, 		      		 	//Weight
				275, 		        		//Price
				Temperature.Frozen 			//Temperature
			)	

		);

		m_stockPrototypes.Add("BeefBurgers4",
			new Stock(
				"BeefBurgers4", 	//IDName
				"Beef Burgers",		//Name
				227, 		      	//Weight
				149, 		        //Price
				Temperature.Frozen 	//Temperature
			)	

		);

		m_stockPrototypes.Add("ChickenFingers14",
			new Stock(
				"ChickenFingers14", 	//IDName
				"Chicken Fingers",		//Name
				350, 		      	//Weight
				150, 		        //Price
				Temperature.Frozen 	//Temperature
			)	

		);

		#endregion

		#region FrozenMeals

		m_stockPrototypes.Add("Lasagne400",
			new Stock(
				"Lasagne400", 		//IDName
				"Lasagne",			//Name
				400, 		      	//Weight
				100, 		        //Price
				Temperature.Frozen 	//Temperature
			)	

		);

		m_stockPrototypes.Add("CottagePie400",
			new Stock(
				"CottagePie400", 	//IDName
				"Cottage Pie",		//Name
				400, 		      	//Weight
				100, 		        //Price
				Temperature.Frozen 	//Temperature
			)	

		);

		m_stockPrototypes.Add("ChilliConCarneRice400",
			new Stock(
				"ChilliConCarneRice400", 		//IDName
				"Chilli Con Carne and Rice",	//Name
				400, 		      				//Weight
				100, 		        			//Price
				Temperature.Frozen 				//Temperature
			)	

		);

		m_stockPrototypes.Add("SweetSourChicken400",
			new Stock(
				"SweetSourChicken400", 		//IDName
				"Sweet and Sour Chicken",	//Name
				400, 		      			//Weight
				100, 		        		//Price
				Temperature.Frozen 			//Temperature
			)	

		);

		m_stockPrototypes.Add("BangersMash400",
			new Stock(
				"BangersMash400", 		//IDName
				"Bangers and Mash",		//Name
				400, 		      		//Weight
				100, 		        	//Price
				Temperature.Frozen 		//Temperature
			)	

		);

		#endregion

		#region FrozenPotatoProducts

		m_stockPrototypes.Add("HomeChips1500",
			new Stock(
				"HomeChips1500", 		//IDName
				"Home Chips",			//Name
				1500, 		      		//Weight
				250, 		        	//Price
				Temperature.Frozen 		//Temperature
			)	

		);

		m_stockPrototypes.Add("OvenChips1500",
			new Stock(
				"OvenChips1500", 		//IDName
				"Oven Chips",			//Name
				1500, 		      		//Weight
				150, 		        	//Price
				Temperature.Frozen 		//Temperature
			)	

		);

		m_stockPrototypes.Add("PotatoWaffles10",
			new Stock(
				"PotatoWaffles10", 		//IDName
				"Potato Waffles",		//Name
				567, 		      		//Weight
				148, 		        	//Price
				Temperature.Frozen 		//Temperature
			)	

		);

		m_stockPrototypes.Add("ExtraChunkyHomeChips1000",
			new Stock(
				"ExtraChunkyHomeChips1000", 	//IDName
				"Extra Chunky Home Chips",		//Name
				1000, 		      				//Weight
				260, 		        			//Price
				Temperature.Frozen 				//Temperature
			)	

		);

		m_stockPrototypes.Add("BatteredOnionRings375",
			new Stock(
				"BatteredOnionRings375", 	//IDName
				"Battered Onion Rings",		//Name
				375, 		      			//Weight
				200, 		        		//Price
				Temperature.Frozen 			//Temperature
			)

		);

		#endregion

		#region FrozenMealContents

		m_stockPrototypes.Add("FourCheesePizza330",
			new Stock(
				"FourCheesePizza330", 	//IDName
				"Four Cheese Pizza",	//Name
				330, 		      		//Weight
				150, 		        	//Price
				Temperature.Frozen 		//Temperature
			)

		);

		m_stockPrototypes.Add("DoublePepperoniPizza330",
			new Stock(
				"DoublePepperoniPizza330", 	//IDName
				"Double Pepperoni Pizza",	//Name
				330, 		      			//Weight
				150, 		        		//Price
				Temperature.Frozen 			//Temperature
			)

		);

		m_stockPrototypes.Add("GarlicCheesePizzaBread210",
			new Stock(
				"GarlicCheesePizzaBread210", 		//IDName
				"Garlic and Cheese Pizza Bread",	//Name
				210, 		      					//Weight
				100, 		        				//Price
				Temperature.Frozen 					//Temperature
			)

		);

		m_stockPrototypes.Add("BreadedOnionRings750",
			new Stock(
				"BreadedOnionRings750", 	//IDName
				"Breaded Onion Rings",		//Name
				750, 		      		//Weight
				115, 		        	//Price
				Temperature.Frozen 		//Temperature
			)

		);

		#endregion

		#region FrozenVegetables

		m_stockPrototypes.Add("GardenPeas1000",
			new Stock(
				"GardenPeas1000", 		//IDName
				"Garden Peas",			//Name
				1000, 		      		//Weight
				135, 		        	//Price
				Temperature.Frozen 		//Temperature
			)

		);

		m_stockPrototypes.Add("BroccoliFlorets900",
			new Stock(
				"BroccoliFlorets900", 	//IDName
				"Broccoli Florets",		//Name
				900, 		      		//Weight
				110, 		        	//Price
				Temperature.Frozen 		//Temperature
			)

		);

		m_stockPrototypes.Add("DicedOnions500",
			new Stock(
				"DicedOnions500", 	//IDName
				"Diced Onions",		//Name
				500, 		      		//Weight
				100, 		        	//Price
				Temperature.Frozen 		//Temperature
			)

		);

		m_stockPrototypes.Add("MixedPeppers500",
			new Stock(
				"MixedPeppers500", 		//IDName
				"Mixed Peppers",		//Name
				500, 		      		//Weight
				100, 		        	//Price
				Temperature.Frozen 		//Temperature
			)

		);

		m_stockPrototypes.Add("Sweetcorn907",
			new Stock(
				"Sweetcorn907", 		//IDName
				"Sweetcorn",			//Name
				907, 		      		//Weight
				99, 		        	//Price
				Temperature.Frozen 		//Temperature
			)

		);

		m_stockPrototypes.Add("SlicedCarrots1000",
			new Stock(
				"SlicedCarrots1000", 	//IDName
				"Sliced Carrots",		//Name
				1000,		      		//Weight
				100, 		        	//Price
				Temperature.Frozen 		//Temperature
			)

		);

		#endregion

		#region RoomTempDrinks

		m_stockPrototypes.Add("CocaCola1750",
			new Stock(
				"CocaCola1750", 	//IDName
				"Coca Cola",		//Name
				1750, 		      	//Weight
				166, 		        //Price
				Temperature.Room 	//Temperature
			)

		);

		m_stockPrototypes.Add("CocaColaDiet1750",
			new Stock(
				"CocaColaDiet1750", 	//IDName
				"Coca Cola Diet",		//Name
				1750, 		      		//Weight
				166, 		        	//Price
				Temperature.Room 		//Temperature
			)

		);

		m_stockPrototypes.Add("Lemonade2000",
			new Stock(
				"Lemonade2000", 	//IDName
				"Lemonade",			//Name
				2000, 		      	//Weight
				53, 		        //Price
				Temperature.Room 	//Temperature
			)

		);

		m_stockPrototypes.Add("LemonadeDiet2000",
			new Stock(
				"LemonadeDiet2000", 	//IDName
				"Lemonade Diet",		//Name
				2000, 		      		//Weight
				53, 		        	//Price
				Temperature.Room 		//Temperature
			)

		);

		m_stockPrototypes.Add("CocaCola24",
			new Stock(
				"CocaCola24", 			//IDName
				"CocaCola Multipack",	//Name
				7920, 		      		//Weight
				700, 		        	//Price
				Temperature.Room 		//Temperature
			)

		);

		m_stockPrototypes.Add("CocaColaDiet24",
			new Stock(
				"CocaColaDiet24", 			//IDName
				"CocaCola Diet Multipack",	//Name
				7920, 		      			//Weight
				700, 		        		//Price
				Temperature.Room 			//Temperature
			)

		);

		m_stockPrototypes.Add("FizzyOrange2000",
			new Stock(
				"FizzyOrange2000", 		//IDName
				"Fizzy Orange",			//Name
				2000, 		      		//Weight
				55, 		        	//Price
				Temperature.Room 		//Temperature
			)

		);

		m_stockPrototypes.Add("FizzyOrange6",
			new Stock(
				"FizzyOrange6", 			//IDName
				"Fizzy Orange Multipack",	//Name
				1980, 		      			//Weight
				355, 		        		//Price
				Temperature.Room 			//Temperature
			)

		);

		m_stockPrototypes.Add("Lemonade12",
			new Stock(
				"Lemonade12", 			//IDName
				"Lemonade Multipack",	//Name
				3000, 		      		//Weight
				240, 		        	//Price
				Temperature.Room 		//Temperature
			)

		);

		m_stockPrototypes.Add("LemonadeDiet12",
			new Stock(
				"LemonadeDiet12", 			//IDName
				"Lemonade Diet Multipack",	//Name
				3000, 		      			//Weight
				240, 		        		//Price
				Temperature.Room 			//Temperature
			)

		);

		#endregion

		#region ChilledJuice

		m_stockPrototypes.Add("OrangeJuice2000",
			new Stock(
				"OrangeJuice2000", 			//IDName
				"Orange Juice Carton",		//Name
				2000, 		      			//Weight
				79, 		        		//Price
				Temperature.Chilled 		//Temperature
			)

		);

		m_stockPrototypes.Add("OrangeJuice1000",
			new Stock(
				"OrangeJuice1000", 			//IDName
				"Orange Juice Carton",		//Name
				1000, 		      			//Weight
				79, 		        		//Price
				Temperature.Chilled 		//Temperature
			)

		);

		m_stockPrototypes.Add("AppleJuice2000",
			new Stock(
				"AppleJuice2000", 			//IDName
				"Apple Juice Carton",		//Name
				2000, 		      			//Weight
				79, 		        		//Price
				Temperature.Chilled 		//Temperature
			)

		);

		m_stockPrototypes.Add("AppleJuice1000",
			new Stock(
				"AppleJuice1000", 			//IDName
				"Apple Juice Carton",		//Name
				1000, 		      			//Weight
				79, 		        		//Price
				Temperature.Chilled 		//Temperature
			)

		);

		m_stockPrototypes.Add("CranberryJuice1000",
			new Stock(
				"CranberryJuice1000", 			//IDName
				"Cranberry Juice Carton",		//Name
				1000, 		      			//Weight
				79, 		        		//Price
				Temperature.Chilled 		//Temperature
			)

		);

		m_stockPrototypes.Add("PineappleJuice1000",
			new Stock(
				"PineappleJuice1000", 			//IDName
				"Pineapple Juice Carton",		//Name
				1000, 		      			//Weight
				79, 		        		//Price
				Temperature.Chilled 		//Temperature
			)

		);

		m_stockPrototypes.Add("AppleJuice3",
			new Stock(
				"AppleJuice3", 			//IDName
				"Apple Juice Triple",	//Name
				600, 		      		//Weight
				45, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("OrangeJuice3",
			new Stock(
				"OrangeJuice3", 		//IDName
				"Orange Juice Triple",	//Name
				600, 		      		//Weight
				45, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("PineappleJuice3",
			new Stock(
				"PineappleJuice3", 			//IDName
				"Pineapple Juice Triple",	//Name
				600, 		      			//Weight
				45, 		        		//Price
				Temperature.Chilled 		//Temperature
			)

		);

		#endregion

		#region RoomTempWater

		m_stockPrototypes.Add("StillWater12",
			new Stock(
				"StillWater12", 			//IDName
				"Bottled Still Water Pack",	//Name
				6000, 		      			//Weight
				229, 		        		//Price
				Temperature.Room 			//Temperature
			)

		);

		m_stockPrototypes.Add("SparklingWater2000",
			new Stock(
				"SparklingWater2000", 		//IDName
				"Sparkling Water",			//Name
				2000, 		      			//Weight
				17, 		        		//Price
				Temperature.Room 			//Temperature
			)

		);

		m_stockPrototypes.Add("StillWater2000",
			new Stock(
				"StillWater2000", 		//IDName
				"Still Water",			//Name
				2000, 		      			//Weight
				17, 		        		//Price
				Temperature.Room 			//Temperature
			)

		);

		#endregion

		#region ChilledDrinks

		m_stockPrototypes.Add("StillWater500",
			new Stock(
				"StillWater500", 		//IDName
				"Still Water",			//Name
				500, 		      		//Weight
				59, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("CokeCola500",
			new Stock(
				"CokeCola500", 			//IDName
				"Coke Cola",			//Name
				500, 		      		//Weight
				59, 		        	//Price
				Temperature.Chilled 	//Temperature
			)

		);

		m_stockPrototypes.Add("CokeColaDiet500",
			new Stock(
				"CokeColaDiet500", 			//IDName
				"Coke Cola Diet",			//Name
				500, 		      			//Weight
				59, 		        		//Price
				Temperature.Chilled 		//Temperature
			)

		);

		m_stockPrototypes.Add("FizzyOrange500",
			new Stock(
				"FizzyOrange500", 	//IDName
				"Fizzy Orange",		//Name
				500, 		      	//Weight
				59, 		        //Price
				Temperature.Chilled //Temperature
			)

		);

		#endregion

		#region Tins

		m_stockPrototypes.Add("BakedBeans4",
			new Stock(
				"BakedBeans4", 		//IDName
				"Baked Beans Pack",		//Name
				1660, 		      	//Weight
				269, 		        //Price
				Temperature.Room //Temperature
			)

		);

		m_stockPrototypes.Add("TomatoSoup400",
			new Stock(
				"TomatoSoup400", 		//IDName
				"Tomato Soup",			//Name
				400, 		      		//Weight
				95, 		       	 	//Price
				Temperature.Room 		//Temperature
			)

		);

		m_stockPrototypes.Add("ChoppedTomatoes4",
			new Stock(
				"ChoppedTomatoes4", 	//IDName
				"Chopped Tomatoes",		//Name
				1600, 		      		//Weight
				139, 		       	 	//Price
				Temperature.Room 		//Temperature
			)

		);

		m_stockPrototypes.Add("TunaChucks4",
			new Stock(
				"TunaChucks4", 				//IDName
				"Tuna Chucks in Brine",		//Name
				640, 		      			//Weight
				400, 		       	 		//Price
				Temperature.Room 			//Temperature
			)

		);

		#endregion

		#region Biscuits

		m_stockPrototypes.Add("ChocolateBiscuit8",
			new Stock(
				"ChocolateBiscuit8", 		//IDName
				"Chocolate Biscuit Pack",	//Name
				20, 		      			//Weight
				159, 		       	 		//Price
				Temperature.Room 			//Temperature
			)

		);

		m_stockPrototypes.Add("JaffaCakes2",
			new Stock(
				"JaffaCakes2", 		//IDName
				"Jaff aCakes Twin",	//Name
				300, 		      	//Weight
				100, 		       	//Price
				Temperature.Room 	//Temperature
			)

		);

		m_stockPrototypes.Add("ChocolateDigestives500",
			new Stock(
				"ChocolateDigestives500", 		//IDName
				"Chocolate Digestives",	//Name
				500, 		      	//Weight
				229, 		       	//Price
				Temperature.Room 	//Temperature
			)

		);

		#endregion

		#region Chocolate

		m_stockPrototypes.Add("MilkChocolateBar200",
			new Stock(
				"MilkChocolateBar200", 	//IDName
				"Milk Chocolate Bar",	//Name
				200, 		      		//Weight
				200, 		       		//Price
				Temperature.Room 		//Temperature
			)	

		);

		m_stockPrototypes.Add("MilkChocolateButtons119",
			new Stock(
				"MilkChocolateButtons119", 	//IDName
				"Milk Chocolate Buttons",	//Name
				119, 		      		//Weight
				150, 		       		//Price
				Temperature.Room 		//Temperature
			)	

		);

		m_stockPrototypes.Add("MarsBars4",
			new Stock(
				"MarsBars4", 			//IDName
				"Mars Bar Pack",		//Name
				157, 		      		//Weight
				150, 		       		//Price
				Temperature.Room 		//Temperature
			)	

		);

		m_stockPrototypes.Add("MinstrelsPouch130",
			new Stock(
				"MinstrelsPouch130", 	//IDName
				"Minstrels Pouch",		//Name
				130, 		      		//Weight
				150, 		       		//Price
				Temperature.Room 		//Temperature
			)	

		);

		#endregion

	}

	//Attempts to place a furniture onto a specified tile. If it fails this returns null, else it returns the instance of the furniture
	//that got placed.
	public Furniture PlaceFurnitureInWorld ( string _furnName, Tile _tile, int _direction = 1 )
	{
		if ( m_furniturePrototypes.ContainsKey ( _furnName ) == false )
		{
			Debug.LogError ( "m_furniturePrototypes doesn't contain a prototype for key: " + _furnName );
			return null;
		}

		Furniture furn = Furniture.PlaceInstanceOfFurniture ( m_furniturePrototypes [ _furnName ], _tile, _direction );

		if ( furn == null )
		{
			return null;
		}

		m_furnitures.Add ( furn );
		if ( m_furnitureInWorld.ContainsKey ( furn.m_name ) == false )
		{
			m_furnitureInWorld.Add ( furn.m_name, new List<Furniture> () );
		}
		m_furnitureInWorld[furn.m_name].Add(furn);

		if ( cbFurnitureCreated != null )
		{
			cbFurnitureCreated(furn);
			InvalidateTileGraph();
		}

		furn.m_furnParameters["m_currTile.X"] = _tile.X;
		furn.m_furnParameters["m_currTile.Y"] = _tile.Y;

		return furn;
	}

	//Attempts to place a furniture onto a specified tile with specified stock. If it fails this returns null, else it returns the instance of the furniture
	//that got placed.
	public Furniture PlaceFurnitureInWorldWithStock ( string _furnName, Tile _tile, List<Stock> _stock, int _direction = 1)
	{
		Furniture furn = PlaceFurnitureInWorld ( _furnName, _tile, _direction );

		foreach ( Stock stock in _stock )
		{
			if ( furn.TryAddStock ( stock ) == false )
			{
				Debug.LogError("World tried to add stock to furniture but failed: Stock: " + stock.Name + ", Furniture " + furn.m_name);
				continue;
			}
		}

		return furn;
	}

	public bool MoveFurniture ( Furniture _furn, Tile _from, Tile _to )
	{
		if ( GetTileAt ( _to.X, _to.Y ).PlaceFurniture ( _furn ) )
		{
			GetTileAt ( _from.X, _from.Y ).RemoveFurniture ();
			return true;
		}
		else
		{
			return false;
		}
	}

	public bool PositionCheck ( Tile _tile, Furniture _furn, int _direction )
	{
		if ( _tile == null )
		{
			return false;
		}

		Furniture furn = _furn.Clone();
		furn.RotateFurniture(_direction);

		//This is for if the furniture is more than 1X1, and is used to check all the tiles for validity.
		for ( int x_off = _tile.X; x_off < ( _tile.X + furn.Width ); x_off++ )
		{
			for ( int y_off = _tile.Y; y_off < ( _tile.Y + furn.Height ); y_off++ )
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
		if ( furn.m_name == "Door" )
		{
			Furniture[] neighboursFurn = _tile.GetNeighboursFurniture ( false );

			if ( neighboursFurn [ 0 ] == null || neighboursFurn [ 0 ].m_name != "Wall" )
			{

				if ( neighboursFurn [ 1 ] == null || neighboursFurn [ 1 ].m_name != "Wall" )
				{
					return false;
				}
				else
				{
					if ( neighboursFurn [ 3 ] == null || neighboursFurn [ 3 ].m_name != "Wall" )
					{
						return false;
					}
				}
			}
			else
			{
				if ( neighboursFurn [ 2 ] == null || neighboursFurn [ 2 ].m_name != "Wall" )
				{
					return false;
				}
			}

		}
		return true;
	}

	public void InvalidateTileGraph()
	{
		m_tileGraph = null;
	}

	public void RegisterFurnitureCreated ( Action<Furniture> _callbackFunc )
	{
		cbFurnitureCreated += _callbackFunc;
	}

	public void UnregisterFurnitureCreated ( Action<Furniture> _callbackFunc )
	{
		cbFurnitureCreated -= _callbackFunc;
	}

	public void RegisterFurnitureMoved ( Action<Furniture> _callbackFunc )
	{
		cbFurnitureMoved += _callbackFunc;
	}

	public void unregisterFurnitureMoved ( Action<Furniture> _callbackFunc )
	{
		cbFurnitureMoved -= _callbackFunc;
	}

	public void RegisterCharacterCreated ( Action<Character> _callbackFunc )
	{
		cbCharacterCreated += _callbackFunc;
	}

	public void UnregisterCharacterCreated ( Action<Character> _callbackFunc )
	{
		cbCharacterCreated -= _callbackFunc;
	}
	
}
