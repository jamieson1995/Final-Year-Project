using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour {

	/// Reference to the WorldController instance.
	WorldController m_worldController;

	/// References to the instruction and end scene GameObjects.
	public GameObject m_instructionScene;
	public GameObject m_endScene;

	///References to the start and game scene GameObject.
	GameObject m_startScene;
	GameObject m_gameScene;

	void Start()
	{
		m_worldController = WorldController.instance;
		WorldController.instance.EC.ScenarioEnd.AddListener(ScenarioEnd);
		m_startScene = m_worldController.m_startScene;
		m_gameScene = m_worldController.m_gameScreen;
	}

	/// Runs when the instruction scene needs to be displayed.
	public void Instructions()
	{
		m_startScene.SetActive(false);
		m_instructionScene.SetActive(true);
	}

	/// Runs when the game scene needs to be displayed, and the world needs to be set up.
	public void BeginSimulation ()
	{
		m_worldController.SetUpWorld();
	}

	/// Runs when the scenario ends and the end scene needs to be displayed.
	public void ScenarioEnd()
	{
		m_gameScene.SetActive(false);
		m_endScene.SetActive(true);
		m_worldController.m_world.m_scenarioOver = true;
	}

	/// Runs when the application quits.
	public void Quit()
	{
		Application.Quit();
	}
}
