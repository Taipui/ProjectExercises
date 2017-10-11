using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲーム全般に関するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
	/// <summary>
	/// ゲームオーバーのCanvasのGameObject
	/// </summary>
	[SerializeField]
	GameObject GameOverCanvas;

	/// <summary>
	/// ゲームの状態
	/// </summary>
	public enum GameState
	{
		/// <summary>
		/// ゲーム中
		/// </summary>
		Play,
		/// <summary>
		/// ゲームオーバー
		/// </summary>
		GameOver
	}

	public GameState CurrentGameState { private set; get; }

	/// <summary>
	/// ゲームオーバーの処理
	/// </summary>
	public void gameOver()
	{
		CurrentGameState = GameState.GameOver;
		GameOverCanvas.SetActive(true);
	}

	void Start ()
	{
		CurrentGameState = GameState.Play;
	}
}

