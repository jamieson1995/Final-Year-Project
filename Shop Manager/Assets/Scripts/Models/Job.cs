//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job {

	public Tile Tile { get; protected set; }

	public float m_maxTime;

	public bool RequiresTrolley { get; protected set;}
	
	public Furniture m_furn;

	public PrimaryStates m_primaryState;
	
	public SecondaryStates m_secondaryState;

	//Create a new job with no given PrimaryState. State of the job will default to ServeOnTill.
	public Job ()
	{
		m_primaryState = PrimaryStates.ServeOnCheckout;
	}

	//Create a new job with its state set to the given PrimaryState.
	public Job ( PrimaryStates _state )
	{
		SetPrimaryState(_state);
	}

	public enum PrimaryStates{
		ServeOnCheckout,
		WorkStockcage,
		EmptyStockcage,
		WorkBackStock,
		FaceUp,
		CountCheckoutMoney
	}
	
	public enum SecondaryStates{
		GoTo, //Employee is going to a location.
		GoWith, //Employee needs to go to a location with a piece of furniture.
		Idle, //Employee currently doesn't have anything to do.
		Use, //Employee is using a piece of furniture.
	}

	//TODO think about whether these actually need to be states, or functions in the employee class.
	public enum UseStates{
		TakeStock,
		GiveStock,
		ScanStock,
		TidyStock
	}
	
	public enum IdleStates
	{
		Fixed, //Employee cannot move from thier location
		Moving, //Employee can move around
	}

	/// <summary>
	/// This is the required furniture for the current job.
	/// </summary>
	/// <value>The furniture's FurnType.</value>
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
					return "Stockcage";
	
				case Job.PrimaryStates.EmptyStockcage:
					return "Stockcage";
	
				case Job.PrimaryStates.WorkBackStock:
					return "FrontShelf";
	
				case Job.PrimaryStates.FaceUp:
					return "FrontShelf";
	
				case Job.PrimaryStates.CountCheckoutMoney:
					return "Checkout";
			}

			return null;
		}

		protected set
		{

		}
	}

	void ResetJobVariables ()
	{
		if ( m_furn != null )
		{
			m_furn.m_manned = false;
		}
		m_furn = null;
		Tile = null;
		m_requiredFurniture = null;
	}

	/// <summary>
	/// Sets this job's furn with the specified furniture, and then sets the Job's tile to the furniture's job tile.
	/// </summary>
	public void SetJobFurn ( Furniture _furn )
	{
		if ( _furn == null )
		{
			ResetJobVariables();
			return;
		}
		m_furn = _furn;
		SetJobTile(_furn);
	}

	/// <summary>
	/// Checks to see if specified furniture is the same as this job's furniture, if true, this job's tile gets set to the furniture's job tile.
	/// If returns false, specified furniture is different than the job's furniture.
	/// </summary>
	public bool SetJobTile ( Furniture _furn )
	{
		//Check to see if the furniture is the one required for the job.
		if ( m_furn != null && _furn != null && m_furn == _furn )
		{
			//Check to see if the job's tile is furniture free 
			//TODO In the future, furniture should be not able to be placed in a furniture job tile, and also furniture should not be
			//able to be placed if its job tile spot is over another piece of furniture.
																
			if ( _furn.m_jobTile.m_furniture == null )
			{
				Tile = _furn.m_jobTile;
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

	public void SetPrimaryState ( PrimaryStates _state )
	{
		ResetJobVariables();
		if ( _state == PrimaryStates.WorkBackStock || _state == PrimaryStates.WorkStockcage )
		{
			RequiresTrolley = true;
		}
		else
		{
			RequiresTrolley = false;
		}
		if ( m_primaryState == PrimaryStates.ServeOnCheckout )
		{
			WorldController.instance.m_world.m_numberOfMannedTills--;
		}
		m_primaryState = _state;
	}

	public void SetSecondaryState(SecondaryStates _state)
	{
		m_secondaryState = _state;
	}
}
