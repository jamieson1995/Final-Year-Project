using UnityEngine;
using System.Collections.Generic;

public class WorldController : MonoBehaviour {

	public static WorldController instance {get; protected set;} //Singleton

	public World m_world {get; protected set;}

	public Sprite m_tiledFloor; //FIXME think about whether a hard-coded floor sprite is ok.

	public GameObject m_tileGameObjects; //This will be the parent of all the tile GameObjects, this allows us to keep the hierarchy uncluttered. 

	Dictionary<Tile, GameObject> tileGameObjectMap; //This is needed to track which tile is related to which GameObject.

	public GameObject m_createWorldButton;
	public GameObject m_gameUI;

	public FurnitureSpriteController FSC;
	public CharacterSpriteController CSC;

	void Awake(){

		if (instance == null){
			instance = this;
		}
		else
			Debug.Log("Second World Controller tried to be created. Cannot have more than one World Controller.");

		
	}

	void Update ()
	{
		if ( m_world == null )
		{
			return;
		}
		m_world.Update(Time.deltaTime); //TODO Speed controls?
	}

	public Tile GetTileAtWorldCoord (Vector3 _coord, bool _canBeOutOfBounds = false)
    {
        int x = Mathf.FloorToInt (_coord.x + 0.5f);
        int y = Mathf.FloorToInt (_coord.y + 0.5f);
        if (_canBeOutOfBounds == false)
        {
            if (x < 0 || x > m_world.m_width || y < 0 || y > m_world.m_height)
            {
          	  return null;
            }
        }

        return m_world.GetTileAt(x,y);
    }

    public void SetUpWorld()
	{
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

		m_createWorldButton.SetActive(false);
		m_gameUI.SetActive(true);

		FSC.SetUpWorld();
		CSC.SetUpWorld();
	}

}
