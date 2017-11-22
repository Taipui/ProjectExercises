using System.Collections;
using System.Collections.Generic;
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

	void Start ()
	{
		this.UpdateAsObservable().Where(x => !!isInput())
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
