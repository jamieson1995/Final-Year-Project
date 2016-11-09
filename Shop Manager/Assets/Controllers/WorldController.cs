using UnityEngine;
using System.Collections;

public class WorldController : MonoBehaviour {

	public static WorldController instance { get; protected set; }
	public World m_world { get; protected set; }

	void Awake()
	{
		if (instance == null) {
			instance = new WorldController ();
		} else
			Debug.LogError ("Trying to create a second World Controller. This is not possible");
	}

	void Start()
	{
		m_world = new World ();
	}
}
