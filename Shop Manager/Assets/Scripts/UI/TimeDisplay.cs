//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeDisplay : MonoBehaviour {

	public GameObject m_timeDisplayGO;
	Text m_timeDisplayText;
	World m_world;

	void Start ()
	{
		m_world = WorldController.instance.m_world;

		m_timeDisplayText = m_timeDisplayGO.GetComponent<Text> ();
		if ( m_timeDisplayText == null )
		{
			Debug.LogError ( "m_timeDisplayGO doesn't have a Text component" );
			m_timeDisplayGO.SetActive ( false );
			return;
		}

	}
	
	void Update ()
	{

		string second = m_world.m_second.ToString();
		string minute = m_world.m_minute.ToString();
		string hour = m_world.m_hour.ToString(); 

		if (m_world.m_second < 10)
		{
			second = "0" + m_world.m_second.ToString();
		}

		if (m_world.m_minute < 10)
		{
			minute = "0" + m_world.m_minute.ToString();
		}

		if (m_world.m_hour < 10)
		{
			hour = "0" + m_world.m_hour.ToString();
		}



		if ( m_timeDisplayGO.activeSelf == true )
		{
			m_timeDisplayText.text = "Time: " + hour + " : " + minute + " : " + second;
		}
	}
}
