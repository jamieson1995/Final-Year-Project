//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SelectDisplay : MonoBehaviour {

	public GameObject m_furnitureNameGO;
	public GameObject m_furnitureMannedGO;
	public GameObject m_furnitureFUPGO;
	public GameObject m_furnitureNumberOfItemsGO;
	public GameObject m_furnitureViewStockButtonGO;

	public GameObject m_characterNameGO;
	public GameObject m_characterMannedGO;
	public GameObject m_characterInteractingGO;
	public GameObject m_characterRelationshipGO;
	public GameObject m_characterNumberOfItemsGO;

	Text m_furnitureNameGOText;
	Text m_furnitureMannedGOText;
	Text m_furnitureFUPGOText;
	Text m_furnitureNumberOfItemsGOText;

	Text m_characterNameGOText;
	Text m_characterMannedGOText;
	Text m_characterInteractingGOText;
	Text m_characterRelationshipGOText;
	Text m_characterNumberOfItemsGOText;

	public Text m_stockDisplayGONameText;

	public Text m_charTraitPPerGOText;
	public Text m_charTraitNPerGOText;
	public Text m_charTraitPPhyGOText;
	public Text m_charTraitNPhyGOText;

	public Scrollbar m_stockScrollBar;

	string m_previousStockText;

	public Button m_viewFurnitureStockButton;
	public Button m_viewCharacterStockButton;
	public Button m_viewCharacterTraitButton;

	InputController m_inputController;

	public enum SelectType
	{
		Furniture, 
		Character,
		NULL
	}

	public SelectType m_selectType;

	void Start ()
	{
		m_inputController = GameObject.FindObjectOfType<InputController> ();

		if ( m_inputController == null )
		{
			Debug.LogError("We don't have an instance of input controller");
		}

		m_furnitureNameGOText = m_furnitureNameGO.GetComponent<Text> ();
		if ( m_furnitureNameGOText == null )
		{
			Debug.LogError ( "m_furnitureNameGO doesn't have a Text component" );
			m_furnitureNameGO.SetActive ( false );
			return;
		}

		m_characterNameGOText = m_characterNameGO.GetComponent<Text> ();
		if ( m_characterNameGOText == null )
		{
			Debug.LogError ( "m_characterNameGO doesn't have a Text component" );
			m_characterNameGO.SetActive ( false );
			return;
		}

		m_furnitureMannedGOText = m_furnitureMannedGO.GetComponent<Text> ();
		if ( m_furnitureMannedGOText == null )
		{
			Debug.LogError ( "m_furnitureMannedGO doesn't have a Text component" );
			m_furnitureMannedGO.SetActive ( false );
			return;
		}

		m_characterMannedGOText = m_characterMannedGO.GetComponent<Text> ();
		if ( m_characterMannedGOText == null )
		{
			Debug.LogError ( "m_characterMannedGO doesn't have a Text component" );
			m_characterMannedGO.SetActive ( false );
			return;
		}

		m_furnitureFUPGOText = m_furnitureFUPGO.GetComponent<Text> ();
		if ( m_furnitureFUPGOText == null )
		{
			Debug.LogError ( "m_furnitureFUPGO doesn't have a Text component" );
			m_furnitureFUPGO.SetActive ( false );
			return;
		}

		m_characterInteractingGOText = m_characterInteractingGO.GetComponent<Text> ();
		if ( m_characterInteractingGOText == null )
		{
			Debug.LogError ( "m_characterInteractingGO doesn't have a Text component" );
			m_characterInteractingGO.SetActive ( false );
			return;
		}

		m_characterRelationshipGOText = m_characterRelationshipGO.GetComponent<Text> ();
		if ( m_characterRelationshipGOText == null )
		{
			Debug.LogError ( "m_characterRelationshipGO doesn't have a Text component" );
			m_characterRelationshipGO.SetActive ( false );
			return;
		}

		m_furnitureNumberOfItemsGOText = m_furnitureNumberOfItemsGO.GetComponent<Text> ();
		if ( m_furnitureNumberOfItemsGOText == null )
		{
			Debug.LogError ( "m_furnitureMannedGO doesn't have a Text component" );
			m_furnitureNumberOfItemsGO.SetActive ( false );
			return;
		}
		m_characterNumberOfItemsGOText = m_characterNumberOfItemsGO.GetComponent<Text> ();
		if ( m_characterNumberOfItemsGOText == null )
		{
			Debug.LogError ( "m_characterMannedGO doesn't have a Text component" );
			m_characterNumberOfItemsGO.SetActive ( false );
			return;
		}

	}

	void Update ()
	{
		if ( WorldController.instance.m_world == null || WorldController.instance.m_world.m_scenarioOver )
		{
			return;
		}

		switch ( m_selectType )
		{
			case SelectType.Furniture:
				if ( m_inputController.m_furnitureSelectDisplay.activeSelf == true )
				{
					if ( m_inputController.m_selectedFurn.m_name == "Checkout"
					     || m_inputController.m_selectedFurn.m_name == "Stockcage"
					     || m_inputController.m_selectedFurn.m_name == "Trolley"
					     || m_inputController.m_selectedFurn.m_name == "BackShelf"
					     || m_inputController.m_selectedFurn.m_name == "Wall"
						 || m_inputController.m_selectedFurn.m_name == "Door" )
					{
						m_furnitureFUPGOText.text = "";
					}
					else
					{
						m_furnitureFUPGOText.text = "Faced up percentage - " + ( m_inputController.m_selectedFurn.m_facedUpPerc / 100 ).ToString ( "p" );
					}
					if ( m_inputController.m_selectedFurn != null )
					{
						if ( m_inputController.m_selectedFurn.m_name == "Wall" || m_inputController.m_selectedFurn.m_name == "Door")
						{
							m_furnitureMannedGOText.text = "";
						}
						else
						{
							m_furnitureMannedGOText.text = "Manned by: - ";
							if ( WorldController.instance.m_world.m_characterFurniture.ContainsKey ( m_inputController.m_selectedFurn ) )
							{
								m_furnitureMannedGOText.text += WorldController.instance.m_world.m_characterFurniture [ m_inputController.m_selectedFurn ].m_name;
							}
							else
							{
								m_furnitureMannedGOText.text += "No one.";
							}
						}
					}
				}
				else
				{
					return;
				}

				if ( m_inputController.m_selectedFurn.m_name == "Wall" || m_inputController.m_selectedFurn.m_name == "Door")
				{
					m_furnitureNumberOfItemsGOText.text = "";
					m_furnitureViewStockButtonGO.SetActive ( false );
					return;
				}

				m_furnitureViewStockButtonGO.SetActive ( true );

				int count = 0;
			
				if ( m_inputController.m_selectedFurn == null )
				{
					return;
				}
			
				foreach ( var stockList in m_inputController.m_selectedFurn.m_stock )
				{
					foreach ( Stock stock in m_inputController.m_selectedFurn.m_stock[stockList.Key] )
					{
						count++;
					}
				}
				m_furnitureNumberOfItemsGOText.text = "Number of Items - " + count;
				if ( count == 0 )
				{
					m_viewFurnitureStockButton.interactable = false;
					m_inputController.m_stockDisplay.SetActive ( false );
				}
				else
				{
					m_viewFurnitureStockButton.interactable = true;
				}
			
				if ( m_inputController.m_stockDisplay.activeSelf == true )
				{
					if ( m_inputController.m_selectedFurn != null )
					{
			
						m_stockDisplayGONameText.text = "";
						foreach ( var stockList in m_inputController.m_selectedFurn.m_stock )
						{
							foreach ( Stock stock in m_inputController.m_selectedFurn.m_stock[stockList.Key] )
							{
			
								m_stockDisplayGONameText.text += Stock.StringPrice ( stock.Price ) + "         " + stock.Name + "\n \n";
							}
						}
						if ( m_previousStockText != m_stockDisplayGONameText.text )
						{
							m_stockScrollBar.value = 0.5f;
						}
						m_previousStockText = m_stockDisplayGONameText.text;
					}
				}
				break;

			case SelectType.Character:

				if ( WorldController.instance.m_world.m_charactersInWorld.ContainsValue ( m_inputController.m_selectedChar ) == false )
				{
					m_inputController.m_characterSelectDisplay.SetActive ( false );
					m_inputController.m_stockDisplay.SetActive ( false );
					m_inputController.m_traitDisplay.SetActive ( false );
					m_inputController.m_selectedChar = null;
					return;
				}

				if ( m_inputController.m_characterSelectDisplay.activeSelf == true )
				{
					if ( m_inputController.m_selectedChar != null )
					{
						m_characterMannedGOText.text = "Using Furniture - ";
						if ( WorldController.instance.m_world.m_characterFurniture.ContainsValue ( m_inputController.m_selectedChar ) )
						{
							if ( m_inputController.m_selectedChar.m_movingFurniture != null )
							{
								m_characterMannedGOText.text += m_inputController.m_selectedChar.m_movingFurniture.m_name;
							}
							else
							{
								if ( m_inputController.m_selectedChar.m_requiredFurn != null )
								{
									m_characterMannedGOText.text += m_inputController.m_selectedChar.m_requiredFurn.m_name;
								}
								else
								{
									m_characterMannedGOText.text += "Nothing.";
								}
							}
						}
						else
						{
							m_characterMannedGOText.text += "Nothing.";
						}

						m_characterInteractingGOText.text = "Talking to - ";
						m_characterRelationshipGOText.text = "Relationship Level - ";
						if ( m_inputController.m_selectedChar.m_interactingCharacter != null )
						{
							m_characterInteractingGOText.text += m_inputController.m_selectedChar.m_interactingCharacter.m_name;
							m_characterRelationshipGOText.text += m_inputController.m_selectedChar.m_relationships [ m_inputController.m_selectedChar.m_interactingCharacter.ID ].relationshipLevel;
						}
						else
						{
							m_characterInteractingGOText.text += "No one.";
							m_characterRelationshipGOText.text = "";
						}
					}
				}
			
				count = 0;
			
				if ( m_inputController.m_selectedChar == null )
				{
					return;
				}
				if ( m_inputController.m_selectedChar.m_basket )
				{
					count = m_inputController.m_selectedChar.m_basketContents.Count;
					m_characterNumberOfItemsGOText.text = "Number of Items - " + count;

					if ( count == 0 )
					{
						m_viewCharacterStockButton.interactable = false;
						m_inputController.m_stockDisplay.SetActive ( false );
					}
					else
					{
						m_viewCharacterStockButton.interactable = true;
					}
			
					if ( m_inputController.m_stockDisplay.activeSelf == true )
					{
						if ( m_inputController.m_selectedChar != null )
						{
			
							m_stockDisplayGONameText.text = "";
							if ( m_inputController.m_selectedChar.m_basket )
							{
								foreach ( Stock stock in m_inputController.m_selectedChar.m_basketContents.ToArray() )
								{
									m_stockDisplayGONameText.text += Stock.StringPrice ( stock.Price ) + "         " + stock.Name + "\n \n";
								}
							}
							if ( m_previousStockText != m_stockDisplayGONameText.text )
							{
								m_stockScrollBar.value = 0.5f;
							}
							m_previousStockText = m_stockDisplayGONameText.text;
						}
					}
				}
				else
				{
					if ( m_inputController.m_selectedChar.m_stock != null )
					{
						m_viewCharacterStockButton.interactable = true;
						m_characterNumberOfItemsGOText.text = "Carrying Stock - " + m_inputController.m_selectedChar.m_stock;
					}
					else
					{
						m_characterNumberOfItemsGOText.text = "Carrying Stock - None";
						m_viewCharacterStockButton.interactable = false;
						m_inputController.m_stockDisplay.SetActive ( false );
					}
				}

				if ( m_inputController.m_traitDisplay.activeSelf )
				{
					if ( m_inputController.m_selectedChar != null )
					{
						m_charTraitPPerGOText.text = "Positve Personality Traits\n";
						m_charTraitPPerGOText.text += "\n";

						m_charTraitNPerGOText.text = "Negative Personality Traits\n";
						m_charTraitNPerGOText.text += "\n";

						m_charTraitPPhyGOText.text = "Positve Physical Traits\n";
						m_charTraitPPhyGOText.text += "\n";

						m_charTraitNPhyGOText.text = "Negative Physical Traits\n";
						m_charTraitNPhyGOText.text += "\n";

						Character character = m_inputController.m_selectedChar;

						if ( character.m_positivePersonTraits.Count > 0 )
						{
							foreach ( Character.PositivePersonalityTraits trait in character.m_positivePersonTraits.ToArray() )
							{
								m_charTraitPPerGOText.text += trait.ToString () + "\n";
							}
						}
						else
						{
							m_charTraitPPerGOText.text+= "None.\n";
						}

						if ( character.m_negativePersonTraits.Count > 0 )
						{
							foreach ( Character.NegativePersonalityTraits trait in character.m_negativePersonTraits.ToArray() )
							{
								m_charTraitNPerGOText.text += trait.ToString () + "\n";
							}
						}
						else
						{
							m_charTraitNPerGOText.text+= "None.\n";
						}

						if ( character.m_positivePhysTraits.Count > 0 )
						{
							foreach ( Character.PositivePhysicalTraits trait in character.m_positivePhysTraits.ToArray() )
							{
								m_charTraitPPhyGOText.text += trait.ToString () + "\n";
							}
						}
						else
						{
							m_charTraitPPhyGOText.text+= "None.\n";
						}

						if ( character.m_negativePhysTraits.Count > 0 )
						{
							foreach ( Character.NegativePhysicalTraits trait in character.m_negativePhysTraits.ToArray() )
							{
								m_charTraitNPhyGOText.text += trait.ToString () + "\n";
							}
						}
						else
						{
							m_charTraitNPhyGOText.text+= "None.\n";
						}

					}
				}
				break;
		
			default:
				m_inputController.m_furnitureSelectDisplay.SetActive ( false );
				m_inputController.m_characterSelectDisplay.SetActive ( false );
				m_inputController.m_stockDisplay.SetActive ( false );
				m_inputController.m_traitDisplay.SetActive ( false );
				break;
		}
	}

	public void SetUpFurnitureSelectionDisplay ()
	{
		m_furnitureNameGOText.text = "Name - " + m_inputController.m_selectedFurn.m_name;
	}

	public void SetUpCharacterSelectionDisplay ()
	{
		m_characterNameGOText.text = "Name - " + m_inputController.m_selectedChar.m_name;
	}
}

