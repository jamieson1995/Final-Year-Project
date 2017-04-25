//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Customer : Character {

	List<World.ShoppingListItem> m_shoppingList;

	bool m_listCompleted;

	List<Furniture> m_searchedFurniture;

	Tile m_queueTile;

	PrimaryStates m_primaryState;

	Furniture m_currentFurniture;

	public bool m_paidForShopping { get; protected set; }

	public enum PrimaryStates
	{
		GoTo,
		FindItems,
		TillTransaction
	}

	public Customer() : base ()
	{

	}

	public Customer(string _name, int _age, Tile _tile) : base ( _name, _age, _tile )
	{
		m_shoppingList = new List<World.ShoppingListItem>();
		m_searchedFurniture = new List<Furniture>();
		PopulateShoppingList();
		m_basket = true;
	}

	public Customer ( Character c, Tile _tile )
	{
		m_world = WorldController.instance.m_world;
		
		//When a new charcater is created. Its ID will be the next ID avaiable, which is just the number of IDs + 1
		ID = c.ID;
		
		m_currTile = m_destTile = m_nextTile = _tile; //Set all three tile variables to tile the character will spawn on.
		m_taskTile = null;
		m_currentSpeed = m_maxSpeed;
		
		m_basketContents = new List<Stock> ();
		m_relationships = c.m_relationships;
		m_positivePersonTraits = c.m_positivePersonTraits;
		m_negativePersonTraits = c.m_negativePersonTraits;
		m_positivePhysTraits = c.m_positivePhysTraits;
		m_negativePhysTraits = c.m_negativePhysTraits;
		
		m_name = c.m_name;
		m_age = c.m_age;
		m_canInteract = true;
		
		if ( _tile != null )
		{
			_tile.ChangeCharacterInTile ( this );
		}

		m_shoppingList = new List<World.ShoppingListItem>();
		m_searchedFurniture = new List<Furniture>();
		PopulateShoppingList();
		m_basket = true;
	}

	void PopulateShoppingList ()
	{
		//Chooses a list from random between 1 and the maximum number of lists in World.
		int num = Random.Range(1, m_world.m_shoppingLists.Count + 1 );
		foreach ( World.ShoppingListItem item in m_world.m_shoppingLists [ num ].ToArray() )
		{
			m_shoppingList.Add(item);
		}
	}

	protected override void OnUpdate_DoThink ( float _deltaTime )
	{

		if ( m_taskTile != null && m_destTile != m_taskTile )
		{
			//Some how we are no longer moving to our task tile. So re-set it.
			SetDestination ( m_taskTile );
		}

		switch ( m_primaryState )
		{
			case ( PrimaryStates.GoTo ):

				if ( m_paidForShopping )
				{
					//We have paid for the shopping, and are leaving the store.
					if ( m_taskTile != null && m_currTile == m_taskTile )
					{
						m_world.RemoveCharacterFromWorld ( this );
					}
					return;
				}

				if ( m_taskTile == null )
				{
					//We should be moving to a tile, but we don't know what tile we should be moving to.
					if ( m_listCompleted == false )
					{
						m_listCompleted = true;

						foreach ( World.ShoppingListItem item in m_shoppingList )
						{
							if ( item.pickedUp == false )
							{
								m_listCompleted = false;
							}
						}
					}

					if ( m_listCompleted )
					{
						if ( m_currTile == m_queueTile )
						{
							//We are at our queue tile. Wait in queue.
							if ( m_currTile.m_queue && m_currTile.m_queueNum != 1 )
							{
								//We are in a queue, but not at the front. Check to see if we can move up in the queue.
								foreach ( Tile t in m_currTile.GetNeighbours(true) )
								{
									if ( t.m_queue )
									{
										if ( t.m_queueNum == m_currTile.m_queueNum-1 )
										{
											//We have found a neighbour with a smaller queueNum.
											if ( t.m_character == null )
											{
												//It is unoccupied.
												SetDestination ( t );
												m_queueTile = t;
												m_inQueue = true;
											}
										}
									}
								}
							}
							else
							{
								m_primaryState = PrimaryStates.TillTransaction;
							}
						}
						else if ( m_destTile == m_queueTile )
						{
							//We are heading to our queue tile.
							return;
						}
						else
						{
							Tile t = new FindFreeQueueTile ( m_world, m_currTile ).m_tileFound;
							SetDestination ( t );
							m_primaryState = PrimaryStates.GoTo;
							m_queueTile = t;
							m_world.m_customersInQueue++;
						}
					}
					else
					{
						//Look for another possible furniture that could contain the item we need.
						foreach ( Furniture furn in m_world.m_frontFurniture.ToArray().Reverse() )
						{
							if ( m_searchedFurniture.Contains ( furn ) == false )
							{
								m_taskTile = furn.m_actionTile;
								SetDestination ( m_taskTile );
								m_primaryState = PrimaryStates.GoTo;
								m_currentFurniture = furn;
								return;
							}
						}

						//If we get here, we have searched every piece of furniture but we still have an uncompleted list.
						//TODO: We may want to look for staff, to ask them if there are any out the back

						ResolveShopRelationshopLevel(-1);
						m_listCompleted = true;
						//This is when we make a note that we didn't find all our items.
					}
					
				}
				else
				{
					if ( m_currTile == m_taskTile )
					{
						//We are at the tile we need to be at to change state and perfrom a task.
						foreach ( Furniture furn in m_searchedFurniture )
						{
							if ( furn.m_actionTile == m_taskTile )
							{
								//We have already searched this furniture for items, we need to find another.
								m_primaryState = PrimaryStates.GoTo;
								return;
							}
						}
				
						//If we get here, we haven't searched this furniture yet for items.
						m_primaryState = PrimaryStates.FindItems;
					}
					else
					{
						//We have a task tile that is different from our current tile, 
						//this means the task tile SHOULD be the same as our destination tile

						if ( m_destTile != m_taskTile )
						{
							Debug.LogWarning ( "Our destination tile was not the same as our task tile." );
							SetDestination ( m_taskTile );
						}
						else
						{
							return;
						}
					}
				}
				
				break;

			case (PrimaryStates.FindItems):

				Update_DoTask(_deltaTime);

				break;

			case (PrimaryStates.TillTransaction):

				Update_DoTask(_deltaTime);

				break;

			default:

				break;
		}

	}

	void Update_DoTask ( float _deltaTime )
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

		switch ( m_primaryState )
		{
			case (PrimaryStates.FindItems):

				bool foundItem = false;

				foreach ( string stockName in m_currentFurniture.m_stock.Keys.ToArray () )
				{
					foreach ( World.ShoppingListItem item in m_shoppingList )
					{
						if ( item.pickedUp )
						{
							//Item has already been picked up, so skip it.
							continue;
						}

						if ( item.stock.IDName == stockName )
						{
							//We have found an item we need.
							if ( TryTakeStock ( m_currentFurniture.m_stock [ stockName ] [ 0 ], m_currentFurniture ) == false )
							{
								Debug.Log ( "Failed to pick up stock from our current furniture." );
							}
							else
							{

								m_currentFurniture.ChangeFaceUpPerc ( (float)-( (float)m_stock.Weight / (float)m_currentFurniture.m_maxCarryWeight ) * 100f );

								for ( int i = 0; i < m_shoppingList.Count; i++ )
								{
									if ( stockName == m_shoppingList [ i ].stock.IDName )
									{
										m_shoppingList [ i ] = new World.ShoppingListItem ( item.stock, item.price, true, false );
										foundItem = true;
									}
								}
							}
						}
					}
				}

				//If we get here, there are no more stock items in this furniture that we need to pick up.
				if ( foundItem == false )
				{
					m_searchedFurniture.Add ( m_currentFurniture );
					m_currentFurniture = null;
					m_movingStock = true;
					m_fullTimeToMoveStock = 5f;
				}
				m_primaryState = PrimaryStates.GoTo;
				m_taskTile = null;

				break;

			case (PrimaryStates.TillTransaction):

				if ( m_queueTile != null && m_queueTile.m_queueNum == 1 )
				{
					//We are next to a checkout.
				if ( m_currentFurniture == null || m_currentFurniture.m_name != "Checkout" )
				{
					//Our current furniture isn't a checkout, find the checkout next to us.
					foreach ( Furniture furn in m_currTile.GetNeighboursFurniture() )
					{
						if ( furn != null && furn.m_name == "Checkout" )
						{
							m_taskTile = m_currTile;
							m_currentFurniture = furn;
						}
					}
				}
				else
				{
					//We are at the checkout, so we can put our items onto it.

					bool unscannedItemsInBasket = true;


					if ( m_basketContents.Count > 0 )
					{
						//Our basket isn't empty, but we need to check to see if the first stock in our basket has been scanned.
						//It may not have been.
						if ( m_basketContents [ 0 ].m_scanned == false )
						{
							unscannedItemsInBasket = true;
						}
						else
						{
							if ( m_currentFurniture.m_stock.Count == 0 )
							{
								//There is no more stock on the till. So now we need to pay.
								if ( m_paidForShopping )
								{
									//We have already paid for the shopping, so we can leave.
									m_primaryState = PrimaryStates.GoTo;
									SetDestination ( m_world.GetTileAt ( UnityEngine.Random.Range ( 0, m_world.m_width), 0 ) );
									m_taskTile = m_destTile;
									m_currentFurniture = null;
									return;
								}
								else
								{
									m_paidForShopping = true;	
								}
							}
							//We have items in our basket, but it has been scanned already.
							foreach ( string name in m_currentFurniture.m_stock.Keys.ToArray() )
							{
								foreach ( Stock stock in m_currentFurniture.m_stock[name].ToArray() )
								{
									if ( stock.m_scanned )
									{
										TryTakeStock ( stock, m_currentFurniture );
										m_scannedItemsInBasket = true;
										return;
									}
								}
							}

							//If we get here then maybe unscannedItemsInBasket = false?
							unscannedItemsInBasket = false;
						}

					}
					else
					{
						//We have no items in our basket.
						unscannedItemsInBasket = false;
						foreach ( string name in m_currentFurniture.m_stock.Keys.ToArray() )
						{
							foreach ( Stock stock in m_currentFurniture.m_stock[name].ToArray() )
							{
								if ( stock.m_scanned )
								{
									TryTakeStock ( stock, m_currentFurniture );
									m_scannedItemsInBasket = true;
									return;
								}
							}
						}
					}


					if (unscannedItemsInBasket)
					{
						if ( m_basketContents [ 0 ].m_scanned == false )
						{
							//The next item in our basket hasn't been scanned, so put it on the till.
							if ( m_currentFurniture.TryAddStock ( TryGiveStock ( m_basketContents [ 0 ].IDName ) ) )
							{
								//m_basketContents.RemoveAt(0);
							}
						}
						else
						{
							//Our first item has been scanned, so all our items should have been scanned.
							//bool itemNotScanned = false;
							foreach ( Stock stock in m_basketContents.ToArray() )
							{
								if ( stock.m_scanned == false )
								{
									//We have found an item that han't been scanned. We need to set this to the front of the list.
									//Therefore we need to remove it, add it to the end, then reverse the list so its in the front.
									//itemNotScanned = true;
									m_basketContents.Remove ( stock );
									m_basketContents.Add ( stock );
									m_basketContents.Reverse ();								
								}
							}
						}
					}
				}
				}

				break;

			default:

				break;
		}
	}

	public bool FinishedWithTransaction ()
	{
		return m_paidForShopping;
	}
}
