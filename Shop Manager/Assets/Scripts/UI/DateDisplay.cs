//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DateDisplay : MonoBehaviour {

	public GameObject m_dateDisplayGO;
	Text m_dateDisplayText;
	World m_world;

	void Start ()
	{
		m_world = WorldController.instance.m_world;

		m_dateDisplayText = m_dateDisplayGO.GetComponent<Text> ();
		if ( m_dateDisplayText == null )
		{
			Debug.LogError ( "m_timeDisplayGO doesn't have a Text component" );
			m_dateDisplayGO.SetActive ( false );
			return;
		}

	}
	
	void Update ()
	{

		string day = m_world.m_day.ToString ();
		string month = m_world.m_month.ToString ();
		string dayOfWeek = "";

		if ( m_world.m_dayOfWeek == 1 )
		{
			dayOfWeek = "Mon";
		}
		else if ( m_world.m_dayOfWeek == 2)
		{
			dayOfWeek = "Tue";
		}
		else if ( m_world.m_dayOfWeek == 3 )
		{
			dayOfWeek = "Wed";
		}
		else if ( m_world.m_dayOfWeek == 4 )
		{
			dayOfWeek = "Thu";
		}
		else if ( m_world.m_dayOfWeek == 5 )
		{
			dayOfWeek = "Fri";
		}
		else if ( m_world.m_dayOfWeek == 6 )
		{
			dayOfWeek = "Sat";
		}
		else if ( m_world.m_dayOfWeek == 7 )
		{
			dayOfWeek = "Sun";
		}

		if (m_world.m_day < 10)
		{
			day = "0" + m_world.m_day.ToString();
		}

		if (m_world.m_month < 10)
		{
			month = "0" + m_world.m_month.ToString();
		}

		if ( m_dateDisplayGO.activeSelf == true )
		{
			m_dateDisplayText.text = "Week: " + m_world.m_week + "   " + dayOfWeek + " " + day + "/" + month + "/" + m_world.m_year;
		}
	}
}
