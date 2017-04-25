using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour {

	WorldController m_worldController;

	public GameObject m_instructionScene;
	public GameObject m_endScene;

	GameObject m_startScene;
	GameObject m_gameScene;

	void Start()
	{
		m_worldController = WorldController.instance;
		WorldController.instance.EC.ScenarioEnd.AddListener(ScenarioEnd);
		m_startScene = m_worldController.m_startScene;
		m_gameScene = m_worldController.m_gameScreen;
	}

	public void Instructions()
	{
		m_startScene.SetActive(false);
		m_instructionScene.SetActive(true);
	}

	public void BeginSimulation ()
	{
		m_worldController.SetUpWorld();
	}

	public void ScenarioEnd()
	{
		m_gameScene.SetActive(false);
		m_endScene.SetActive(true);
		m_worldController.m_world.m_scenarioOver = true;
	}
}
