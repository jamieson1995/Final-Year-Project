using UnityEngine;
using System.Collections;

public class MouseController : MonoBehaviour {

	//World position of the mouse last frame
	Vector3 m_lastFramePos;

	//World position of the mouse this frame
	Vector3 m_currFramePos;

	bool buildModeIsFurniture = false;

	void Update ()
	{
		m_currFramePos = Camera.main.ScreenToWorldPoint ( Input.mousePosition );

		UpdateCameraMovement();

		m_lastFramePos = Camera.main.ScreenToWorldPoint ( Input.mousePosition );
	}

	void UpdateCameraMovement()
	{
		//Screen Dragging
		if ( Input.GetMouseButton ( 2 ) )
		{
			Vector3 diff = m_lastFramePos - m_currFramePos;
			Camera.main.transform.Translate(diff); //Moves the camera relative to itself by the given vector.
		}

		//Camera Zooming
		Camera.main.orthographicSize -=Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");
		Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 1, 20); //This sets the zoom level to always be between 1 and 20.
	}

	//Returns the tile that the mouse is currently on top of
	public Tile GetTileUnderMouse ()
	{
		return WorldController.instance.m_world.GetTileAt(
		Mathf.FloorToInt(m_currFramePos.x + 0.5f),
		Mathf.FloorToInt(m_currFramePos.y + 0.5f)
		);
	}

	void SetMode_BuildWall ()
	{
		buildModeIsFurniture = true;
	}
}
