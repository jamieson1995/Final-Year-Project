//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EventController : MonoBehaviour {

	[NonSerialized]
	public UnityEvent StockcageStockWorked = new UnityEvent();

	[NonSerialized]
	public UnityEvent BackStockWorked = new UnityEvent();

	[NonSerialized]
	public UnityEvent CustomerEnteredMap = new UnityEvent();

	[NonSerialized]
	public UnityEvent TransactionEnded = new UnityEvent();

	[NonSerialized]
	public UnityEvent ScenarioEnd = new UnityEvent();

	public AudioClip m_audioCustomerEnteredMapClip;
	public AudioClip m_audioTransactionEndedClip;

	AudioSource m_audioCustomerEnteredMapSource;
	AudioSource m_audioTransactionEndedSource;

	public GameObject m_messageBoxGO;

	Text m_messageBoxGOText;

	float m_messageBoxMaxTime = 0.0f;
	float m_messageBoxElapsedTime = 0.0f;

	bool m_timerOn = false;

	void Start ()
	{
		m_messageBoxGOText = m_messageBoxGO.GetComponentInChildren<Text> ();
		if ( m_messageBoxGOText == null )
		{
			Debug.LogError ( "m_furnitureNameGO doesn't have a Text component" );
			m_messageBoxGO.SetActive ( false );
			return;
		}

		StockcageStockWorked.AddListener ( StockcageStockWorkedFunc );
		BackStockWorked.AddListener ( BackStockWorkedFunc );
		CustomerEnteredMap.AddListener ( CustomerEnteredMapFunc );
		TransactionEnded.AddListener ( TransactionEndedFunc );

		if ( m_audioCustomerEnteredMapClip != null )
		{
			m_audioCustomerEnteredMapClip.LoadAudioData ();
			m_audioCustomerEnteredMapSource = gameObject.AddComponent<AudioSource> ();
			m_audioCustomerEnteredMapSource.clip = m_audioCustomerEnteredMapClip;
		}

		if ( m_audioTransactionEndedClip != null )
		{
			m_audioTransactionEndedClip.LoadAudioData ();
			m_audioTransactionEndedSource = gameObject.AddComponent<AudioSource> ();
			m_audioTransactionEndedSource.clip = m_audioTransactionEndedClip;
		}
	}

	void Update ()
	{
		if ( WorldController.instance.m_world == null || WorldController.instance.m_world.m_scenarioOver )
		{
			return;
		}

		if ( m_timerOn )
		{
			m_messageBoxElapsedTime += Time.deltaTime;

			if ( m_messageBoxElapsedTime >= m_messageBoxMaxTime )
			{
				m_messageBoxElapsedTime = 0.0f;
				m_messageBoxGO.SetActive(false);
				m_timerOn = false;
			}
		}
	}

	void StockcageStockWorkedFunc()
	{
		m_messageBoxMaxTime = 10.0f;	
		m_timerOn = true;
		m_messageBoxGO.SetActive(true);
		m_messageBoxGOText.text = "All stock on stockcages has been worked. Changing job to working back stock.";
	}

	void BackStockWorkedFunc()
	{
		m_messageBoxMaxTime = 10.0f;	
		m_timerOn = true;
		m_messageBoxGO.SetActive(true);
		m_messageBoxGOText.text = "All back stock worked. Changing job to facing up the shop front.";
	}

	void CustomerEnteredMapFunc ()
	{
		if ( WorldController.instance.m_world.m_scenarioOver )
		{
			return;
		}

		if ( m_audioCustomerEnteredMapSource != null )
		{
			m_audioCustomerEnteredMapSource.Play ();
		}
		else
		{
			Debug.LogWarning("Could not play m_audioCustomerEnteredMapSource because it is null.");
		}
	}

	void TransactionEndedFunc ()
	{
		if ( WorldController.instance.m_world.m_scenarioOver )
		{
			return;
		}

		if ( m_audioTransactionEndedSource != null )
		{
			m_audioTransactionEndedSource.Play ();
		}
		else
		{
			Debug.LogWarning("Could not play m_audioTransactionEndedSource because it is null.");
		}
	}

}
