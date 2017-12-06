﻿using UnityEngine;

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
	/// カーソルの画像をセットする
	/// </summary>
	public static void setCursor()
	{
		Cursor.lockState = CursorLockMode.Confined;
		var tex = Resources.Load("Aim") as Texture2D;
		var hotspot = new Vector2(tex.width / 2, tex.height / 2);
		Cursor.SetCursor(tex, hotspot, CursorMode.ForceSoftware);
	}
}
