using UnityEngine;
using System.Collections.Generic;

public class WorldController : MonoBehaviour {

	public static WorldController instance {get; protected set;} //Singleton

	public World m_world {get; protected set;}

	public Sprite m_tiledFloor; //For now, while we haven't got the rest of the sprites, we will hard code the floor sprite so we can
								//see it in game. TODO

	public GameObject m_tileGameObjects; //This will be the parent of all the tile GameObjects, this allows us to keep the hierarchy uncluttered. 

	Dictionary<Tile, GameObject> tileGameObjectMap; //This is needed to track which tile is related to which GameObject.

	void Awake(){

		if (instance == null){
			instance = this;
		}
		else
			Debug.Log("Second World Controller tried to be created. Cannot have more than one World Controller.");

		m_world = new World();

		//Instantiate our dictionary that tracks which GameObject is rendering which tile data.
		tileGameObjectMap = new Dictionary<Tile, GameObject>();

		//With World set up, we need to instantiate the game object for each tile.

		for ( int x = 0; x < m_world.m_width; x++ )
		{
			for ( int y = 0; y < m_world.m_height; y++ )
			{
				Tile tileData = m_world.GetTileAt(x,y);
				GameObject tileGO = new GameObject();
				tileGameObjectMap.Add( tileData, tileGO);
				tileGO.name = "Tile (" + x + "," + y + ")";
				tileGO.transform.position = new Vector3 ( tileData.X, tileData.Y, 0 );
				tileGO.transform.SetParent ( m_tileGameObjects.transform, true );
				SpriteRenderer sr = tileGO.AddComponent<SpriteRenderer> ();
				sr.sortingLayerName = "Tile";
				tileGO = tileGameObjectMap [ tileData ];
				sr.sprite = m_tiledFloor;

			}
		}
		//Center the camera in the middle of the world.
		Camera.main.transform.position = new Vector3( m_world.m_width/2, m_world.m_height/2, Camera.main.transform.position.z );
	}


}
