using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// ゲーム全般に関するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
	/// <summary>
	/// ゲームオーバー時に表示するGameObject
	/// </summary>
	[SerializeField]
	GameObject GameOverObj;
	/// <summary>
	/// クリア時に表示するGameObject
	/// </summary>
	[SerializeField]
	GameObject ClrObj;


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
		GameOver,
		/// <summary>
		/// クリア
		/// </summary>
		Clr
	}

	/// <summary>
	/// 現在のゲームの状態
	/// </summary>
	public GameState CurrentGameState { private set; get; }

	/// <summary>
	/// ゲームオーバーの処理
	/// </summary>
	public void gameOver()
	{
		CurrentGameState = GameState.GameOver;
		GameOverObj.SetActive(true);
	}

	/// <summary>
	/// クリアの処理
	/// </summary>
	public void clr()
	{
		CurrentGameState = GameState.Clr;
		ClrObj.SetActive(true);
	}

	void Start ()
	{
		CurrentGameState = GameState.Play;
		GameOverObj.SetActive(false);
		ClrObj.SetActive(false);

		this.UpdateAsObservable().Where(x => (CurrentGameState == GameState.GameOver && CurrentGameState == GameState.Clr) && !!Input.anyKeyDown)
			.Subscribe(_ => {
				SceneManager.LoadScene(Common.Title);
			})
			.AddTo(this);
	}
}
