using UnityEngine;
using System.Collections;


//This class cannot currently be tested because there are no characters implemented into the game.
public static class FurnitureActions {

	public static void Door_UpdateAction ( Furniture _furn, float _deltaTime )
	{
	if ( _furn.m_furnParameters [ "m_isOpening" ] >= 1 )
		{
			_furn.m_furnParameters [ "m_openness" ] += _deltaTime * 4;
			if ( _furn.m_furnParameters [ "m_openness" ] >= 1 )
			{
				_furn.m_furnParameters [ "m_isOpening" ] = 0;
			}
		}
		else
		{
			_furn.m_furnParameters [ "m_openness" ] -= _deltaTime * 4;
		}
		_furn.m_furnParameters["m_openness"] = Mathf.Clamp01(_furn.m_furnParameters["m_openness"]);

		if(_furn.cbOnChanged != null)
			_furn.cbOnChanged( _furn );
	}

	public static ENTERABILITY Door_IsEnterable ( Furniture _furn)
	{
		_furn.m_furnParameters [ "m_isOpening" ] = 1;
		if ( _furn.m_furnParameters [ "m_openness" ] >= 1 )
		{
			return ENTERABILITY.Yes;
		}

		return ENTERABILITY.Soon;

	}
}
