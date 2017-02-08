﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job {

	public Tile Tile { get; protected set; }

	public float m_maxTime;
	
	public Dictionary<string, Furniture > m_furn;

	public PrimaryStates m_primaryState;
	
	public SecondaryStates m_secondaryState;

	//Create a new job with no given PrimaryState. State of the job will default to ServeOnTill.
	public Job ()
	{
		m_furn = new Dictionary<string, Furniture>();
		m_primaryState = PrimaryStates.ServeOnTill;
	}

	//Create a new job with its state set to the given PrimaryState.
	public Job ( PrimaryStates _state )
	{
		m_furn = new Dictionary<string, Furniture>();
		m_primaryState = _state;
	}

	public enum PrimaryStates{
		ServeOnTill,
		WorkStockCage,
		EmptyStockCage,
		WorkBackStock,
		FaceUp,
		CountTillMoney
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
			case PrimaryStates.ServeOnTill:
				return "Other_Checkout";

			case Job.PrimaryStates.WorkStockCage:
				return "Movable_Stockcage";
	
			case Job.PrimaryStates.EmptyStockCage:
					return "Movable_Stockcage";
	
			case Job.PrimaryStates.WorkBackStock:
				return "Back_StockShelf";
	
			case Job.PrimaryStates.FaceUp:
				return "Front_StockShelf";
	
			case Job.PrimaryStates.CountTillMoney:
				return "Other_Checkout";
			}

			return null;
		}

		protected set
		{

		}
	}

	public void DoJob ()
	{
		
	}
	
	void EnterSecState ( SecondaryStates _state )
	{
		switch ( _state )
		{
			case SecondaryStates.GoTo:
	
				break;
	
			case SecondaryStates.GoWith:
	
				break;
	
			case SecondaryStates.Idle:
	
				break;
	
			case SecondaryStates.Use:
	
				break;
		}
	
	}

	public bool SetJobTile ( Furniture _furn )
	{
		//Check to see if the furniture in the job's furniture list.
		if ( m_furn.ContainsValue ( _furn ) )
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

		return false;
	}
	
	public void SetPrimaryState(PrimaryStates _state)
	{
		m_primaryState = _state;
	}

	public void SetSecondaryState(SecondaryStates _state)
	{
		m_secondaryState = _state;
		EnterSecState(m_secondaryState);
	}
}
