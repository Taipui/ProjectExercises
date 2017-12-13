using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

/// <summary>
/// クリアシーンに関連するクラス
/// </summary>
public class Clear : MonoBehaviour
{
	/// <summary>
	/// フェード用の画像
	/// </summary>
	[SerializeField]
	Image FadePanel;

	/// <summary>
	/// 画面遷移が始まったかどうか
	/// </summary>
	bool isTransition;

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		isTransition = false;
		Cursor.visible = false;
	}

	void Start ()
	{
		init();

		this.UpdateAsObservable().Where(x => !isTransition && !!isInput())
			.Subscribe(_ => {
				transition();
			})
			.AddTo(this);
	}

	/// <summary>
	/// どれかのキーかどれかのマウスのボタンが押されたかどうか
	/// </summary>
	/// <returns>押された瞬間true</returns>
	bool isInput()
	{
		return Input.anyKeyDown;
	}

	/// <summary>
	/// 画面遷移
	/// </summary>
	void transition()
	{
		if (!!isTransition) {
			return;
		}
		isTransition = true;

		FadePanel.color = Color.clear;
		DOTween.ToAlpha(
			() => FadePanel.color,
			color => FadePanel.color = color,
			1.0f,
			3.0f
		).OnComplete(() => {
			SceneManager.LoadScene(Common.Title_Scene);
		});
	}
}
