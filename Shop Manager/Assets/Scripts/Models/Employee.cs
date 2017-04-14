//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// Possible titles for employees.
	public enum Title { Manager, AssistantManager, Supervisor, CustomerServiceAssistant }

/// <summary>
/// An employee is a character that can be given jobs.
/// </summary>
public class Employee : Character {

	/// Reference to the Job this employee is doing.
	Job m_job;

	/// Employee's Title as a string.
	string m_titleString;

	/// Employee's Title as an enum.
	public Title m_title { get; protected set; }

	/// Flag that determines if all stockcages have been worked or not, according to this employee's knowledge.
	bool? m_allStockcagesWorked;

	/// Reference to the Furniture instance that is being used as the Trolley for this employee's current Job.
	public Furniture TrolleyBeingUsed { get; protected set; }

	/// Flag to determine if this employee is in charge of the shop.
	bool m_inCharge;

	/// Property for m_inCharge.
	public bool InCharge
	{
		get
		{
			if ( m_world.InCharge == this )
			{
				m_inCharge = true;
				return m_inCharge;
			}
			else
			{
				m_inCharge = false;
				return m_inCharge;
			}
		}
		set
		{
			if ( value == false )
			{
				m_inCharge = false;
			}
			else
			{
				foreach ( Employee employee in m_world.m_employees )
				{
					if ( employee.InCharge )
					{
						Debug.LogError ( "Already an employee in charge: " + employee.m_name );
						return;
					}
				}
				m_inCharge = true;
			}
		}
	}

	/// Spawns a character, and in turn employee, with the specified name, specified maxCarryWeight, on the specified Tile, and with the specified title.
	public Employee ( string _name, int _maxCarryWeight, Tile _tile, Title _title ) : base ( _name, _maxCarryWeight, _tile )
	{
		SetJobTitle(_title);
	}

	/// Returns a new Job that is created based upon world circumstances.
	Job CreateJob ()
	{
		World world = WorldController.instance.m_world;

		if ( ( world.m_numberOfMannedCheckouts == 0 && world.m_customersInStore > 0 ) || world.m_customersInQueue != 0 && ( world.m_customersInQueue / world.m_numberOfMannedCheckouts ) > 3 )
		{
			Debug.Log("Started new job - Serve on Checkout");
			return new Job ( Job.PrimaryStates.ServeOnCheckout );
		}
		else
		{
			Debug.Log("Started new job - Work Stockcage");
			return new Job ( Job.PrimaryStates.FaceUp );
		}
	}

	public Job GiveJob ()
	{
		return CreateJob();
	}

	/// If employee is in charge, this returns a new job. If not, the employee will seek out an employee that can give them a job.
	Job FindJob ()
	{
		if ( InCharge )
		{
			return CreateJob ();
		}
		else
		{
			//Find a path to the employee in charge
			Employee employee = m_world.InCharge;

			Path_AStar TempPath = new Path_AStar ( m_world, m_currTile, employee.m_currTile );
			//Set destination 1 tile away from the employee in charge.
			if ( m_currTile.IsNeighbour ( employee.m_currTile, true, true ) == false )
			{
				SetDestination ( TempPath.InitialPathToArray () [ TempPath.Length () - 2 ] );
			}
			else
			{
				//The employee will now ask the employee in charge for a job.
				if ( RequestInteraction ( employee, 2.0f ) )
				{
					//Ask for job
					Job j = employee.GiveJob();
					return j;
				}
				else
				{
					Debug.Log(employee.m_name + " said no to interacting with " + m_name);
				}

			}
			return null;
		}

	}

	/// Sets the job title variables.
	public void SetJobTitle ( Title _title )
	{
		m_title = _title;
		switch ( _title )
		{
			case Title.Manager:
				foreach ( Employee emp in m_world.m_characters.ToArray() )
				{
					if ( emp.m_title == Title.Manager )
					{
						Debug.LogError("Already have a Manager. Cannot set another.");
						return;
					}
				}
				m_titleString = "Manager";
				break;
			case Title.AssistantManager:

				foreach ( Employee emp in m_world.m_characters.ToArray() )
				{
					if ( emp.m_title == Title.AssistantManager )
					{
						Debug.LogError("Already have an Assistant Manager. Cannot set another.");
						return;
					}
				}
				m_titleString = "Assistant Manager";

				break;

			case Title.CustomerServiceAssistant:

				m_titleString = "Customer Service Assistant";

				break;
			default:

				m_titleString = _title.ToString();

				break;
		}
	}

	/// The overridden Update_DoThink function from Character. Processes the employee's brain for this frame.
	protected override void Update_DoThink ( float _deltaTime )
	{

		if ( m_interacting != null )
		{
			if ( m_walkAndTalk )
			{
				m_canMove = true;
			}
			else
			{
				m_canMove = false;
			}
			if ( m_authorityLevel >= m_interacting.m_authorityLevel )
			{
				//The character we are interacting with is has a bigger authority than us. So keep up with them.
				if (m_currTile.IsNeighbour(m_interacting.m_currTile, true, true) == false)
				{
					Path_AStar TempPath = new Path_AStar(m_world, m_currTile, m_interacting.m_currTile);
					SetDestination(TempPath.InitialPathToArray()[TempPath.Length()-2]);
				}	
			}
			m_elaspedTimeInteraction += _deltaTime;
			if ( m_elaspedTimeInteraction >= m_fullTimeInteraction )
			{
				m_interacting = null;
				m_canMove = true;
				m_ignoreTask = false;
				StopInteraction();
			}	
			return;
		}
		else
		{
			m_canMove = true;
		}
		
		if ( m_currTile == m_destTile )
		{
			m_ignoreTask = false;
		}

		//If the character ignore its current task, it doesn't need to think about what to do next. It should just carry on moving to where its going.
		if ( m_ignoreTask == true )
		{
			return;
		}

		// All employees need a job, so if it hasn't got one it needs to create one.
		if ( m_job == null )
		{
			m_job = FindJob ();
			if ( m_job == null )
			{
				return;
			}
		}

		//If we get to here, the employee has a job.

		//Is the employee at the job tile?
		if ( m_job.Tile != null && m_currTile == m_job.Tile )
		{
			//Employee is at the job tile, so needs to use the furniture if it can.
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
				Update_DoJob (_deltaTime);
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
								//The trolley we found is already manned, so skip it.
								continue;
							}
							else
							{
								TrolleyBeingUsed = f;
								f.m_manned = true;
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
						if ( m_currTile.IsNeighbour ( TrolleyBeingUsed.m_mainTile, false, true ) == false && m_currTile != TrolleyBeingUsed.m_mainTile )
						{
							//We are not next to the trolley we need to move, so go there.
							Path_AStar TEMPPath = new Path_AStar(m_world, m_currTile, TrolleyBeingUsed.m_mainTile);
							//We need a temporary path because we don't want to go to where the trolley is, we need to go to the tile one from it.
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
								GoToTaskTile ( TrolleyBeingUsed );
							}
							else
							{
								m_movingFurniture = null;
								m_pathAStar = null;
								GoToTaskTile ();
							}
						}
					}
				}
				else
				{
					GoToTaskTile ();
				}
				break;
			case  Job.SecondaryStates.Idle:
				Debug.Log(m_name + " is idle");
				break;
		}
	}

	/// This is the function that processes the employee's job thought process.
	void Update_DoJob ( float _deltaTime )
	{

		if ( m_movingStock )
		{
			//We are currently picking up a piece of stock.
			m_elaspedTimeToMoveStock += _deltaTime;

			if ( m_elaspedTimeToMoveStock >= m_fullTimeToMoveStock )
			{
				//We have picked up the stock.
				m_elaspedTimeToMoveStock = 0f;
				m_fullTimeToMoveStock = 0f;
				m_movingStock = false;
			}
			else
			{
				//We have not finished picking up the stock.
				return;
			}
		}

		if ( m_job == null )
		{
			//Employee does not have a job, so does not need to do anything.
			return;
		}

		switch ( m_job.m_primaryState )
		{
			case Job.PrimaryStates.ServeOnCheckout:

				m_world.m_numberOfMannedCheckouts++;

				if ( TryTakeAnyStock ( m_job.m_furn, true ) == false )
				{
					//There is no more stock to take on this till. TODO later, when customers are implemented, the employee will finish the transaction.
					m_job.SetSecondaryState ( Job.SecondaryStates.Idle );
					break;
				}
				ScanStockTill ( m_stock );
				m_job.m_furn.TryAddStock ( TryGiveStock ( m_stock.IDName ) );
				break;
	
			case Job.PrimaryStates.WorkStockcage:

				if ( m_job.m_additionalInfoState == Job.WorkTrolleyStates.FillTrolley )
				{	
					if ( m_job.m_furn.m_name != "Stockcage" )
					{
						Debug.LogError ( "We are at the job tile, and we are set to fill the trolley, but our job's furniture isn't a stockcage" );
						return;
					}
					if ( TrolleyBeingUsed.m_full == false )
					{
						if ( TryTakeAnyStock ( m_job.m_furn ) == false )
						{
							Debug.Log ( "Failed to take any stock from stockcage, it is probably empty" );
							m_job.SetWorkTrolleyState ( Job.WorkTrolleyStates.EmptyTrolleyToFront );
							m_job.SetJobFurn ( null );
							m_taskTile = null;
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
						//The trolley is full, so we should work the stock.
						m_job.SetWorkTrolleyState ( Job.WorkTrolleyStates.EmptyTrolleyToFront );
						m_job.SetJobFurn ( null );
						m_taskTile = null;
					}
				}
				else
				{
					WorkStockOnTrolley ();
				}
				break;
	
			case Job.PrimaryStates.EmptyStockcage:
	
				break;
	
			case Job.PrimaryStates.WorkBackStock:

				if ( m_job.m_additionalInfoState == Job.WorkTrolleyStates.FillTrolley )
				{	
					if ( m_job.m_furn.m_name != "BackShelf" )
					{
						Debug.LogError ( "We are at the job tile, and we are set to fill the trolley, but our job's furniture isn't a BackShelf" );
						return;
					}
					if ( TrolleyBeingUsed.m_full == false )
					{
						if ( TryTakeAnyStock ( m_job.m_furn, false, true ) == false )
						{
							Debug.Log ( "Failed to take any stock from BackShelf, it is probably empty, or all the stock has already been worked." );
							m_job.m_furn.m_allStockWorked = true;
							m_job.SetWorkTrolleyState ( Job.WorkTrolleyStates.EmptyTrolleyToFront );
							m_job.SetJobFurn ( null );
							m_taskTile = null;
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
						//The trolley is full, so we should work the stock.
						m_job.SetWorkTrolleyState ( Job.WorkTrolleyStates.EmptyTrolleyToFront );
						m_job.SetJobFurn ( null );
						m_taskTile = null;
					}
				}
				else
				{
					WorkStockOnTrolley ();
				}

				break;
	
			case Job.PrimaryStates.FaceUp:

				//If our job's furniture's m_facedUpPerc is below 90%, we need to face up the furniture.
				//if ( m_job.m_furn.m_facedUpPerc <= 90 )
				//{
					//This furniture needs to be faced up.
				if ( m_job.m_furn.m_facedUpPerc >= 100 )
				{
					//All stock on the job's furniture has been faced up, so find a new furniture.
					m_job.SetJobFurn ( null );
					m_taskTile = null;
					return;
				}
				foreach ( string stockName in m_job.m_furn.m_stock.Keys.ToArray () )
				{
					int i = 0;
					while ( i < m_job.m_furn.m_stock [ stockName ].Count )
					{
						if ( m_job.m_furn.m_stock [ stockName ][i].m_facedUp == false )
						{
							FaceUp ( m_job.m_furn.m_stock [ stockName ][i] );
							return;
						}
						i++;
					}
					foreach ( Stock stock in m_job.m_furn.m_stock[stockName].ToArray() )
					{
						if ( stock.m_facedUp == false )
						{
							FaceUp ( stock );
							return;
						}
						else
						{
							continue;
						}
					}
				}

				break;
	
			case Job.PrimaryStates.CountCheckoutMoney:
	
				break;
		}
	}

	/// Returns the attempt's outcome. Attempts to find a furniture with the specified name. If successful, sets the job's furniture to it.
	bool FindJobFurn ( string _furn )
	{
		switch ( m_job.m_primaryState )
		{
			case Job.PrimaryStates.WorkBackStock:

				//We don't know what furniture we need, so work it out.
			//Find first type of stock in the trolley being used.
				if ( TrolleyBeingUsed == null )
				{
					Debug.Log ( "We don't have a trolley" );
					return false;
				}
				if ( TrolleyBeingUsed.m_stock.Count == 0 )
				{
					m_job.SetWorkTrolleyState ( Job.WorkTrolleyStates.FillTrolley );
				}

				if ( m_job.m_additionalInfoState == Job.WorkTrolleyStates.FillTrolley )
				{
					// We need to fill this trolley.
					foreach ( Furniture f in m_world.m_furnitureInWorld["BackShelf"].ToArray() )
					{
						if ( f.m_stock.Count == 0 )
						{
							//This furniture has no stock, so go to the next one.
							continue;
						}
						else if ( f.m_allStockWorked == true )
						{
							//All stock on this furniture has been worked, so go to the next one.
							continue;
						}
						else
						{
							m_job.SetJobFurn ( f );
							if ( m_job.m_furn != null )
							{
								m_taskTile = m_job.m_furn.m_actionTile;
							}
							return true;
						}
					}

					//We did not find a BackShelf that needs working. So now we need to face up.
					m_job.SetPrimaryState ( Job.PrimaryStates.FaceUp );
				}
				else if ( m_job.m_additionalInfoState == Job.WorkTrolleyStates.EmptyTrolleyToFront || m_job.m_additionalInfoState == Job.WorkTrolleyStates.EmptyTrolleyToBack )
				{
					// We need to work this stock.
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
							if ( m_job.m_additionalInfoState == Job.WorkTrolleyStates.EmptyTrolleyToFront )
							{
								Debug.Log ( "All stock on this trolley has been worked." );
								m_job.SetWorkTrolleyState ( Job.WorkTrolleyStates.EmptyTrolleyToBack );
								return false;
							}
							else if ( m_job.m_additionalInfoState == Job.WorkTrolleyStates.EmptyTrolleyToBack )
							{
								furn = FindFurnWithStockOfType ( TrolleyBeingUsed.m_stock [ key ] [ 0 ].IDName, false );
								m_job.SetJobFurn ( furn );	
								if ( furn == null )
								{
									return false;
								}
								if ( m_job.m_furn != null )
								{
									m_taskTile = m_job.m_furn.m_actionTile;
								}
								return true;
							}
						}
						foreach ( Stock s in TrolleyBeingUsed.m_stock [ key ].ToArray() )
						{
							if ( s.m_triedGoingOut == false )
							{
								furn = FindFurnWithStockOfType ( s.IDName, true );
								m_job.SetJobFurn ( furn );	
								if ( m_job.m_furn != null )
								{
									m_taskTile = m_job.m_furn.m_actionTile;
								}
								if ( furn == null )
								{
									s.m_triedGoingOut = true;
									return false;
								}
								return true;
							}
						}
					}
				}

				break;

			case Job.PrimaryStates.WorkStockcage:

				//Our trolley is empty.
				if ( m_job.m_additionalInfoState == Job.WorkTrolleyStates.FillTrolley )
				{
					//We need to fill our trolley with stock from a stockcage.
					if ( WorldController.instance.m_world.m_furnitureInWorld.ContainsKey ( _furn ) )
					{
						foreach ( Furniture furn in WorldController.instance.m_world.m_furnitureInWorld[_furn] )
						{
							if ( furn.m_stock.Count == 0 )
							{
								//This stockcage doesn't have stock. So skip it.
								continue;
							}
							m_job.SetJobFurn ( furn );
							if ( furn == null )
							{
								return false;
							}
							if ( m_job.m_furn != null )
							{
								m_taskTile = m_job.m_furn.m_actionTile;
							}
							return true;
						}	
					}
					Debug.Log ( "All stockcages worked. Changing job to WorkBackStock" );
					m_allStockcagesWorked = true;
					m_job.SetPrimaryState ( Job.PrimaryStates.WorkBackStock );
					return false;
				}
				else if ( m_job.m_additionalInfoState == Job.WorkTrolleyStates.EmptyTrolleyToFront || m_job.m_additionalInfoState == Job.WorkTrolleyStates.EmptyTrolleyToBack )
				{
					// We need to work this stock.
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
							if ( m_job.m_additionalInfoState == Job.WorkTrolleyStates.EmptyTrolleyToFront )
							{
								Debug.Log ( "All stock on this trolley has been worked." );
								m_job.SetWorkTrolleyState ( Job.WorkTrolleyStates.EmptyTrolleyToBack );
								return false;
							}
							else if ( m_job.m_additionalInfoState == Job.WorkTrolleyStates.EmptyTrolleyToBack )
							{
								furn = FindFurnWithStockOfType ( TrolleyBeingUsed.m_stock [ key ] [ 0 ].IDName, false );
								m_job.SetJobFurn ( furn );
								if ( furn == null )
								{
									return false;
								}	
								if ( m_job.m_furn != null )
								{
									m_taskTile = m_job.m_furn.m_actionTile;
								}
								return true;
							}
						}
						furn = FindFurnWithStockOfType ( TrolleyBeingUsed.m_stock [ key ] [ 0 ].IDName, true );
						m_job.SetJobFurn ( furn );	
						if ( furn == null )
						{
							foreach ( Stock s in TrolleyBeingUsed.m_stock [ key ] )
							{
								s.m_triedGoingOut = true;
							}
							//TrolleyBeingUsed.m_stock [ key ] [ 0 ].m_triedGoingOut = true;
							return false;
						}
						if ( m_job.m_furn != null )
						{
							m_taskTile = m_job.m_furn.m_actionTile;
						}
						return true;
					}
				}

				break;

			case Job.PrimaryStates.FaceUp:

				foreach ( Furniture furn in m_world.m_frontFurniture.ToArray() )
				{
					if ( furn.m_stock.Count == 0 )
					{
						furn.m_worked = true;
						continue;
					}
					if ( furn.m_worked == false )
					{
						m_job.SetJobFurn ( furn );
						if ( furn == null )
						{
							return false;
						}
						if ( m_job.m_furn != null )
						{
							m_taskTile = m_job.m_furn.m_actionTile;
						}
					}
					else
					{
						continue;
					}
				}

				break;

			default:

				if ( WorldController.instance.m_world.m_furnitureInWorld.ContainsKey ( _furn ) )
				{
					foreach ( Furniture furn in WorldController.instance.m_world.m_furnitureInWorld[_furn] )
					{
						if ( furn.m_manned == true )
						{
							continue;
						}
						m_job.SetJobFurn ( furn );
						if ( furn == null )
						{
							return false;
						}
						if ( m_job.m_furn != null )
						{
							m_taskTile = m_job.m_furn.m_actionTile;
						}
						if ( m_job.SetJobTile ( furn ) == false )
						{
							Debug.LogError ( "Failed to set job tile - _furn -> " + _furn + ", m_job.m_primaryState - > " + m_job.m_primaryState.ToString () );
							return false;
						}

						return true;
					}
				}

				break;
		}

		Debug.LogWarning ( "Failed to find job furniture - Furniture -> " + _furn + ", Job State -> " + m_job.m_primaryState.ToString () );
		return false;
	}


	/// Sets the employee's destination to the job's furniture's job tile. If the job doesn't have a furniture, it finds one. 
	/// If variable is not null, it moves the specifed furniture with the employee.
	void GoToTaskTile ( Furniture _furn = null )
	{
		if ( m_job.m_furn == null )
		{
			//We need a furniture for the job.
			if ( FindJobFurn ( m_job.m_requiredFurniture ) )
			{
				World m_world = WorldController.instance.m_world;
				if ( m_job.m_furn != null)
				{
					m_job.m_furn.m_manned = true;
				}
				if ( m_world.m_characterFurniture.ContainsValue ( this ) == false )
				{
					m_world.m_characterFurniture.Add ( m_job.m_furn, this );
				}
				if ( m_job.RequiresTrolley == true )
				{
					if ( m_taskTile == null )
					{
						Debug.LogError("Job's tile is null.");
						return;
					}
					SetDestWithFurn(m_taskTile, _furn);
				}
				else
				{
					if ( m_taskTile == null )
					{
						Debug.LogError("Job's tile is null.");
						return;
					}
					m_requiredFurn = m_job.m_furn;
					SetDestination ( m_taskTile);
				}
			}
		}
		if ( m_job.RequiresTrolley == false )
		{
			//We have a furniture for the job.
			if ( m_taskTile != null && m_currTile != m_taskTile && m_destTile != m_taskTile )
			{
				//If we get here, we are not in the job's tile, and we are not going to the job's tile, and we don't need a trolley.
				m_requiredFurn = m_job.m_furn;
				SetDestination ( m_taskTile );	
			}
		}
		else
		{
			if(m_taskTile != null && m_currTile != m_taskTile && m_destTile != m_taskTile)
			{
				//If we get here, we are not in the job's tile, and we are not going to the job's tile, but we have a trolley we need.
				SetDestWithFurn(m_taskTile, _furn);	
				return;
			}
			else
			{
				//Trolley is at the job tile, so we need to now go there. We no longer require it, we now require the other furniture needed for the job.
				if ( m_taskTile == null )
				{
					Debug.LogError("Job's tile is null.");
					return;
				}
				m_requiredFurn = m_job.m_furn;
				SetDestination(m_taskTile);
			}
		}
	}

	/// Processes the specifed stock, and sets its m_scanned variable to true.
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

	/// Returns the attempt's outcome. Attempts to move the specificed stock from TrolleyBeingWorked to the specified furniture.
	bool EmptyTrolley ( Stock _stock )
	{
		Furniture furn = m_job.m_furn;
		if ( m_currTile.IsNeighbour ( furn.m_middleTile ) == false )
		{
			Debug.LogError ( "Trying to empty a trolley onto a furniture we are not neighbours with" );
			return false;
		}

		if ( TryTakeStock ( _stock, TrolleyBeingUsed ) == false )
		{
			Debug.Log ( "Tried to pick up stock from our trolley, but failed." );
			return false;
		}
		if ( furn.TryAddStock ( TryGiveStock ( _stock.IDName ) ) == false)
		{
			Debug.Log ( "Failed to give stock to " + furn.m_name + "." );
			_stock.m_triedGoingOut = true;
			if ( furn.m_full == true )
			{
				Debug.LogWarning ( furn.m_name + " is full." );
				return false;
			}
		}

		return true;
		 
	}

	/// Returns the first furniture found with the specified stock's name. Specified _front refers to location of the furniture required.
	Furniture FindFurnWithStockOfType ( string _stockIDName, bool _front = true )
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
					if ( m_world.m_frontFurniture.Contains ( f ) == false)
					{
						continue;
					}
				}
				else
				{
					if ( m_world.m_backFurniture.Contains ( f ) == false)
					{
						continue;
					}
				}

				//If we get here, the furniture will be of the appropriate name.
				foreach ( string stockName in f.m_stock.Keys )
				{
					if ( _stockIDName == stockName )
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
					if ( m_world.m_frontFurniture.Contains ( f ) == false)
					{
						continue;
					}
				}
				else
				{
					if ( m_world.m_backFurniture.Contains ( f ) == false)
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

		if ( m_job.m_primaryState == Job.PrimaryStates.WorkBackStock )// || m_job.m_primaryState == Job.PrimaryStates.WorkStockcage)
		{
			m_job.SetWorkTrolleyState(Job.WorkTrolleyStates.EmptyTrolleyToBack);
		}

		//If we get here, there is no place in the world for this stock, and no empty places either. TODO: This is when we would create overflow stockcages.

		return null;
}

	void WorkStockOnTrolley ()
	{
		// We are at a furniture that has stock with the same name as on our trolley.

		// We are at the job's tile.

		if ( TrolleyBeingUsed.m_stock.Count == 0 )
		{
			//There is no stock on this trolley.
			m_job.SetWorkTrolleyState(Job.WorkTrolleyStates.FillTrolley);
		}

		if ( m_job.m_furn.m_stock.Count == 0 )
		{
			//The furniture has no stock on it, so take any stock from the trolley.

			if ( EmptyTrolley ( TrolleyBeingUsed.m_stock.First ().Value [ 0 ] ) == false )
			{
				Debug.Log ( "Failed to move " + TrolleyBeingUsed.m_stock.First ().Value [ 0 ] + " to the job's furniture." );
			}
			return;
		}
		else
		{
			//The furniture has stock, so compare it to the stock on the trolley, if they match, move the stock to the furniture.

			//Loop through each different type of stock in the furniture
			foreach ( string furnStockString in m_job.m_furn.m_stock.Keys.ToArray () )
			{
				//Loop through each different type of stock in the trolley
				foreach ( string trolleyStockString in TrolleyBeingUsed.m_stock.Keys.ToArray () )
				{
					//If the two match, we can try putting the stock from the trolley to the furniture.
					if ( furnStockString == trolleyStockString )
					{
						if ( EmptyTrolley ( TrolleyBeingUsed.m_stock [ trolleyStockString ] [ 0 ] ) == false)
						{
							Debug.Log ( "Failed to move " + m_job.m_furn.m_stock.First ().Value [ 0 ] + " to the job's furniture." );
						}
						else
						{
							//Movement was successful so we can stop looping through all the other stock.
							return;
						}
					}
					else
					{
						continue;
					}
				}
			}
			//If we get here, none of the stock in the furniture matches the stock in the trolley. So reset the job's furniture.
			m_job.SetJobFurn(null);
			m_taskTile = null;
		}
	}

	/// Faces up the specified stock, and adjusts the stock's and job's furniture's face up flags and percentages.
	void FaceUp (Stock _stock)
	{
		float percIncreased = ( (float)_stock.Weight / (float)m_job.m_furn.m_weightUsed ) * 100;
		_stock.m_facedUp = true;
		if ( m_job.m_furn.m_facedUpPerc + percIncreased > 100f )
		{
			percIncreased = 100f - m_job.m_furn.m_facedUpPerc;
		}
		m_job.m_furn.ChangeFaceUpPerc(percIncreased);
	
		if (m_job.m_furn.m_facedUpPerc == 100f)
		{
			m_job.m_furn.m_worked = true;
			m_job.SetJobFurn(null);
			m_taskTile = null;
			return;
		}
		else if (m_job.m_furn.m_facedUpPerc > 100f)
		{
			m_job.m_furn.m_worked = true;
			m_job.SetJobFurn(null);
			m_taskTile = null;
			return;
		}
	}
}