using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// どのクラスでも使うものを集めたクラス
/// </summary>
public static class Common
{
	#region シーン名
	/// <summary>
	/// タイトル
	/// </summary>
	public const string Title = "Title";
	/// <summary>
	/// メイン
	/// </summary>
	public const string Main = "Main";
	/// <summary>
	/// ゲームオーバー
	/// </summary>
	public const string GameOver = "GameOver";
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

	public static void setCursor()
	{
		Cursor.lockState = CursorLockMode.Confined;
		var tex = Resources.Load("Aim9") as Texture2D;
		var hotspot = tex.texelSize * 0.5f;
		hotspot.y *= -1;
		Cursor.SetCursor(tex, hotspot, CursorMode.ForceSoftware);
	}
}
