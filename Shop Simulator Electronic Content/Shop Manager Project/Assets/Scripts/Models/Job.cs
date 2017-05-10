﻿//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job {

	/// Reference to the tile used by employees when performing this job.
	public Tile Tile { get; protected set; }

	/// Flag to determine if this job required a Trolley.
	public bool RequiresTrolley { get; protected set;}

	/// Reference to the furniture required to perform this job.
	public Furniture m_furn;

	/// The current Primary State of this job.
	public PrimaryStates m_primaryState;

	/// The current Secondary State of this job.
	public SecondaryStates m_secondaryState;

	/// The additional information required for certain Primary states.
	public WorkTrolleyStates m_additionalInfoState;

	/// Creates a new job with Primary State ServeOnCheckout
	public Job ()
	{
		SetPrimaryState(PrimaryStates.ServeOnCheckout);
	}

	/// Creates a new job with the specified Primary State
	public Job ( PrimaryStates _newState, Job _oldJob = null )
	{
		SetPrimaryState(_newState, _oldJob);
	}

	/// All Primary States possible.
	public enum PrimaryStates{
		WorkStockcage,
		ServeOnCheckout,
		EmptyStockcage,
		WorkBackStock,
		FaceUp	
	}

	/// All Secondary States possible.
	public enum SecondaryStates{
		GoTo, //Employee is going to a location.
		GoWith, //Employee needs to go to a location with a piece of furniture.
		Idle, //Employee currently doesn't have anything to do.
		Use, //Employee is using a piece of furniture.
	}

	/// The different forms of Secondary State Idle.
	public enum IdleStates
	{
		Fixed, //Employee cannot move from thier location
		Moving, //Employee can move around
	}

	/// Additional State Information.
	public enum WorkTrolleyStates
	{
		// The Primary States WorkStockcage and WorkBackStock require additional information to determine what they need to do.
		FillTrolley,
		EmptyTrolleyToFront,
		EmptyTrolleyToBack
	}

	/// Returns the name of the required furniture based upon this job's current Primary State.
	public string m_requiredFurniture
	{
		get
		{
			World world = WorldController.instance.m_world;
			switch ( m_primaryState )
			{
				case PrimaryStates.ServeOnCheckout:
					return "Checkout";

				case Job.PrimaryStates.WorkStockcage:
					if ( m_additionalInfoState == Job.WorkTrolleyStates.FillTrolley )
					{
						return "Stockcage";
					}
					else if ( m_additionalInfoState == Job.WorkTrolleyStates.EmptyTrolleyToFront )
					{
						return "FrontShelf";
					}
					else if ( m_additionalInfoState == Job.WorkTrolleyStates.EmptyTrolleyToBack )
					{
						return "BackShelf";
					}
					break;
	
				case Job.PrimaryStates.EmptyStockcage:
					return "Stockcage";
	
				case Job.PrimaryStates.WorkBackStock:
					if ( m_additionalInfoState == Job.WorkTrolleyStates.FillTrolley )
					{
						return "BackShelf";
					}
					else if ( m_additionalInfoState == Job.WorkTrolleyStates.EmptyTrolleyToFront )
					{
						return "FrontShelf";
					}
					else if ( m_additionalInfoState == Job.WorkTrolleyStates.EmptyTrolleyToBack )
					{
						return "BackShelf";
					}
					break;	
						
				case Job.PrimaryStates.FaceUp:
					return "FrontShelf";
	
				default:
					return "Checkout";
			}

			return null;
		}

		protected set
		{

		}
	}

	/// Sets this job's furniture and tile variables to null. Before this, it sets this job's furniture to unmanned.
	void ResetJobVariables ( Job _oldJob )
	{
		if ( _oldJob != null && _oldJob.m_furn != null )
		{
			WorldController.instance.m_world.m_characterFurniture.Remove ( _oldJob.m_furn );
			_oldJob.m_furn.m_manned = false;
		}

		if ( _oldJob == null && m_furn != null )
		{
			WorldController.instance.m_world.m_characterFurniture.Remove ( m_furn );
			m_furn.m_manned = false;
		}

		m_furn = null;
		Tile = null;
		m_requiredFurniture = null;
	}

	/// Sets this job's furniture to the specified Furniture. Also sets this job's tile. Parameter CAN be null.
	public void SetJobFurn ( Furniture _furn, Character _char )
	{
		if ( _furn == null )
		{
			ResetJobVariables (null);
			return;
		}

		m_furn = _furn;

		if ( WorldController.instance.m_world.m_characterFurniture.ContainsKey ( _furn ) )
		{
			WorldController.instance.m_world.m_characterFurniture.Remove(_furn);	
		}

		WorldController.instance.m_world.m_characterFurniture.Add(_furn, _char);

		SetJobTile ( _furn );
	}

	/// Returns the attempt's outcome. Attempts to set this Job's tile to the specified furniture's job tile.
	/// If this job's furniture is different than the specified furniture, this returns false.
	public bool SetJobTile ( Furniture _furn )
	{
		//Check to see if the furniture is the one required for the job.
		if ( m_furn != null && _furn != null && m_furn == _furn )
		{
			//Check to see if the job's tile is furniture free 
																
			if ( _furn.m_actionTile.m_furniture == null || _furn.m_actionTile.m_furniture.m_movable == true)
			{
				Tile = _furn.m_actionTile;
				return true;
			}
		}
		else
		{
			Debug.LogError("SetJobTile() -- Specified furniture: " + _furn.m_name + " is different from job's furniture: " + m_furn.m_name);
			return false;
		}

		//This should be unreachable.
		return false;
	}

	/// Sets this job's Primary State to the specified state.
	public void SetPrimaryState ( PrimaryStates _newState,  Job _oldJob = null)
	{
		ResetJobVariables(_oldJob);
		if ( _newState == PrimaryStates.WorkBackStock || _newState == PrimaryStates.WorkStockcage )
		{
			RequiresTrolley = true;
		}
		else
		{
			RequiresTrolley = false;
		}

		if ( _oldJob != null && _oldJob.m_primaryState == PrimaryStates.ServeOnCheckout )
		{
			_oldJob.m_furn.m_manned = false;
			WorldController.instance.m_world.m_characterFurniture.Remove(_oldJob.m_furn);
			WorldController.instance.m_world.m_numberOfMannedCheckouts--;
		}

		m_primaryState = _newState;
	}

	/// Sets this job's Secondary State to the specified state.
	public void SetSecondaryState(SecondaryStates _state)
	{
		m_secondaryState = _state;
	}

	/// Sets this job's WorkTrolley State to the specified state.
	public void SetWorkTrolleyState(WorkTrolleyStates _state)
	{
		m_additionalInfoState = _state;
	}
}
