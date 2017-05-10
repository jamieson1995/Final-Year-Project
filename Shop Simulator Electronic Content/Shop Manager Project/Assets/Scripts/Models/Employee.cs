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

	/// Reference to the Furniture instance that is being used as the Trolley for this employee's current Job.
	public Furniture TrolleyBeingUsed { get; protected set; }

	/// Refeerence to the customer this employee is serving.
	Customer m_servingCustomer;

	/// Cost of this transaction.
	int transactionCost;

	/// Flag to determine if this employee is busy.
	public bool m_busy { get; protected set; }

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
				foreach ( KeyValuePair<int, Employee> employee in m_world.m_employeesInWorld)
				{
					if ( employee.Value.InCharge )
					{
						Debug.LogError ( "Already an employee in charge: " + employee.Value.m_name );
						return;
					}
				}
				m_inCharge = true;
			}
		}
	}

	public Employee() : base ()
	{

	}

	/// Spawns a character, and in turn employee, with the specified name, on the specified Tile, and with the specified title.
	public Employee ( string _name, Tile _tile, Title _title, int _age ) : base ( _name, _age, _tile )
	{
		SetJobTitle(_title);
	}

	/// Returns a new Job that is created based upon world circumstances.
	Job CreateJob ( Job _oldJob, bool _overrideCurrentJob = false, bool _forceCheckout = false )
	{
		if ( _forceCheckout )
		{
			return CreateCheckoutJob(_overrideCurrentJob);
		}

		World world = WorldController.instance.m_world;

		if ( TrolleyBeingUsed != null )
		{
			TrolleyBeingUsed.m_manned = false;
			m_world.m_characterFurniture.Remove ( TrolleyBeingUsed );
			TrolleyBeingUsed = null;
			m_movingFurniture = null;
			m_movingFurnsFinalTile = null;
		}

		if ( _overrideCurrentJob )
		{
			if ( ( world.m_numberOfMannedCheckouts == 0 && world.m_customersInStore > 0 ) || world.m_customersInQueue != 0 && ( world.m_customersInQueue / world.m_numberOfMannedCheckouts ) > 3 )
			{
				return CreateCheckoutJob ( _overrideCurrentJob );
			}
		}

		if ( ( world.m_numberOfMannedCheckouts == 0 && world.m_customersInStore > 0 ) || world.m_customersInQueue != 0 && ( world.m_customersInQueue / world.m_numberOfMannedCheckouts ) > 3 )
		{
			return CreateCheckoutJob ( _overrideCurrentJob );

		}
		else if ( m_world.AllStockcagesEmpty == false )
		{
			
			return new Job ( Job.PrimaryStates.WorkStockcage, _oldJob );
		}
		else if ( m_world.AllWarehouseStockWorked == false )
		{
			return new Job ( Job.PrimaryStates.WorkBackStock, _oldJob );
		}
		else if ( m_world.AllFacingUpFinished == false )
		{
			return new Job ( Job.PrimaryStates.FaceUp, _oldJob );
		}
		else
		{
			return CreateCheckoutJob(_overrideCurrentJob);
		}
	}

	/// Returns the job created. Creates a new job with Primary State of ServeOnCheckout. Flag to determine if the current job should be overridden.
	Job CreateCheckoutJob ( bool _overrideCurrentJob = false )
	{
		m_world.m_numberOfMannedCheckouts++;

		Job j = new Job ( Job.PrimaryStates.ServeOnCheckout, m_job );

		if ( _overrideCurrentJob )
		{
			m_job = j;
		}

		return j;
	}

	/// Returns created job.
	public Job GiveJob (Job _oldJob)
	{
		return CreateJob(_oldJob);
	}

	/// If employee is in charge, this returns a new job. If not, the employee will seek out an employee that can give them a job.
	void FindJob ()
	{
		if ( InCharge )
		{
			m_job = CreateJob (m_job);
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
				m_job = CreateJob (m_job, false, true);
			}
			return;
		}

	}

	/// Sets the job title variables.
	public void SetJobTitle ( Title _title )
	{
		m_title = _title;
		switch ( _title )
		{
			case Title.Manager:
				foreach ( KeyValuePair<int, Employee> emp in m_world.m_employeesInWorld )
				{
					if ( emp.Value.m_title == Title.Manager )
					{
						Debug.LogError("Already have a Manager. Cannot set another.");
						return;
					}
				}
				m_titleString = "Manager";
				break;
			case Title.AssistantManager:

				foreach ( KeyValuePair<int, Employee> emp in m_world.m_employeesInWorld )
				{
					if ( emp.Value.m_title == Title.AssistantManager )
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
	protected override void OnUpdate_DoThink ( float _deltaTime )
	{
		if ( m_job == null )
		{
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

			FindJob ();
			if ( m_job == null )
			{
				return;
			}
		}

		//If we get to here, the employee has a job.
		//We need to check to see if more staff are needed at the checkouts.

		//If we are in charge, we we are serving at the checkout, we should know if more checkout staff is required.
		if ( InCharge || m_job.m_primaryState == Job.PrimaryStates.ServeOnCheckout )
		{

			if ( ( m_world.m_numberOfMannedCheckouts == 0 && m_world.m_customersInStore > 0 ) || m_world.m_customersInQueue != 0 && ( m_world.m_customersInQueue / m_world.m_numberOfMannedCheckouts ) > 3 )
			{
				m_world.RequireMoreCheckoutStaff = true;
			}
			else
			{
				m_world.RequireMoreCheckoutStaff = false;
			}
		}

		if ( m_world.RequireMoreCheckoutStaff && m_job.m_primaryState != Job.PrimaryStates.ServeOnCheckout)
		{
			CreateJob (m_job,  true );
		}

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
				Update_DoJob ( _deltaTime );
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
								f.m_manned = true;
								TrolleyBeingUsed = f;
								if ( m_world.m_characterFurniture.ContainsKey ( TrolleyBeingUsed ) )
								{
									m_world.m_characterFurniture.Remove ( TrolleyBeingUsed );
								}
								m_world.m_characterFurniture.Add ( TrolleyBeingUsed, this );
								break;
							}
						}
						if ( TrolleyBeingUsed == null )
						{
							//We looked for a free trolley, but there wasn't one.
							Debug.LogWarning ( "Looked for a trolley for job: " + m_job.m_primaryState + " but couldn't find a free one." );
							WorldController.instance.EC.BackStockWorked.Invoke();
							ChangeJobPrimaryState(Job.PrimaryStates.FaceUp);
							return;
						}
					}
					if ( TrolleyBeingUsed.m_moving == false )
					{
						if ( m_currTile.IsNeighbour ( TrolleyBeingUsed.m_mainTile, true, true ) == false && m_currTile != TrolleyBeingUsed.m_mainTile )
						{
							//We are not next to the trolley we need to move, so go there.
							Path_AStar TEMPPath = new Path_AStar ( m_world, m_currTile, TrolleyBeingUsed.m_mainTile );
							//We need a temporary path because we don't want to go to where the trolley is, we need to go to the tile one from it.
							Tile tile = TEMPPath.InitialPathToArray () [ TEMPPath.InitialPathToArray ().Length - 2 ];
							if ( m_destTile != tile )
							{
								SetDestination ( tile );
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
				if ( m_job.m_primaryState == Job.PrimaryStates.ServeOnCheckout )
				{
					if ( m_job.m_furn.m_stock.Count > 0 )
					{
						m_job.SetSecondaryState(Job.SecondaryStates.Use);
					}
					if ( ( m_world.m_numberOfMannedCheckouts == 1 && m_world.m_customersInStore > 0 ) || m_world.m_customersInQueue != 0 && ( m_world.m_customersInQueue / m_world.m_numberOfMannedCheckouts + 1 ) > 3 )
					{
						//We need to stay.

						//Check to see if there is stock on the checkout.
						if ( m_servingCustomer != null && m_servingCustomer.FinishedWithTransaction() == false )
						{
							m_job.SetSecondaryState(Job.SecondaryStates.Use);
						}
					}
					else
					{
						//We can change job.
						CreateJob(m_job);
					}

				}
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


				bool transactionFinishedThisFrame = false;

				if ( TryTakeAnyStock ( m_job.m_furn, true ) == false )
				{
					if ( inTransaction )
					{
						//Ask the customer if they are done with the tranaction.
						if ( m_servingCustomer.FinishedWithTransaction () )
						{
							//Pick up money.
							m_job.m_furn.m_money += transactionCost;
							WorldController.instance.EC.TransactionEnded.Invoke();
							m_world.m_money+=transactionCost;
							m_servingCustomer = null;
							transactionCost = 0;
							inTransaction = false;
							transactionFinishedThisFrame = true;
							m_world.m_customersInQueue--;
							m_world.m_customersInStore--;
						}
					}
					else
					{
						m_job.SetSecondaryState ( Job.SecondaryStates.Idle );
						break;
					}
				}

				if ( inTransaction == false )
				{
					if ( transactionFinishedThisFrame == false )
					{
						inTransaction = true;
						if ( m_servingCustomer == null )
						{
							m_servingCustomer = FindCustomerBeingServed ();
						}
					}
					else
					{
						if ( m_interactingCharacter != null )
						{
							StopInteraction ( true );
						}
					}
				}

				if ( m_stock == null )
				{
					break;
				}

				transactionCost += m_stock.Price;
				ScanStockTill ( m_stock );
				if ( m_interactingCharacter != m_servingCustomer )
				{
					RequestInteraction ( m_servingCustomer, 30, InteractionType.TalkingAtCheckout );
				}
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
							m_job.SetJobFurn ( null, this );
							m_taskTile = null;
						}
						else
						{
							if ( TrolleyBeingUsed.TryAddStock ( TryGiveStock ( m_stock.IDName ) ) == false )
							{
								Debug.Log ( "Failed to give stock to trolley, it is probably full." );
								if ( m_stock != null )
								{
									m_job.m_furn.TryGiveStock ( m_stock.IDName );
								}
							}
						}
					}
					else
					{
						//The trolley is full, so we should work the stock.
						m_job.SetWorkTrolleyState ( Job.WorkTrolleyStates.EmptyTrolleyToFront );
						m_job.SetJobFurn ( null, this );
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
							m_job.SetJobFurn ( null, this );
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
						m_job.SetJobFurn ( null, this );
						m_taskTile = null;
					}
				}
				else
				{
					WorkStockOnTrolley ();
				}

				break;
	
			case Job.PrimaryStates.FaceUp:

				//This furniture needs to be faced up unles it is at 100%.
				if ( m_job.m_furn.m_facedUpPerc >= 100 )
				{
					//All stock on the job's furniture has been faced up, so find a new furniture.
					m_job.SetJobFurn ( null, this );
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
	
			default:
	
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
					foreach ( Furniture furn in m_world.m_furnitureInWorld["BackShelf"].ToArray() )
					{
						if ( furn.m_stock.Count == 0 )
						{
							//This furniture has no stock, so go to the next one.
							continue;
						}
						else if ( furn.m_allStockWorked == true )
						{
							//All stock on this furniture has been worked, so go to the next one.
							continue;
						}
						else
						{
							if ( furn.m_actionTile.m_character != null )
							{
								continue;
							}
							m_job.SetJobFurn ( furn, this );
							if ( m_job.m_furn != null )
							{
								m_taskTile = m_job.m_furn.m_actionTile;
							}
							return true;
						}
					}

					//We did not find a BackShelf that needs working. So now we need to face up.
					m_world.AllWarehouseStockWorked = true;
					WorldController.instance.EC.BackStockWorked.Invoke();
					ChangeJobPrimaryState ( Job.PrimaryStates.FaceUp );
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
								m_job.SetJobFurn ( furn, this );	
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
								m_job.SetJobFurn ( furn, this );	
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
							if ( furn.m_actionTile.m_character != null )
							{
								continue;
							}
							m_job.SetJobFurn ( furn, this );
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
					m_world.AllStockcagesEmpty = true;
					WorldController.instance.EC.StockcageStockWorked.Invoke();
					ChangeJobPrimaryState ( Job.PrimaryStates.WorkBackStock );
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
								m_job.SetJobFurn ( furn, this );
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
						m_job.SetJobFurn ( furn, this );	
						if ( furn == null )
						{
							foreach ( Stock s in TrolleyBeingUsed.m_stock [ key ] )
							{
								s.m_triedGoingOut = true;
							}
							return false;
						}
						if ( m_job.m_furn != null )
						{
							m_taskTile = m_job.m_furn.m_actionTile;
						}
						return true;
					}
					//If we get here, there is no stock on the trolley.
					m_job.m_additionalInfoState = Job.WorkTrolleyStates.FillTrolley;
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
						if ( furn.m_actionTile.m_character != null )
						{
							continue;
						}

						m_job.SetJobFurn ( furn, this );
						if ( furn == null )
						{
							return false;
						}
						if ( m_job.m_furn != null )
						{
							m_taskTile = m_job.m_furn.m_actionTile;
							return true;
						}
					}
					else
					{
						continue;
					}
				}

				//If we get here, all the stock in the store has been faced up.
				m_world.AllFacingUpFinished = true;

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
						if ( furn.m_actionTile.m_character != null )
						{
							continue;
						}
						m_job.SetJobFurn ( furn, this );
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
				if ( m_job.m_furn != null )
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
						Debug.LogError ( "Job's tile is null." );
						return;
					}
					SetDestWithFurn ( m_taskTile, _furn );
				}
				else
				{
					if ( m_taskTile == null )
					{
						Debug.LogError ( "Job's tile is null." );
						return;
					}
					m_requiredFurn = m_job.m_furn;
					SetDestination ( m_taskTile );
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
			if ( m_taskTile != null && m_currTile != m_taskTile && m_destTile != m_taskTile )
			{
				//If we get here, we are not in the job's tile, and we are not going to the job's tile, but we have a trolley we need.
				SetDestWithFurn ( m_taskTile, _furn );	
				return;
			}
			else
			{
				//Trolley is at the job tile, so we need to now go there. We no longer require it, we now require the other furniture needed for the job.
				if ( m_taskTile == null )
				{
					Debug.LogWarning("Job's tile is null.");
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
			foreach ( Furniture furn in m_world.m_furnitureInWorld[name] )
			{
				if ( furn.m_name == "Wall" )
				{
					continue;
				}
				if ( _front )
				{
					if ( m_world.m_frontFurniture.Contains ( furn ) == false )
					{
						continue;
					}
				}
				else
				{
					if ( m_world.m_backFurniture.Contains ( furn ) == false )
					{
						continue;
					}
				}

				if ( furn.m_actionTile.m_character != null )
				{
					continue;
				}

				//If we get here, the furniture will be of the appropriate name.
				foreach ( string stockName in furn.m_stock.Keys )
				{
					if ( _stockIDName == stockName )
					{
						if ( _front == false )
						{
							if ( furn.m_full == false )
							{
								return furn;
							}
							else
							{
								Debug.LogWarning ( furn.m_name + " is full." );

								break;
							}
						}
						else
						{
							if ( m_currTile.IsNeighbour ( furn.m_actionTile, true, false ) )
							{
								if ( furn.m_full == false )
								{
									return furn;
								}
								else
								{
									Debug.LogWarning ( furn.m_name + " is full." );
								
									break;
								}
							}
							else
							{
								return furn;
							}
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
					if ( m_world.m_frontFurniture.Contains ( f ) == false )
					{
						continue;
					}
				}
				else
				{
					if ( m_world.m_backFurniture.Contains ( f ) == false )
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
			m_job.SetWorkTrolleyState ( Job.WorkTrolleyStates.EmptyTrolleyToBack );
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
			m_job.SetJobFurn(null, this);
			m_taskTile = null;
		}
	}

	void ChangeJobPrimaryState ( Job.PrimaryStates _newState, Job _oldJob = null )
	{
		if ( TrolleyBeingUsed != null )
		{
			TrolleyBeingUsed.m_manned = false;
			TrolleyBeingUsed.m_moving = false;
			TrolleyBeingUsed = null;
		}
		m_movingFurniture = null;
		m_job.SetPrimaryState(_newState, _oldJob);
	}

	/// Faces up the specified stock, and adjusts the stock's and job's furniture's face up flags and percentages.
	void FaceUp ( Stock _stock )
	{
		float percIncreased = ( (float)_stock.Weight / (float)m_job.m_furn.m_weightUsed ) * 100;
		_stock.m_facedUp = true;
		if ( m_job.m_furn.m_facedUpPerc + percIncreased > 100f )
		{
			percIncreased = 100f - m_job.m_furn.m_facedUpPerc;
		}
		m_job.m_furn.ChangeFaceUpPerc ( percIncreased );

		m_movingStock = true;
		m_fullTimeToMoveStock = 1.0f + ( (float)_stock.Weight / 1000.0f );
	
		if ( m_job.m_furn.m_facedUpPerc >= 100f )
		{
			m_job.m_furn.m_worked = true;
			m_job.SetJobFurn ( null, this );
			m_taskTile = null;
			return;
		}
	}

	Customer FindCustomerBeingServed ()
	{
		//To find the customer in the front of the queue, the one we are serving.
		//We need to floodfill from the center of the till to a tile with a queue number of 1.

		Tile frontOfQueue = new FindFreeQueueTile ( m_world, m_job.m_furn.m_middleTile, true ).m_tileFound;
		if ( frontOfQueue.m_character == null)
		{
			Debug.Log("FindCustomerBeingServed -- No character in given tile.");
		}

		return (Customer)frontOfQueue.m_character;
	}
}