using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour {

	//World position of the mouse last frame
	Vector3 m_lastFramePos;

	//World position of the mouse this frame
	Vector3 m_currFramePos;

	//World position at start  of left-mouse drag operation
	Vector3 m_dragStartPos;

	bool m_isDragging;

	public string m_mode { get; protected set; }

	string m_buildModeFurnName;

	GameObject m_furniturePreview;

	FurnitureSpriteController m_furnSpriteController;

	public GameObject m_menu;

	public GameObject[] m_menuButtonGOs;

	public Dictionary<string, GameObject> m_menuButtons;

	SelectDisplay m_selectDisplayScript;

	public GameObject m_selectDisplay;

	public GameObject m_stockDisplay;

	public Furniture m_selectedFurn;

	void Start()
	{
		m_furnSpriteController = GameObject.FindObjectOfType<FurnitureSpriteController> ();

		m_furniturePreview = new GameObject ();
		m_furniturePreview.name = "Furniture Preview";
		m_furniturePreview.transform.SetParent ( this.transform );
		m_furniturePreview.AddComponent<SpriteRenderer> ();
		m_furniturePreview.SetActive ( false );

		m_menuButtons = new Dictionary<string, GameObject> ();
		foreach ( GameObject go in m_menuButtonGOs )
		{
			m_menuButtons.Add ( go.name, go );
		}

		m_selectDisplayScript = GameObject.FindObjectOfType<SelectDisplay>();
	}

	void Update ()
	{
		if ( WorldController.instance.m_world == null )
		{
			return;
		}

		m_currFramePos = Camera.main.ScreenToWorldPoint ( Input.mousePosition );
		m_currFramePos.z = 0;

		WorldController.instance.GetTileAtWorldCoord ( m_currFramePos );

		UpdateDragging ();
		UpdateCameraMovement ();
		ProcessKeyboardInput();

		m_furniturePreview.transform.rotation = Quaternion.Euler ( 0, 0, 0 );

		if ( m_mode == "Furniture" && m_buildModeFurnName != null && m_buildModeFurnName != "" )
		{
			//Show transparent preview of the furniture that is colour-coded based on
			// whether or not you can actually build the furniture here.
			//ShowFurnitureSpriteAtTile ( m_buildModeFurnName, GetTileUnderMouse () );
			if ( m_buildModeFurnName == "Door" )
			{
				Furniture[] neighboursFurn = GetTileUnderMouse ().GetNeighboursFurniture ( false );

				if ( neighboursFurn [ 0 ] != null && neighboursFurn [ 2 ] != null)
				{
					if ( neighboursFurn [ 0 ].m_name == "Wall" && neighboursFurn [ 2 ].m_name == "Wall" )
					{
						m_furniturePreview.transform.rotation = Quaternion.Euler ( 0, 0, 90 );
					}
				}
			}
		}
		else
		{
			m_furniturePreview.SetActive ( false );
		}

		m_lastFramePos = Camera.main.ScreenToWorldPoint ( Input.mousePosition );
		m_lastFramePos.z = 0;

    }

    //The below function is only required if the player will be placing furniture during the game. Since this is not the case
    //it will not be used. It has been commented out so that the functions called within this function do not cause errors.

	/*void ShowFurnitureSpriteAtTile ( string _furnitureName, Tile _tile )
	{
		m_furniturePreview.SetActive ( true );

		SpriteRenderer sr = m_furniturePreview.GetComponent<SpriteRenderer> ();
		sr.sprite = m_furnSpriteController.GetSpriteForFurniture ( _furnitureName );
		if ( WorldController.instance.m_world.IsFurniturePlacementValid ( _furnitureName, _tile ) )
		{
			sr.color = new Color ( 0.5f, 1f, 0.5f, 0.25f );
		}
		else
		{
			sr.color = new Color ( 1f, 0.5f, 0.5f, 0.25f );
		}

		sr.sortingLayerName = "Preview";

		Furniture proto = WorldController.instance.m_world.m_furniturePrototypes [ _furnitureName ];	

		if ( _tile != null )
		{
			m_furniturePreview.transform.position = new Vector3 ( _tile.X + ( ( proto.Width - 1 ) / 2f ), _tile.Y + ( ( proto.Height - 1 ) / 2f ), 0 );
		}
	}
	*/


	public Vector3 GetMousePosition ()
	{
		return m_currFramePos;
	}

	void UpdateCameraMovement()
	{
		//Screen Dragging
		if ( Input.GetMouseButton ( 2 ) )
		{
			Vector3 diff = m_lastFramePos - m_currFramePos;
			Camera.main.transform.Translate(diff); //Moves the camera relative to itself by the given vector.
		}

		//Camera Zooming
		Camera.main.orthographicSize -=Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");
		Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 1, 10); //This sets the zoom level to always be between 1 and 20.
	}

	void UpdateDragging ()
	{

		if ( WorldController.instance.m_world == null )
		{
			return;
		}

		if ( EventSystem.current.IsPointerOverGameObject () )
		{
			return;
		}

		//Start Drag
		if ( Input.GetMouseButtonDown ( 0 ) )
		{
			m_isDragging = true;
			m_dragStartPos = m_currFramePos;
		}

		if ( m_buildModeFurnName != null && WorldController.instance.m_world.m_furniturePrototypes [ m_buildModeFurnName ].m_draggable == false )
		{
			m_dragStartPos = m_currFramePos;
		}

		int start_x = Mathf.FloorToInt ( m_dragStartPos.x + 0.5f );
		int end_x = Mathf.FloorToInt ( m_currFramePos.x + 0.5f );
		int start_y = Mathf.FloorToInt ( m_dragStartPos.y + 0.5f );
		int end_y = Mathf.FloorToInt ( m_currFramePos.y + 0.5f );

		if ( end_x < start_x )
		{
			int tmp = end_x;
			end_x = start_x;
			start_x = tmp;
		}

		if ( end_y < start_y )
		{
			int tmp = end_y;
			end_y = start_y;
			start_y = tmp;
		}

		//End Drag
		if ( Input.GetMouseButtonUp ( 0 ) && m_isDragging == true )
		{

			World world = WorldController.instance.m_world;

			for ( int x = start_x; x <= end_x; x++ )
			{
				for ( int y = start_y; y <= end_y; y++ )
				{
					Tile t = world.GetTileAt ( x, y );
					if ( t != null )
					{
						switch ( m_mode )
						{
							case "Furniture":
								world.PlaceFurnitureInWorld ( m_buildModeFurnName, t );
								break;
							case "CharacterWalk":
								world.m_characters [ 0 ].SetDestination ( t );
								break;
							default: //This means nothing is selected and the mouse is not in any mode.
									 //Default is essentailly "Select" mode.
								if ( t.m_furniture != null )
								{
									//A tile with some furniture was clicked.
									m_selectedFurn = t.m_furniture;
									m_selectDisplay.SetActive(true);
									m_selectDisplayScript.SetUpSelectionDisplay();
									Debug.Log("A tile with some furniture was clicked: Furniture: " + t.m_furniture.m_name);
								}
								break;
						}
						//Other modes will be implemented once characters and deleting furniture has been developed.
                    }
                }
            }
            m_isDragging = false;
          }

    }

	void ProcessKeyboardInput ()
	{
		if ( Input.GetKeyDown ( KeyCode.Escape ) )
		{
			m_selectDisplay.SetActive(false);
			m_stockDisplay.SetActive(false);
			m_mode = null;
		}
	}

	//Returns the tile that the mouse is currently on top of
	public Tile GetTileUnderMouse ()
	{
		return WorldController.instance.m_world.GetTileAt(
		Mathf.FloorToInt(m_currFramePos.x + 0.5f),
		Mathf.FloorToInt(m_currFramePos.y + 0.5f)
		);
	}

	public void SetMode_BuildFurniture (string _furnName)
	{
		Debug.Log("Set build mose to: " + _furnName);
		m_mode = "Furniture";
		m_buildModeFurnName = _furnName;
	}

	public void RevealMenuList ( string _list )
	{
		GameObject go = m_menuButtons [ _list ];

		for ( int i = 0; i < go.transform.childCount; i++ )
		{
			if ( go.transform.GetChild ( i ).name == "Menu" )
			{
				if ( go.transform.GetChild ( i ).gameObject.activeSelf == false )
				{
					go.transform.GetChild ( i ).gameObject.SetActive ( true );
				}
				else
				{
					go.transform.GetChild ( i ).gameObject.SetActive ( false );
				}
			}
		}

		for ( int i = 0; i < m_menu.transform.childCount; i++ )
		{
			for ( int j = 0; j < m_menu.transform.GetChild ( i ).childCount; j++ )
			{
				if ( m_menu.transform.GetChild ( i ).GetChild ( j ).name == "Menu" && m_menu.transform.GetChild ( i ).name != _list)
				{
					m_menu.transform.GetChild ( i ).GetChild ( j ).gameObject.SetActive ( false );	
				}

			}
			
		}
    }

    public void SetCharacterDest ()
	{
		m_mode = "CharacterWalk"; //This means that when the user clicks a tile. The character's destination will change.
	}

	public void ViewStock()
	{
		//This will cause the activeness of the StockDisplay to switch.
		//If it is true, it will become false.
		//If it is false, it will become true.
		m_stockDisplay.SetActive(!m_stockDisplay.activeSelf);
	}
}
