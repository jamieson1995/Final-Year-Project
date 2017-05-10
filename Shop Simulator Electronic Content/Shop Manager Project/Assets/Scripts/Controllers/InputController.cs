//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputController : MonoBehaviour {

	///World position of the mouse at the end of last frame, used in camera movement.
	Vector3 m_lastFramePos;

	///World position of the mouse at the beginning of this frame.
	Vector3 m_currFramePos;

	/// String to determine the current mode of the mouse.
	public string m_mode { get; protected set; }

	/// Reference to the Select Display Script.
	SelectDisplay m_selectDisplayScript;

	/// Reference to the Furniture Select Display GameObject.
	public GameObject m_furnitureSelectDisplay;

	/// Reference to the Character Select Display GameObject.
	public GameObject m_characterSelectDisplay;

	///Reference to the Stock Display GameObject.
	public GameObject m_stockDisplay;

	///Reference to the trait Display GameObject.
	public GameObject m_traitDisplay;

	///Reference to the selected furniture information
	public Furniture m_selectedFurn;

	///Reference to the selected character information
	public Character m_selectedChar;

	void Start()
	{

		m_selectDisplayScript = GameObject.FindObjectOfType<SelectDisplay>();
	}

	void Update ()
	{
		if ( WorldController.instance.m_world == null )
		{
			return;
		}

		if ( WorldController.instance.m_world.m_scenarioOver )
		{
			return;
		}

		m_currFramePos = Camera.main.ScreenToWorldPoint ( Input.mousePosition );
		m_currFramePos.z = 0;

		WorldController.instance.GetTileAtWorldCoord ( m_currFramePos );

		UpdateDragging ();
		UpdateCameraMovement ();
		ProcessKeyboardInput();

		m_lastFramePos = Camera.main.ScreenToWorldPoint ( Input.mousePosition );
		m_lastFramePos.z = 0;

    }

    /// Returns m_currFramePos - Vector3
	public Vector3 GetMousePosition ()
	{
		return m_currFramePos;
	}

	/// Deals with the camera position and zooming.
	void UpdateCameraMovement ()
	{
		//Screen Dragging
		if ( Input.GetMouseButton ( 1 ) )
		{
			Vector3 diff = m_lastFramePos - m_currFramePos;

			Camera.main.transform.Translate ( diff ); //Moves the camera relative to itself by the given vector.

			//The following code clamps the camera to the world, meaning the user cannot drag the camera away from the shop.
			if ( Camera.main.transform.position.x < 0 )
			{
				Camera.main.transform.position = new Vector3 ( 0, Camera.main.transform.position.y, Camera.main.transform.position.z );
			}
			else if ( Camera.main.transform.position.x > WorldController.instance.m_world.m_width )
			{
				Camera.main.transform.position = new Vector3 ( WorldController.instance.m_world.m_width, Camera.main.transform.position.y, Camera.main.transform.position.z );
			}

			if ( Camera.main.transform.position.y < 0 )
			{
				Camera.main.transform.position = new Vector3 ( Camera.main.transform.position.x, 0, Camera.main.transform.position.z );
			}
			else if ( Camera.main.transform.position.y > WorldController.instance.m_world.m_height)
			{
				Camera.main.transform.position = new Vector3 ( Camera.main.transform.position.x, WorldController.instance.m_world.m_height, Camera.main.transform.position.z );
			}
		}

		//Camera Zooming
		Camera.main.orthographicSize -=Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");
		Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 1, 10); //This sets the zoom level to always be between 1 and 20.
	}

	/// Deals with all mouse inputs, and processing the dragging and selecting of tiles and furniture.
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

		//If the left-mouse button is released.
		if ( Input.GetMouseButtonUp ( 0 ) )
		{

			Tile t = GetTileUnderMouse ();
			if ( t != null )
			{
				Debug.Log ( "Tile clicked: (" + t.X + ", " + t.Y + ")" );

				//If we get here, we have clicked on a valid tile.
				switch ( m_mode )
				{
				//The only mode is selction mode, which is active when no other mode is active.
				//This placeholder is here so the switch statement can exist, if more mode need to be added later.
					case "PLACEHOLDER":
						break;
					default: //This means nothing is selected and the mouse is not in any mode.
							 //Default is essentailly "Select" mode.
						if ( t.m_furniture != null )
						{
							//A tile with some furniture was clicked.
							m_selectDisplayScript.m_selectType = SelectDisplay.SelectType.Furniture;
							m_selectedFurn = t.m_furniture;
							m_characterSelectDisplay.SetActive ( false );
							m_furnitureSelectDisplay.SetActive ( true );
							m_traitDisplay.SetActive ( false );
							m_selectDisplayScript.SetUpFurnitureSelectionDisplay ();
							if (m_selectedFurn.m_name == "Wall")
							{
								m_stockDisplay.SetActive ( false );
							}
						}
						else if ( t.m_character != null )
						{
							m_selectDisplayScript.m_selectType = SelectDisplay.SelectType.Character;
							m_selectedChar = t.m_character;
							m_furnitureSelectDisplay.SetActive ( false );
							m_characterSelectDisplay.SetActive ( true );
							m_selectDisplayScript.SetUpCharacterSelectionDisplay ();
						}
						else
						{
							m_selectDisplayScript.m_selectType = SelectDisplay.SelectType.NULL;
							m_furnitureSelectDisplay.SetActive ( false );
							m_characterSelectDisplay.SetActive ( false );
							m_stockDisplay.SetActive ( false );
							m_traitDisplay.SetActive ( false );
						}

						break;
				}
			}
		}
    }

    /// Processes the keyboard inputs.
	void ProcessKeyboardInput ()
	{

		if ( Input.GetKeyDown ( KeyCode.Escape ) )
		{
			m_furnitureSelectDisplay.SetActive ( false );
			m_characterSelectDisplay.SetActive ( false );
			m_stockDisplay.SetActive ( false );
			m_traitDisplay.SetActive ( false );
			m_mode = null;
		}
		else if ( Input.GetKeyDown ( KeyCode.Alpha1 ) )
		{
			WorldController.instance.SetGameSpeed ( 1f );
		}
		else if ( Input.GetKeyDown ( KeyCode.Alpha2 ) )
		{
			WorldController.instance.SetGameSpeed ( 10f );
		}
		else if ( Input.GetKeyDown ( KeyCode.Alpha3 ) )
		{
			WorldController.instance.SetGameSpeed ( 20f );
		}
		else if ( Input.GetKeyDown ( KeyCode.P ) )
		{
			if ( WorldController.instance.m_world != null && WorldController.instance.m_world.m_gameSpeed == 0 )
			{
				WorldController.instance.SetGameSpeed ( 1f );
			}
			else if ( WorldController.instance.m_world != null && WorldController.instance.m_world.m_gameSpeed != 0 )
			{
				WorldController.instance.SetGameSpeed ( 0f );
			}
		}
		else if ( Input.GetKeyDown ( KeyCode.Minus ) || Input.GetKeyDown ( KeyCode.KeypadMinus ) )
		{
			Camera.main.orthographicSize += 1;
			Camera.main.orthographicSize = Mathf.Clamp ( Camera.main.orthographicSize, 1, 10 ); //This sets the zoom level to always be between 1 and 20.
		}
		else if ( Input.GetKeyDown ( KeyCode.Equals ) || Input.GetKeyDown ( KeyCode.KeypadPlus ) )
		{
			Camera.main.orthographicSize -= 1;
			Camera.main.orthographicSize = Mathf.Clamp ( Camera.main.orthographicSize, 1, 10 ); //This sets the zoom level to always be between 1 and 20.
		}
	}

	///Returns the tile that the mouse is currently on top of.
	public Tile GetTileUnderMouse ()
	{
		if ( WorldController.instance.m_world == null )
		{
			return null;
		}


		return WorldController.instance.m_world.GetTileAt(
		Mathf.FloorToInt(m_currFramePos.x + 0.5f),
		Mathf.FloorToInt(m_currFramePos.y + 0.5f)
		);
	}

	///Reverses the activeness of the StockDisplay. Inactive becomes active, and active becomes inactive.
	public void ReverseStockDisplayActiveness ()
	{
		//This will cause the activeness of the StockDisplay to switch.
		//If it is true, it will become false.
		//If it is false, it will become true.
		m_stockDisplay.SetActive ( !m_stockDisplay.activeSelf );
		if (m_stockDisplay.activeSelf)
		{
			m_traitDisplay.SetActive(false);
		}
	}

	///Reverses the activeness of the StockDisplay. Inactive becomes active, and active becomes inactive.
	public void ReverseTraitDisplayActiveness ()
	{
		//This will cause the activeness of the TraitDisplay to switch.
		//If it is true, it will become false.
		//If it is false, it will become true.
		m_traitDisplay.SetActive ( !m_traitDisplay.activeSelf );
		if (m_traitDisplay.activeSelf)
		{
			m_stockDisplay.SetActive(false);
		}
	}
}
