//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// An employee is a character that can be given jobs.
/// </summary>
public class Employee : Character {

	Job m_job;

	string m_title;

	public Furniture TrolleyBeingUsed { get; protected set; }

	public Employee ( string _name, int _maxCarryWeight, Tile _tile, string _title ) : base ( _name, _maxCarryWeight, _tile )
	{
		m_title = _title;
	}

	Job CreateJob ()
	{

		World world = WorldController.instance.m_world;

		if ( ( world.m_numberOfMannedTills == 0 && world.m_customersInStore > 0 ) || world.m_customersInQueue != 0 && ( world.m_customersInQueue / world.m_numberOfMannedTills ) > 3 )
		{
			Debug.Log("Started new job - Serve on Checkout");
			return new Job ( Job.PrimaryStates.ServeOnCheckout );
		}
		else
		{
			Debug.Log("Started new job - Work Stockcage");
			return new Job ( Job.PrimaryStates.WorkStockcage );
		}
	}

	protected override void Update_DoThink ()
	{
		if ( m_currTile == m_destTile )
		{
			m_ignoreTask = false;
		}

		if ( m_ignoreTask == true )
		{
			return;
		}

		if ( m_job == null )
		{
			m_job = CreateJob ();
		}

		//If we get to here, the employee has a job.

		//Is the employee at the job tile?
		if ( m_job.Tile != null && m_currTile == m_job.Tile )
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
		}

		switch ( m_job.m_secondaryState )
		{
			case Job.SecondaryStates.Use:
				Update_DoJob ();
				break;
			case Job.SecondaryStates.GoTo:
				if ( m_job.RequiresTrolley == true )
				{
					if ( TrolleyBeingUsed == null )
					{
						//We need a trolley for this job, but don't have one, so find one.
						foreach ( Furniture f in m_world.m_furnitureInWorld["Trolley"].ToArray() )
						{
							if ( f.m_manned == true )
							{
								continue;
							}
							else
							{
								TrolleyBeingUsed = f;
								break;
							}
						}
						if ( TrolleyBeingUsed == null )
						{
							//We looked for a free trolley, but there wasn't one.
							Debug.LogWarning ( "Looked for a trolley for job: " + m_job.m_primaryState + " but couldn't find a free one." );
							return;
						}
					}
					if ( TrolleyBeingUsed.m_moving == false )
					{
						if ( m_currTile.IsNeighbour ( TrolleyBeingUsed.m_mainTile, true ) == false && m_currTile != TrolleyBeingUsed.m_mainTile )
						{
							//We are not next to the trolley we need to move, so go there.
							Path_AStar TEMPPath = new Path_AStar(m_world, m_currTile, TrolleyBeingUsed.m_mainTile);
							Tile tile = TEMPPath.InitialPathToArray()[TEMPPath.InitialPathToArray().Length - 2];
							if ( m_destTile != tile )
							{
								if ( m_requiredFurn == null )
								{
									SetDestination ( tile );
								}
								else if ( TrolleyBeingUsed.m_mainTile != m_requiredFurn.m_mainTile )
								{
									SetDestination ( tile );
								}
							}
						}
						else
						{
							//We are next to the trolley we need to move
							if ( TrolleyBeingUsed.m_mainTile != m_job.Tile )
							{
								m_requiredFurn = TrolleyBeingUsed;
								GoToJobTile ( TrolleyBeingUsed );
							}
							else
							{
								m_movingFurniture = null;
								m_pathAStar = null;
								GoToJobTile ();
							}
						}
					}
				}
				else
				{
					GoToJobTile ();
				}
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
			case Job.PrimaryStates.ServeOnCheckout:
				m_world.m_numberOfMannedTills++;
				if ( TryTakeAnyStock ( m_job.m_furn, true ) == false )
				{
					//There is no more stock to take on this till.
					m_job.SetSecondaryState ( Job.SecondaryStates.Idle );
					break;
				}
				ScanStockTill ( m_stock );
				m_job.m_furn.TryAddStock ( TryGiveStock ( m_stock.IDName ) );
				break;
	
			case Job.PrimaryStates.WorkStockcage:
				if ( TrolleyBeingUsed.m_full == false )
				{
					if ( TryTakeAnyStock ( m_job.m_furn ) == false )
					{
						Debug.Log ( "Failed to take any stock from stockcage, it is probably empty." );
						m_job.SetPrimaryState ( Job.PrimaryStates.WorkBackStock );
					}
					else
					{
						if ( TrolleyBeingUsed.TryAddStock ( TryGiveStock ( m_stock.IDName ) ) == false )
						{
							Debug.Log ( "Failed to give stock to trolley, it is probably full." );
							m_job.m_furn.TryGiveStock ( m_stock.IDName );
						}
					}
				}
				else
				{
					//The trolley is full, so we should change our job to go and work this trolley's stock.
					m_job.SetPrimaryState ( Job.PrimaryStates.WorkBackStock );
				}
				break;
	
			case Job.PrimaryStates.EmptyStockcage:
	
				break;
	
			case Job.PrimaryStates.WorkBackStock:
				if ( m_destTile == m_job.Tile )
				{
					//If we are going to, or where we need to be.
					if ( m_currTile == m_job.Tile )
					{
						//If we are in the job's tile.
						if ( EmptyTrolley ( m_job.m_furn ) == false )
						{
							//The trolley didn't get emptied, because the furniture was full.
							//We need to now find another furniture that this stock can be moved to.
							//This can be done by just searching through all furniture again, because the furniture will now get flagged as full.
							m_job.SetJobFurn(null);
							m_job.SetSecondaryState(Job.SecondaryStates.Use);
							return;
						}
						else
						{
							//Either all the stock was moved, or there is no more stock on the trolley that has the same name as the furniture stock.
							if ( TrolleyBeingUsed.m_stock.Count == 0 )
							{
								//Trolley is empty.
								m_job.SetPrimaryState ( Job.PrimaryStates.WorkStockcage );
							}
							//The stock on our trolley cannot go in the furniture we are next to. So find another.
							m_job.SetJobFurn(null);
						}
					}
					else
					{
						//We are not yet at the job tile, but we are going there
						return;
					}
				}
				else
				{
					//We are not on our way to the job tile.
					if ( TrolleyBeingUsed.m_stock.Count > 0 )
					{
						//We have stock on this trolley. So find out where it needs to go.
						foreach ( var stockList in TrolleyBeingUsed.m_stock )
						{
							foreach ( Stock stock in TrolleyBeingUsed.m_stock[stockList.Key] )
							{
								if ( stock.m_triedGoingOut == true )
								{
									//This piece of stock has already been worked. So put it back in the warehouse.
									Furniture furn = FindFurnWithStockOfType ( stock, false );
									m_job.SetJobFurn ( furn );
								}
							}

						}
					}
					else
					{
						//Our trolley is empty
						m_job.SetPrimaryState(Job.PrimaryStates.WorkStockcage);
					}
				}
				break;
	
			case Job.PrimaryStates.FaceUp:
	
				break;
	
			case Job.PrimaryStates.CountCheckoutMoney:
	
				break;
		}
	}

	bool FindJobFurn ( string _furn )
	{
		if ( m_job.m_primaryState == Job.PrimaryStates.WorkBackStock )
		{
			//We don't know what furniture we need, so work it out.
			//Find first type of stock in the trolley being used.
			if ( TrolleyBeingUsed == null )
			{
				Debug.Log ( "We don't have a trolley" );
				return false;
			}
			foreach ( string key in TrolleyBeingUsed.m_stock.Keys )
			{
				bool allWorked = true;
				foreach ( Stock s in TrolleyBeingUsed.m_stock [ key ].ToArray() )
				{
					if ( s.m_triedGoingOut == false )
					{
						allWorked = false;
						break;
					}
				}
				Furniture furn;
				if ( allWorked == true )
				{
					Debug.Log ( "All stock on this trolley has been worked." );
					furn = FindFurnWithStockOfType ( TrolleyBeingUsed.m_stock [ key ] [ 0 ], false );
					m_job.SetJobFurn ( furn );	
					return true;
				}
				furn = FindFurnWithStockOfType ( TrolleyBeingUsed.m_stock [ key ] [ 0 ], true );
				m_job.SetJobFurn ( furn );	
				return true;
			}

		}

		if ( WorldController.instance.m_world.m_furnitureInWorld.ContainsKey ( _furn ) )
		{
			foreach ( Furniture furn in WorldController.instance.m_world.m_furnitureInWorld[_furn] )
			{
				if ( furn.m_manned == true )
				{
					continue;
				}
				if ( m_job.m_primaryState == Job.PrimaryStates.WorkStockcage )
				{
					if ( furn.m_stock.Count == 0 )
					{
						continue;
					}
				}
				m_job.SetJobFurn(furn);
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

	void GoToJobTile ( Furniture _furn = null )
	{
		if ( m_job.m_furn == null )
		{
			//We need a furniture for the job.
			if ( FindJobFurn ( m_job.m_requiredFurniture ) )
			{
				World m_world = WorldController.instance.m_world;
				m_job.m_furn.m_manned = true;
				if ( m_world.m_characterFurniture.ContainsValue ( this ) == false )
				{
					m_world.m_characterFurniture.Add ( m_job.m_furn, this );
				}
				if ( m_job.RequiresTrolley == true )
				{
					if ( m_job.Tile == null )
					{
						Debug.LogError("Job's tile is null.");
						return;
					}
					SetDestWithFurn(m_job.Tile, _furn);
				}
				else
				{
					if ( m_job.Tile == null )
					{
						Debug.LogError("Job's tile is null.");
						return;
					}
					m_requiredFurn = m_job.m_furn;
					SetDestination ( m_job.Tile );
				}
			}
		}
		if ( m_job.RequiresTrolley == false )
		{
			//We have a furniture for the job.
			if ( m_job.Tile != null && m_currTile != m_job.Tile && m_destTile != m_job.Tile )
			{
				//If we get here, we are not in the job's tile, and we are not going to the job's tile, and we don't need a trolley.
				m_requiredFurn = m_job.m_furn;
				SetDestination ( m_job.Tile );	
			}
		}
		else
		{
			//if ( TrolleyBeingUsed.m_mainTile != m_job.Tile )

			if(m_job.Tile != null && m_currTile != m_job.Tile && m_destTile != m_job.Tile)
			{
				//If we get here, we are not in the job's tile, and we are not going to the job's tile, but we have a trolley we need.
				SetDestWithFurn(m_job.Tile, _furn);	
				return;
			}
			else
			{
				//Trolley is at the job tile, so we need to now go there. We no longer require it, we now require the other furniture needed for the job.
				if ( m_job.Tile == null )
				{
					Debug.LogError("Job's tile is null.");
					return;
				}
				m_requiredFurn = m_job.m_furn;
				SetDestination(m_job.Tile);
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
	/// <summary>
	/// Loops through each stock type in specified furn, and in TrolleyBeingUsed; if they are the same, attempt to move it.
	/// If return is false, some stock couldn't be moved because they furniture is full.
	///If return is true, all stock was moved, or there is no stock left on trolley with the same name as on furniture.
	/// </summary>
	bool EmptyTrolley ( Furniture _furn )
	{
		if ( m_currTile.IsNeighbour ( _furn.m_middleTile ) == false )
		{
			Debug.LogError ( "Trying to empty a trolley onto a furniture we are not neighbours with" );
			return false;
		}

		string[] stockStringFurnTempArray = _furn.m_stock.Keys.ToArray ();

		if ( stockStringFurnTempArray.Length == 0 )
		{
			//This furniture contains no stock, so just add it from the trolley.

			string[] stockStringTrolleyTempArray = TrolleyBeingUsed.m_stock.Keys.ToArray ();
			foreach ( string stockStringTrolley in stockStringTrolleyTempArray )
			{
				Stock stock = TrolleyBeingUsed.m_stock[stockStringTrolley][0];
				if ( TryTakeStock ( stock, TrolleyBeingUsed ) == false)
				{
					Debug.Log("Tried to pick up stock from our trolley, but failed.");
				}
				if ( _furn.TryAddStock ( TryGiveStock ( stock.IDName ) ) == false)
				{
					Debug.Log ( "Failed to give stock to " + _furn.m_name + "." );
					stock.m_triedGoingOut = true;
					if ( _furn.m_full == true )
					{
						Debug.LogWarning ( _furn.m_name + " is full." );
						return false;
					}
				}
			}
		}

		//Loop through each different type of stock in the furniture
		foreach ( string stockStringFurn in stockStringFurnTempArray )
		{
			string[] stockStringTrolleyTempArray = TrolleyBeingUsed.m_stock.Keys.ToArray();

			//Loop through each different type of stock in the trolley
			foreach ( string stockStringTrolley in stockStringTrolleyTempArray )
			{
				//If the two match, we can try putting the stock from the trolley to the furniture.
				if ( stockStringFurn == stockStringTrolley )
				{
					Stock[] stockTempArray = TrolleyBeingUsed.m_stock[stockStringTrolley].ToArray();

					//Loop through each stock item of the given name, and try and add if to the furniture from the trolley.
					foreach ( Stock stock in stockTempArray )
					{
						if ( TryTakeStock ( stock, TrolleyBeingUsed ) == false)
						{
							Debug.Log("Tried to pick up stock from our trolley, but failed.");
						}
						if ( _furn.TryAddStock ( TryGiveStock ( stock.IDName ) ) == false)
						{
							Debug.Log ( "Failed to give stock to " + _furn.m_name + "." );
							stock.m_triedGoingOut = true;
							if ( _furn.m_full == true )
							{
								Debug.LogWarning ( _furn.m_name + " is full." );
								return false;
							}
						}
					}
				}
			}
		}

		// If we get here, either the trolley is empty, or there is no stock with the same name as the stock in the furniture
		return true;

	}

	/// <summary>
	/// Finds the first furniture with specified stock, with specified type, i.e. FrontShelf, BackShelf
	///	Only finds the furniture, and returns it. No internal logic
	/// </summary>
	Furniture FindFurnWithStockOfType ( Stock _stock, bool _front = true )
	{
		foreach ( string name in m_world.m_furnitureInWorld.Keys )
		{
			foreach ( Furniture f in m_world.m_furnitureInWorld[name] )
			{
				if ( f.m_name == "Wall" )
				{
					continue;
				}
				if ( _front )
				{
					if ( f.m_name != "FrontShelf" && f.m_name != "BigFridge" && f.m_name != "BigFreezer" )
					{
						continue;
					}
				}
				else
				{
					if ( f.m_name != "BackShelf" )
					{
						continue;
					}
				}

				//If we get here, the furniture will be of the appropriate name.
				foreach ( string stockName in f.m_stock.Keys )
				{
					if ( _stock.IDName == stockName )
					{
						if ( f.m_full == false )
						{
							return f;
						}
						else
						{
							Debug.LogWarning ( f.m_name + " is full." );
							break;
						}
					}
				}
			}
		}

		//If we get to here, there is no furniture in the entire world that has stock of the same name as the specified stock.
		//This means we need to search for a empty one now.

		foreach ( string name in m_world.m_furnitureInWorld.Keys )
		{
			foreach ( Furniture f in m_world.m_furnitureInWorld[name] )
			{
				if ( f.m_name == "Wall" )
				{
					continue;
				}
				if ( _front )
				{
					if ( f.m_name != "FrontShelf" && f.m_name != "BigFridge" && f.m_name != "BigFreezer" )
					{
						continue;
					}
				}
				else
				{
					if ( f.m_name != "BackShelf" )
					{
						continue;
					}
				}

				//If we get here, the furniture will be of the appropriate name.

				//Work out if it is empty.
				if ( f.m_stock.Count == 0 )
				{
					return f;
				}
			}
		}

		//If we get here, there is no place in the world for this stock, and no empty places either. TODO: This is when we would create overflow stockcages.

		return null;
	}
	
}
