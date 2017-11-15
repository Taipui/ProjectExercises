using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
	static GameManager instance = new GameManager();
	public static GameManager Instance {
		get
		{
			if (instance == null) {
				instance = new GameManager();
			}
			return instance;
		}
	}

	/// <summary>
	/// アイテムの画像の配列
	/// </summary>
	Sprite[] ItemSprites;
	public Sprite[] ItemSprites_ {
		get
		{
			if (ItemSprites == null) {
				ItemSprites = Resources.LoadAll<Sprite>("Sprites/Items");
			}
			return ItemSprites;
		}
		set
		{
			ItemSprites = value;
		}
	}

	void Start ()
	{
		
	}
}

