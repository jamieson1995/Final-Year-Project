//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;


public static class FurnitureActions {

	/// This runs once a frame if the required furniture registers it.
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

	/// This runs once a frame if the required furniture registers it.
	public static ENTERABILITY Door_IsEnterable ( Furniture _furn)
	{
		_furn.m_furnParameters [ "m_isOpening" ] = 1;
		if ( _furn.m_furnParameters [ "m_openness" ] >= 1 )
		{
			return ENTERABILITY.Yes;
		}

		return ENTERABILITY.Soon;

	}

	/// This runs once a frame if the required furniture registers it.
	public static void MovableFurn_UpdateAction ( Furniture _furn, float _deltaTime )
	{
		if ( _furn.m_moving == true )
		{
			if ( _furn.m_furnParameters [ "m_currTile.X" ] == _furn.m_furnParameters [ "m_destTile.X" ] &&
			     _furn.m_furnParameters [ "m_currTile.Y" ] == _furn.m_furnParameters [ "m_destTile.Y" ] )
			{
				_furn.m_moving = false;
				return;
			}

			float distToTravel = Mathf.Sqrt ( Mathf.Pow ( _furn.m_furnParameters [ "m_currTile.X" ] - _furn.m_furnParameters [ "m_destTile.X" ], 2 ) +
				                    		  Mathf.Pow ( _furn.m_furnParameters [ "m_currTile.Y" ] - _furn.m_furnParameters [ "m_destTile.Y" ], 2 ) );

			float distThisFrame = _furn.m_furnParameters [ "m_speed" ] * _deltaTime;
			float percThisFrame = distThisFrame / distToTravel;

			_furn.m_furnParameters [ "m_movementPercentage" ] += percThisFrame;

			if ( _furn.m_furnParameters [ "m_movementPercentage" ] >= 1 )
			{
				_furn.m_furnParameters[ "m_currTile.X" ] = _furn.m_furnParameters[ "m_destTile.X" ];
				_furn.m_furnParameters[ "m_currTile.Y" ] = _furn.m_furnParameters[ "m_destTile.Y" ];
				_furn.m_mainTile = WorldController.instance.m_world.GetTileAt( (int)_furn.m_furnParameters[ "m_destTile.X" ], (int)_furn.m_furnParameters[ "m_destTile.Y" ] );
				_furn.m_furnParameters["m_movementPercentage"] = 0.0f;
			}
				WorldController.instance.m_world.cbFurnitureMoved( _furn );
		}
	}
}
