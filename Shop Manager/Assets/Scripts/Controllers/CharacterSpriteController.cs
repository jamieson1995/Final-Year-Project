//////////////////////////////////////////////////////
//Copyright James Jamieson 2016/2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

public class CharacterSpriteController : MonoBehaviour {

	Dictionary<Character, GameObject> m_characterGameObjectMap; //Dictionary of all Character GameObjects.
	Dictionary<string, Sprite> m_characterSprites; //Dictionary of all Character Sprites.

	public Sprite[] m_sprites;

	World m_world;

	void Start ()
	{
		m_characterGameObjectMap = new Dictionary<Character, GameObject>();
		m_characterSprites = new Dictionary<string, Sprite>();
	}

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

		_char.RegisterOnChangedCallback( OnCharacterMoved );

	}

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
