using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SelectDisplay : MonoBehaviour {

	public GameObject m_furnitureNameGO;
	public GameObject m_furnitureMannedGO;
	public GameObject m_stockDisplayGO;
	Text m_furnitureNameGOText;
	Text m_furnitureMannedGOText;
	public Text m_stockDisplayGONameText;
	public Text m_stockDisplayGOScannedText;

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

		if ( m_inputController.m_stockDisplay.activeSelf == true )
		{
			if ( m_inputController.m_selectedFurn != null )
			{
				m_stockDisplayGONameText.text = "";
				foreach ( var stockList in m_inputController.m_selectedFurn.m_stock )
				{
					foreach ( Stock stock in m_inputController.m_selectedFurn.m_stock[stockList.Key] )
					{
						m_stockDisplayGONameText.text += stock.Name + "\n \n";
					}
				}

				m_stockDisplayGOScannedText.text = "";
				foreach ( var stockList in m_inputController.m_selectedFurn.m_stock )
				{
					foreach ( Stock stock in m_inputController.m_selectedFurn.m_stock[stockList.Key] )
					{
						m_stockDisplayGOScannedText.text += stock.m_scanned.ToString() + "\n \n";
					}
				}
			}
		}
	}

	public void SetUpSelectionDisplay()
	{
		m_furnitureNameGOText.text = "Name: " + m_inputController.m_selectedFurn.m_name;
	}
}
