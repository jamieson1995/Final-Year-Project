using UnityEngine;
using UnityEngine.UI;

public class TileInspector : MonoBehaviour {

	public GameObject m_furnitureTypeGO;
	public GameObject m_mouseModeGO;
	Text m_furnitureTypeGOText;
	Text m_mouseModeGOText;
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

		m_mouseModeGOText = m_mouseModeGO.GetComponent<Text> ();
		if ( m_mouseModeGOText == null )
		{
			Debug.LogError ( "furnitureGO doesn't have a Text component" );
			m_mouseModeGO.SetActive ( false );
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

		string s = "NULL";

		if ( t != null && t.m_furniture != null )
		{
			s = t.m_furniture.m_name;
		}

		if ( t == null )
		{
			s = "N/A";
		}

		m_furnitureTypeGOText.text = "Furniture: " + s;
		s = "NULL";

		if ( m_inputController.m_mode != null )
		{
			s = m_inputController.m_mode.ToString ();
			m_mouseModeGOText.text = "Mouse Mode: " + s;
		}

		m_mouseModeGOText.text = "Mouse Mode: " + s;
		s = "NULL";
	}
}
