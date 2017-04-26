using UnityEngine;
using UnityEngine.UI;

public class TileInspector : MonoBehaviour {

	public GameObject m_furnitureTypeGO;
	public GameObject m_characterGO;
	Text m_furnitureTypeGOText;
	Text m_characterTextGO;
	InputController m_inputController;

	void Start ()
	{
		m_furnitureTypeGOText = m_furnitureTypeGO.GetComponent<Text> ();
		if ( m_furnitureTypeGOText == null )
		{
			Debug.LogError ( "furnitureGO doesn't have a Text component" );
			m_furnitureTypeGO.SetActive ( false );
			return;
		}

		m_characterTextGO = m_characterGO.GetComponent<Text> ();
		if ( m_characterTextGO == null )
		{
			Debug.LogError ( "characterGO doesn't have a Text component" );
			m_characterGO.SetActive ( false );
			return;
		}

		m_inputController = GameObject.FindObjectOfType<InputController> ();

		if ( m_inputController == null )
		{
			Debug.LogError("We don't have an instance of input controller");
		}
	}

	void Update ()
	{
		Tile t = m_inputController.GetTileUnderMouse ();

		string s = "None";

		if ( t != null && t.m_furniture != null )
		{
			s = t.m_furniture.m_name;
		}

		if ( t == null )
		{
			s = "N/A";
		}

		m_furnitureTypeGOText.text = "Furniture: " + s;
		s = "None";

		if ( t != null && t.m_character != null )
		{
			s = t.m_character.m_name;
			m_characterTextGO.text = "Character: " + s;
		}

		if ( t == null )
		{
			s = "N/A";
		}

		m_characterTextGO.text = "Character: " + s;
		s = "None";
	}
}
