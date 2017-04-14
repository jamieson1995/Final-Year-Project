//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

public class CharacterSpriteController : MonoBehaviour {

	///Returns the GameObject linked to the character input.
	Dictionary<Character, GameObject> m_characterGameObjectMap; 

	///Returns the Sprite based on the input name.
	Dictionary<string, Sprite> m_characterSprites; 

	///An array of all character sprites.
	public Sprite[] m_sprites;

	/// Reference to WorldController.instance.m_world
	World m_world;


	void Start ()
	{
		m_characterGameObjectMap = new Dictionary<Character, GameObject>();
		m_characterSprites = new Dictionary<string, Sprite>();
	}

	/// Establishes the reference to world and sets up the callbacks and sprites.
	public void SetUpWorld()
	{
		m_world = WorldController.instance.m_world;
		m_world.RegisterCharacterCreated(OnCharacterCreated);
		foreach ( Sprite s in m_sprites )
		{
			m_characterSprites[s.name] = s;
		}

		foreach ( Character c in m_world.m_characters )
		{
			OnCharacterCreated(c);
		}
	}


	/// Callback function which runs when a character gets created.
	public void OnCharacterCreated(Character _char)
	{
		GameObject char_go = new GameObject();

		m_characterGameObjectMap.Add( _char, char_go);
		char_go.name = "Character";
		char_go.transform.position = new Vector3(_char.X, _char.Y, 0);
		char_go.transform.SetParent (this.transform, true);
		SpriteRenderer sr = char_go.AddComponent<SpriteRenderer>();
		sr.sprite = m_characterSprites["StickMan"];
		sr.sortingLayerName = "Character";

		//Registers the callback for when a character changes visually - position etc.
		_char.RegisterOnChangedCallback( OnCharacterMoved );

	}

	/// Callback function which runs when a character moves.
	void OnCharacterMoved ( Character _char )
	{
		//Make sure the Character's graphics are correct.

		if ( m_characterGameObjectMap.ContainsKey ( _char ) == false )
		{
			Debug.LogError("OnCharacterMoved -- Trying to change visuals for a character not in our map!");
			return;
		}

		GameObject char_go = m_characterGameObjectMap[_char];
		char_go.transform.position = new Vector3(_char.X, _char.Y, 0);
	}
}
