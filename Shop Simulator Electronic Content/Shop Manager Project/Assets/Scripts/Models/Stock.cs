//////////////////////////////////////////////////////
//Copyright James Jamieson 2017
//University Dissertation Project
//Shop Manager AI Simulation
//////////////////////////////////////////////////////


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Temperature { Frozen, Chilled, Room }

/// <summary>
/// All stock in the game will be instancing of this class.
/// </summary>
public class Stock {

	/// Used for internal information.
	public string IDName { get; protected set; }

	/// Used when outputting stock name to UI.
	public string Name { get; protected set; }

	/// Weight of this stock. Units: Grams/Millilitres
	public int Weight { get; protected set; }

	/// Value of this stock. Units: pence
	public int Price { get; protected set; }
											 //TODO The price of the stock should be set by the player, and so should be able to
											 //be changed once the game has begun. This will not be currently required for this project
											 //and so will not be implemented. The price used will be hardcoded.
											 //TODO In the future, maybe characters can only view the price of a piece of stock
											 //if they go to any front shelf where they are stocked, or by scanning it at the till.
											 //Managers may have the knowledge of every item and thier price automatically.

	/// Environment this stock should be kept at.
	public Temperature Temperature { get; protected set; }

	/// Flag to determine if this stock has been scanned through the checkout.
	public bool m_scanned;	

	/// Flag to deteermine if an employee has tried working this stock.
	public bool m_triedGoingOut;

	/// Flag to determine if this stock is faced up.
	public bool m_facedUp;

	/// Used when outputting price to UI. Conversion is made so UI shows price in pounds and pence.
	public static string StringPrice(int _price)
	{
		string priceToString = "";
		if ( _price < 100 )
		{
			if ( _price < 10 )
			{
				priceToString = "      " + _price.ToString () + "p";
			}
			else
			{
				priceToString = "    " + _price.ToString () + "p";
			}
		}
		else if ( _price % 100 == 0 )
		{
			priceToString = "£" + ( (float)_price / 100 ).ToString () + ".00";
		}
		else if ( _price % 10 == 0 )
		{
			priceToString = "£" + ( (float)_price / 100 ).ToString () + "0";
		}
		else
		{
			priceToString = "£" + ( (float)_price / 100 ).ToString ();
		}
		return priceToString;

	}											  

	//This constructor is only used when the prototypes are created.
	/// Spawns a new stock item with specified information.
	public Stock(string _IDName, string _name, int _weight, int _price, Temperature _temperature)
	{
		this.IDName = _IDName;
		this.Name = _name;
		this.Weight = _weight;
		this.Price = _price;
		this.Temperature = _temperature;
	}

	/// Returns a copy of this stock
	virtual public Stock Clone()
	{
		return new Stock(this);
	}

	//Used in conjuction with the virtual Clone function.
	protected Stock(Stock _other)
	{
		this.IDName = _other.IDName;
		this.Name = _other.Name;
		this.Weight = _other.Weight;
		this.Price = _other.Price;
		this.Temperature = _other.Temperature;
		this.m_facedUp = _other.m_facedUp;
	}

}
