using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TileInspector : MonoBehaviour {

	public GameObject m_furnitureTypeGO;
	public GameObject m_mouseModeGO;
	Text m_furnitureTypeGOText;
	Text m_mouseModeGOText;
	MouseController m_mouseController;

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

		m_mouseController = GameObject.FindObjectOfType<MouseController> ();

		if ( m_mouseController == null )
		{
			Debug.LogError("We don't have an instance of mouse controller");
		}
	}

	void Update ()
	{
		Tile t = m_mouseController.GetTileUnderMouse ();

		string s = "NULL";

		if ( t.m_furniture != null )
		{
			s = t.m_furniture.m_furnType.ToString ();
		}

		m_furnitureTypeGOText.text = "Furniture: " + s;
		s = "NULL";

		if ( m_mouseController.m_mode != null )
		{
			s = m_mouseController.m_mode.ToString ();
			m_mouseModeGOText.text = "Mouse Mode: " + s;
		}

		m_mouseModeGOText.text = "Mouse Mode: " + s;
		s = "NULL";
	}
}
