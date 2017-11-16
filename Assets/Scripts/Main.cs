using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

/// <summary>
/// ゲーム全般に関するクラス
/// </summary>
public class Main : MonoBehaviour
{
	/// <summary>
	/// ゲームオーバー時に表示するGameObject
	/// </summary>
	[SerializeField]
	GameObject GameOverGo;
	/// <summary>
	/// クリア時に表示するGameObject
	/// </summary>
	[SerializeField]
	GameObject ClrGo;

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
	/// ゲームがスローになる速度(倍)
	/// </summary>
	const float Slow_Speed = 0.3f;

	#region フェード関連
	/// <summary>
	/// フェードのImage
	/// </summary>
	[SerializeField]
	Image FadeImg;
	/// <summary>
	/// フェードする時間(何秒でフェードするか)
	/// </summary>
	const float Fade_Time = 3.0f;
	#endregion

	/// <summary>
	/// ゲームオーバーの処理
	/// </summary>
	public void gameOver()
	{
		CurrentGameState = GameState.GameOver;
		GameOverGo.SetActive(true);
	}

	/// <summary>
	/// クリアの処理
	/// </summary>
	public void clr()
	{
		CurrentGameState = GameState.Clr;
		Time.timeScale = Slow_Speed;
		DOTween.ToAlpha(
			() => FadeImg.color,
			color => FadeImg.color = color,
			1.0f,
			Fade_Time
		).SetUpdate(true)
		.OnComplete(() => {
			Time.timeScale = 1.0f;
			SceneManager.LoadScene(Common.Clr_Scene);
		});
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		Common.setCursor();
		CurrentGameState = GameState.Play;
		if (GameOverGo != null) {
			GameOverGo.SetActive(false);
		}
		if (ClrGo != null) {
			ClrGo.SetActive(false);
		}

		FadeImg.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
	}

	void Start ()
	{
		init();

		this.UpdateAsObservable().Where(x => (CurrentGameState == GameState.GameOver || CurrentGameState == GameState.Clr) && !!Input.anyKeyDown)
			.Subscribe(_ => {
				SceneManager.LoadScene(Common.Title_Scene);
			})
			.AddTo(this);
	}
}
