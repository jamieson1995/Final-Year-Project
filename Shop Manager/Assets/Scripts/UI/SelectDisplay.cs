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
	public GameObject m_furnitureNumberOfItemsGO;
	Text m_furnitureNameGOText;
	Text m_furnitureMannedGOText;
	Text m_furnitureNumberOfItemsGOText;
	public Text m_stockDisplayGONameText;

	public Scrollbar m_stockScrollBar;

	string m_previousStockText;

	public Button m_viewStockButton;

	InputController m_inputController;

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

		m_furnitureMannedGOText = m_furnitureMannedGO.GetComponent<Text> ();
		if ( m_furnitureMannedGOText == null )
		{
			Debug.LogError ( "m_furnitureMannedGO doesn't have a Text component" );
			m_furnitureMannedGO.SetActive ( false );
			return;
		}

		m_furnitureNumberOfItemsGOText = m_furnitureNumberOfItemsGO.GetComponent<Text> ();
		if ( m_furnitureNumberOfItemsGOText == null )
		{
			Debug.LogError ( "m_furnitureMannedGO doesn't have a Text component" );
			m_furnitureNumberOfItemsGO.SetActive ( false );
			return;
		}
	}

	void Update ()
	{
		if ( m_inputController.m_selectDisplay.activeSelf == true )
		{
			if ( m_inputController.m_selectedFurn != null )
			{
				m_furnitureMannedGOText.text = "Manned by: ";
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
		m_furnitureNumberOfItemsGOText.text = "Number of Items: " + count;
		if ( count == 0 )
		{
			m_viewStockButton.interactable = false;
			m_inputController.m_stockDisplay.SetActive(false);
		}
		else
		{
			m_viewStockButton.interactable = true;
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
						string priceToString = "";

						m_stockDisplayGONameText.text += stock.StringPrice + "         " + stock.Name + "\n \n";
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

	public void SetUpSelectionDisplay ()
	{
		m_furnitureNameGOText.text = "Name: " + m_inputController.m_selectedFurn.m_name;
	}
}
