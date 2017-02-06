using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

/// <summary>
/// An employee is a character that can be given jobs.
/// </summary>
public class Employee : Character {

	Job m_job;

	string m_title;

	public Employee ( Tile _tile, string _title ) : base ( _tile )
	{
		m_title = _title;
	}

	Job CreateJob ()
	{
		Job j = new Job();

		return j;
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
			m_job.SetSecondaryState ( Job.SecondaryStates.Use );
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
				Debug.Log("Serving on the till!");
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
		if ( WorldController.instance.m_world.m_furnitureInMap.ContainsKey ( _furn ) )
		{
			foreach ( Furniture furn in WorldController.instance.m_world.m_furnitureInMap[_furn] )
			{
				if ( furn.m_used == true )
				{
					continue;
				}
				m_job.m_furn.Add ( _furn, furn );
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
		if ( m_job.m_furn.Count == 0 )
		{
			if ( FindJobFurn ( m_job.m_requiredFurniture ) )
			{
				SetDestination(m_job.Tile);
			}
		}
	}
	
}
