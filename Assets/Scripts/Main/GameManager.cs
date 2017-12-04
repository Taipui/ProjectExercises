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
}
