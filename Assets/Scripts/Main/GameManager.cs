﻿using UnityEngine;

/// <summary>
/// ゲーム内で使用するリソースを管理するクラス
/// </summary>
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
				//ItemSprites = new Sprite[3];
				//for (var i = 0; i < 3; ++i) {
				//	ItemSprites[i] = Resources.Load<Sprite>("Sprites/Items/Item" + (i + 1).ToString());
				//}
				ItemSprites = Resources.LoadAll<Sprite>("Sprites/Items");
			}
			return ItemSprites;
		}
		set
		{
			ItemSprites = value;
		}
	}

	/// <summary>
	/// アイテムのマテリアルの配列(Sprite)
	/// </summary>
	Material[] ItemMatsSprite;
	public Material[] ItemMatsSprite_ {
		get
		{
			if (ItemMatsSprite == null) {
				ItemMatsSprite = Resources.LoadAll<Material>("Materials/Items/Masked");
			}
			return ItemMatsSprite;
		}
		set
		{
			ItemMatsSprite = value;
		}
	}
	/// <summary>
	/// アイテムのマテリアルの配列(UI)
	/// </summary>
	Material[] ItemMatsUI;
	public Material[] ItemMatsUI_ {
		get
		{
			if (ItemMatsUI == null) {
				ItemMatsUI = Resources.LoadAll<Material>("Materials/Items/MaskedUI");
			}
			return ItemMatsUI;
		}
		set
		{
			ItemMatsUI = value;
		}
	}

	/// <summary>
	/// Mainシーンで使うBGMの配列
	/// </summary>
	AudioClip[] mainBGMs;
	public AudioClip[] MainBGMs {
		get
		{
			return mainBGMs;
		}
		set
		{
			mainBGMs = value;
		}
	}
	/// <summary>
	/// スタッフロールシーンで使うBGMの配列
	/// </summary>
	AudioClip[] staffRollBGMs;
	public AudioClip[] StaffRollBGMs {
		get
		{
			return staffRollBGMs;
		}
		set
		{
			staffRollBGMs = value;
		}
	}

	/// <summary>
	/// 現在ロードしているBGMのインデックス
	/// </summary>
	int currentLoadBGMIndex;
	public int CurrentLoadBGMIndex {
		get
		{
			return currentLoadBGMIndex;
		}
		set
		{
			currentLoadBGMIndex = value;
		}
	}

	/// <summary>
	/// 前回までロードしていたBGMのインデックス
	/// </summary>
	int prevLoadBGMIndex;
	public int PrevLoadBGMIndex {
		get
		{
			return prevLoadBGMIndex;
		}
		set
		{
			prevLoadBGMIndex = value;
		}
	}

	/// <summary>
	/// 一度クリアしたかどうか
	/// </summary>
	bool isClr;
	public bool IsClr {
		get
		{
			return isClr;
		}
		set
		{
			isClr = value;
		}
	}

	GameManager()
	{
		currentLoadBGMIndex = 0;
		prevLoadBGMIndex = 0;
		isClr = false;
	}
}
