﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An employee is a character that can be given jobs.
/// </summary>
public class Employee : Character {

	Job m_job;

	string m_title;

	public Employee ( string _name, int _maxCarryWeight, Tile _tile, string _title ) : base ( _name, _maxCarryWeight, _tile )
	{
		m_title = _title;
	}

	Job CreateJob ()
	{
		return new Job();
	}

	protected override void Update_DoThink ()
	{
		if ( m_job == null )
		{
			m_job = CreateJob ();
		}

		//If we get to here, the employee has a job.

		//Is the employee at the job tile?
		if ( m_currTile == m_job.Tile )
		{
			//Employee is at the job tile, so needs to use the furniture
			if ( m_job.m_secondaryState != Job.SecondaryStates.Idle )
			{
				m_job.SetSecondaryState ( Job.SecondaryStates.Use );
			}	
		}
		else
		{
			//Employee is not at the job tile, so they need to go there.
			m_job.SetSecondaryState ( Job.SecondaryStates.GoTo );
			;
		}

		switch ( m_job.m_secondaryState )
		{
			case Job.SecondaryStates.Use:
				Update_DoJob ();
				break;
			case Job.SecondaryStates.GoTo:
				GoToJobTile ();
				break;
			case  Job.SecondaryStates.Idle:
				Debug.Log(m_name + " is idle");
				break;
		}
	}

	void Update_DoJob ()
	{
		if ( m_job == null )
		{
			//Employee does not have a job, so does not need to do anything.
			return;
		}

		switch ( m_job.m_primaryState )
		{
			case Job.PrimaryStates.ServeOnTill:
				if ( TryTakeAnyStock ( m_job.m_furn, true ) == false )
				{
					//There is no more stock to take on this till.
					m_job.SetSecondaryState(Job.SecondaryStates.Idle);
					break;
				}
				ScanStockTill(m_stock);
				m_job.m_furn.TryAddStock( TryGiveStock( m_stock.IDName ) );
				//Debug.Log("Serving on the till!");
				break;
	
			case Job.PrimaryStates.WorkStockCage:
	
				break;
	
			case Job.PrimaryStates.EmptyStockCage:
	
				break;
	
			case Job.PrimaryStates.WorkBackStock:
	
				break;
	
			case Job.PrimaryStates.FaceUp:
	
				break;
	
			case Job.PrimaryStates.CountTillMoney:
	
				break;
		}
	}

	bool FindJobFurn ( string _furn )
	{
		if ( WorldController.instance.m_world.m_furnitureInWorld.ContainsKey ( _furn ) )
		{
			foreach ( Furniture furn in WorldController.instance.m_world.m_furnitureInWorld[_furn] )
			{
				if ( furn.m_used == true )
				{
					continue;
				}
				m_job.m_furn = furn;
				if ( m_job.SetJobTile ( furn ) == false )
				{
					Debug.LogError ( "Failed to set job tile - _furn -> " + _furn + ", m_job.m_primaryState - > " + m_job.m_primaryState.ToString () );
					return false;
				}

				return true;
			}
		}

		Debug.LogWarning("Failed to find job furniture - Furniture -> " + _furn + ", Job State -> " + m_job.m_primaryState.ToString());
		return false;
	}

	void GoToJobTile ()
	{
		if ( m_job.m_furn == null )
		{
			if ( FindJobFurn ( m_job.m_requiredFurniture ) )
			{
				SetDestination ( m_job.Tile );
			}
		}
	}

	void ScanStockTill ( Stock _stock )
	{
		if ( _stock == null )
		{
			Debug.LogError(m_name + " tried to scan a null stock");
			return;
		}
		_stock.m_scanned = true;
		Debug.Log("BEEP! " + _stock.Name + " was scanned through");
	}
	
}
