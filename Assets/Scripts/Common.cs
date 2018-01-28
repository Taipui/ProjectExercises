using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Linq;

/// <summary>
/// どのクラスでも使うものを集めたクラス
/// </summary>
public static class Common
{
	#region シーン名
	/// <summary>
	/// タイトル
	/// </summary>
	public const string Title_Scene = "Title";
	/// <summary>
	/// Mainシーンをロードするためのシーン
	/// </summary>
	public const string Load_Scene = "Load";
	/// <summary>
	/// メイン
	/// </summary>
	public const string Main_Scene = "Main";
	/// <summary>
	/// ゲームオーバー
	/// </summary>
	public const string GameOver_Scene = "GameOver";
	/// <summary>
	/// ゲームクリア
	/// </summary>
	public const string Clr_Scene = "Clear";
	/// <summary>
	/// スタッフロール
	/// </summary>
	public const string StaffRoll_Scene = "StaffRoll";
	#endregion

	#region レイヤー
	/// <summary>
	/// プレイヤーが発射した弾のレイヤー
	/// </summary>
	public const int PlayerBulletLayer = 13;
	/// <summary>
	/// 敵が発射した弾のレイヤー
	/// </summary>
	public const int EnemyBulletLayer = 14;
	/// <summary>
	/// どちらにも属さない弾のレイヤー
	/// </summary>
	public const int BulletLayer = 9;
	#endregion

	/// <summary>
	/// Mainシーンで使うBGMのタイトルのリスト
	/// </summary>
	public static readonly ReadOnlyCollection<string> Main_BGM_Title_List =
		Array.AsReadOnly(new string[] {
		"オオドラ\nEnemy Approaching アレンジ",
		"こふ\nFallen Down (Reprise) Butterscotch cinnamon mix",
		"mossari\nSnowy (mossari Remix)",
		"ああああ\nfluffy spooky blooky!",
		"daph\nBattle of Ordeal",
		"梅干茶漬け\nTrident of Sadness",
		"orangentle\nHopes and Dreams\n'A newer new home.'",
		"Tanchiky\nUnbelievable surrounded by Blue",
		"ryhki\nFascinate ft. Muffet",
		"s-don\nDogdance",
		"Kiryu\nThe Star of the Underground",
		"izna\nOnly",
		"shimaL\nUndersouls",
		"Frums\nUndiscardable",
		"ゆうゆ\nHowdy! and... Good-Die!",
		"モリモリあつし\nSpear of Justice(MRM REMIX)",
		"Puru\nASGORE (Puru Remix)",
		"Saiph\nDREAMEND",
		"Silentroom\nNEVERAGAIN",
		"Kankitsu\nRe-Undulate",
		"Unite In The Sky"
		});

	/// <summary>
	/// スタッフロールシーンで使うBGMのタイトルのリスト
	/// </summary>
	public static readonly ReadOnlyCollection<string> StaffRoll_BGM_Title_List =
		Array.AsReadOnly(new string[] {
		"Silentroom\nPRESS START! ...too clicke",
		"taqumi\nNu Dating",
		"Yamajet\nDeath by Glamour (Yamajet Cyber Disco Remix)",
		"seaside-metro\nOnly my determination",
		"izna\nRug Lags",
		"こふ\nWish upon the overground",
		"オオドラ\nEveryone's Home",
		"モリモリあつし+hara kana\nDummy! (FOOLISH HEART MRM REMIX)",
		"TCT a.k.a. anubasu-anubasu\nDogdonk",
		"l.olo.l\nihatov_x86",
		"猫smoke with ハマチルアウツ\nRuins(猫smoke with ハマチルアウツ)",
		"daph\nIt's cold outside",
		"shimaL\nASGORE (shimaL Remix)",
		"糸奇 はな\nHerTears",
		"ryhki\nYour Very Bery Best Friend",
		"Frums\nultra-blazures",
		"Se-U-Ra\nAster Mirror",
		"s-don\nUndying vs. Endlessness",
		"Kankitsu\nMEGALOVANIA (Kankitsu Remix)",
		"Kiryu\nIn Another Time",
		"MYTK\nWaterfall (MYTK Remix)",
		"コルソン\nTRUE HERO ~Memory of Justice~",
		"nitro\nkiss the sexy robot ultimate championship 201X!!!",
		"Tanchiky\nCOME ON! DISCO THE SOULS",
		"tc-taka\nHis Hopes and His Dreams",
		"梅干茶漬け\nFrom U to E",
		"ゆうゆ\nDREEMURRS",
		"ああああ\nour stories will never end!",
		"Saiph\nHOPEALIVE",
		"ぷりりー\nUndertale(improvisation)",
		});

	/// <summary>
	/// カーソルの画像をセットする
	/// </summary>
	public static void setCursor()
	{
		Cursor.lockState = CursorLockMode.Confined;
		var tex = Resources.Load("Aim") as Texture2D;
		var hotspot = new Vector2(tex.width / 2, tex.height / 2);
		Cursor.SetCursor(tex, hotspot, CursorMode.ForceSoftware);
	}

	/// <summary>
	/// 渡された重み付け配列からindexを得る
	/// 重み付きランダム - Qiita
	/// https://qiita.com/divideby_zero/items/a8e749e307013ab24a0b
	/// </summary>
	/// <param name="weightTable">重み付け配列</param>
	/// <returns>weightTableのindex</returns>
	public static int getRandomIndex(params int[] weightTable)
	{
		var totalWeight = weightTable.Sum();
		var val = UnityEngine.Random.Range(1, totalWeight + 1);
		var retIndex = -1;
		for (var i = 0; i < weightTable.Length; ++i) {
			if (weightTable[i] >= val) {
				retIndex = i;
				break;
			}
			val -= weightTable[i];
		}
		return retIndex;
	}
}
