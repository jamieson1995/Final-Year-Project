using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Temperature { Frozen, Chilled, Room }

/// <summary>
/// All stock in the game will be instancing of this class.
/// </summary>
public class Stock {

	public string IDName { get; protected set; }

	public string Name { get; protected set; }

	public int Weight { get; protected set; } // Grams/Millilitres

	public int Price { get; protected set; } //pence
											 //TODO The price of the stock should be set by the player, and so should be able to
											 //be changed once the game has begun. This will not be currently required for this project
											 //and so will not be implemented. The price used will be hardcoded.
											 //TODO In the future, maybe characters can only view the price of a piece of stock
											 //if they go to any front shelf where they are stocked, or by scanning it at the till.
											 //Managers may have the knowledge of every item and thier price automatically.

	public Temperature Temperature { get; protected set; }

	public bool m_scanned;	

	//This takes the price of this stock, and tranfers the value into a nice to read string. If the value is less than 100, the price is shown
	//in pence instead of pounds, but is still formatted to fit the same space.
	public string StringPrice
	{
		get
		{
			string priceToString = "";
			if ( Price < 100 )
			{
				if ( Price < 10 )
				{
					priceToString = "      " + Price.ToString () + "p";
				}
				else
				{
					priceToString = "    " + Price.ToString () + "p";
				}
			}
			else if ( Price % 100 == 0 )
			{
				priceToString = "£" + ( (float)Price / 100 ).ToString () + ".00";
			}
			else if ( Price % 10 == 0 )
			{
				priceToString = "£" + ( (float)Price / 100 ).ToString () + "0";
			}
			else
			{
				priceToString = "£" + ( (float)Price / 100 ).ToString ();
			}
			return priceToString;
		}

	}											  

	//This constructor is only used when the prototypes are created.
	public Stock(string _IDName, string _name, int _weight, int _price, Temperature _temperature)
	{
		this.IDName = _IDName;
		this.Name = _name;
		this.Weight = _weight;
		this.Price = _price;
		this.Temperature = _temperature;
	}

	//When Stock is added to the world via a piece of furniture or a character, the actual stock isn't created, a copy of the 
	//stock prototype is created. Therefore, a temp stock needs to be cloned from the original and that is what gets placed.
	virtual public Stock Clone()
	{
		return new Stock(this);
	}

	//Used in conjunction with the Clone function
	protected Stock(Stock _other)
	{
		this.IDName = _other.IDName;
		this.Name = _other.Name;
		this.Weight = _other.Weight;
		this.Price = _other.Price;
		this.Temperature = _other.Temperature;
	}
}
