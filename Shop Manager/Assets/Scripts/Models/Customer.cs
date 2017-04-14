using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Customer : Character {

	List<World.ShoppingListItem> m_shoppingList;

	bool m_listCompleted;

	List<Furniture> m_searchedFurniture;

	PrimaryStates m_primaryState;

	Furniture m_currentFurniture;

	public enum PrimaryStates
	{
		GoTo,
		FillBasket,
		EmptyBasket
	}


	public Customer(string _name, int _maxCarryWeight, Tile _tile) : base ( _name, _maxCarryWeight, _tile )
	{
		m_shoppingList = new List<World.ShoppingListItem>();
		m_searchedFurniture = new List<Furniture>();
		PopulateShoppingList();
		m_basket = true;
	}

	void PopulateShoppingList ()
	{
		//Chooses a list from random between 1 and the maximum number of lists in World.
		int num = Random.Range(1, m_world.m_shoppingLists.Count + 1 );
		m_shoppingList = m_world.m_shoppingLists [ num ];
	}

	protected override void Update_DoThink ( float _deltaTime )
	{
		//If the character ignore its current task, it doesn't need to think about what to do next. It should just carry on moving to where its going.
		if ( m_ignoreTask == true )
		{
			return;
		}

		if ( m_interacting != null )
		{
			m_canMove = false;
		}
		else
		{
			m_canMove = true;
		}

		switch ( m_primaryState )
		{
			case ( PrimaryStates.GoTo ):

				if ( m_taskTile == null )
				{
					//We should be moving to a tile, but we don't know what tile we should be moving to.

					m_listCompleted = true;

					foreach ( World.ShoppingListItem item in m_shoppingList )
					{
						if (item.pickedUp == false)
						{
							m_listCompleted = false;
						}
					}

					if ( m_listCompleted )
					{
						//TODO: Enter Queue
						Debug.Log("Enter Queue.");
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
						Debug.LogWarning ( "All furniture searched, but uncompleted list." );
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
						m_primaryState = PrimaryStates.FillBasket;
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

			case (PrimaryStates.FillBasket):

				Update_DoTask(_deltaTime);

				break;

			case (PrimaryStates.EmptyBasket):

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
			case (PrimaryStates.FillBasket):

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
							//We have found an item we need!
							Debug.Log ( "Found item - " +  item.stock.Name);
							//m_currentFurniture.m_stock[stockName][0];
							if ( TryTakeStock ( m_currentFurniture.m_stock [ stockName ] [ 0 ], m_currentFurniture ) == false )
							{
								Debug.Log ( "Failed to pick up stock from our current furniture." );
							}
							else
							{
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
					m_searchedFurniture.Add(m_currentFurniture);
					m_currentFurniture = null;
				}
				m_primaryState = PrimaryStates.GoTo;
				m_taskTile = null;

				break;

			case (PrimaryStates.EmptyBasket):

				break;

			default:

				break;
		}
	}
}
