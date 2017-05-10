using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyDisplay : MonoBehaviour {

	public GameObject m_moneyDisplayGO;
	Text m_moneyDisplayText;
	World m_world;

	void Start ()
	{
		m_world = WorldController.instance.m_world;

		m_moneyDisplayText = m_moneyDisplayGO.GetComponentInChildren<Text> ();
		if ( m_moneyDisplayText == null )
		{
			Debug.LogError ( "m_moneyDisplayGO doesn't have a Text component" );
			m_moneyDisplayGO.SetActive ( false );
			return;
		}

	}

	void Update ()
	{
		if ( m_moneyDisplayGO.activeSelf == true )
		{
			if ( m_world.m_money < 100 )
			{
				
				m_moneyDisplayText.text = "£0." + m_world.m_money;

				if ( m_world.m_money < 10 )
				{
					m_moneyDisplayText.text+= "0";
				}
			}
			else
			{
				m_moneyDisplayText.text = Stock.StringPrice(m_world.m_money);
			}

		}
	}
}
