using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

/// <summary>
/// 全てのタイトルシーンに共通するクラス
/// </summary>
public class TitleBase : MonoBehaviour
{
	/// <summary>
	/// テキストのGameObjectのRenderer
	/// </summary>
	[SerializeField]
	Renderer[] TxtGoRenderers;

	/// <summary>
	/// 選択/非選択を表すマテリアル
	/// </summary>
	[SerializeField]
	Material[] Mats;

	/// <summary>
	/// 選択時に発生させるパーティクル
	/// </summary>
	[SerializeField]
	GameObject[] Particles;

	/// <summary>
	/// 現在選択しているメニューの番号
	/// </summary>
	readonly protected ReactiveProperty<int> currentSelect = new ReactiveProperty<int>();

	/// <summary>
	/// フェード用
	/// </summary>
	[SerializeField]
	Image FadePanel;

	/// <summary>
	/// Loadingの文字のGameObject
	/// </summary>
	[SerializeField]
	GameObject LoadingTxtGo;

	/// <summary>
	/// 現在選択しているメニューの番号をセット
	/// </summary>
	/// <param name="val">セットする番号</param>
	public void setCurrentSelect(int val)
	{
		currentSelect.Value = val;
		decide();
	}

	protected virtual void Start ()
	{
		currentSelect.Value = 0;
		for (var i = 0; i < Particles.Length; ++i) {
			Particles[i].SetActive(false);
		}
		FadePanel.color = Color.clear;
		LoadingTxtGo.SetActive(false);

		this.UpdateAsObservable().Where(x => !!isNext() && currentSelect.Value > 0)
			.Subscribe(_ => {
				--currentSelect.Value;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPrev() && currentSelect.Value < TxtGoRenderers.Length)
			.Subscribe(_ => {
				++currentSelect.Value;
			})
			.AddTo(this);

		currentSelect.AsObservable().Subscribe(val => {
			for (var i = 0; i < TxtGoRenderers.Length; ++i) {
				TxtGoRenderers[i].material = Mats[1];
			}
			TxtGoRenderers[val].material = Mats[0];
		})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isEnter())
			.Subscribe(_ => {
				decide();
			})
			.AddTo(this);
	}

	/// <summary>
	/// 次へボタンが押されたかどうか
	/// </summary>
	/// <returns>押された瞬間true</returns>
	protected virtual bool isNext()
	{
		// 派生クラスで実装
		return false;
	}

	/// <summary>
	/// 前へボタンが押されたかどうか
	/// </summary>
	/// <returns>押された瞬間true</returns>
	protected virtual bool isPrev()
	{
		// 派生クラスで実装
		return false;
	}

	/// <summary>
	/// Enterキーが押されたかどうか
	/// </summary>
	/// <returns>押された瞬間true</returns>
	bool isEnter()
	{
		return !!Input.GetKeyDown(KeyCode.Return);
	}

	/// <summary>
	/// 決定
	/// </summary>
	protected void decide()
	{
		TxtGoRenderers[currentSelect.Value].enabled = false;
		Particles[currentSelect.Value].SetActive(true);
		//StartCoroutine("loadScene");
		DOTween.ToAlpha(
			() => FadePanel.color,
			color => FadePanel.color = color,
			1.0f,
			1.0f
		).OnComplete(() => {
			if (currentSelect.Value == 0) {
				LoadingTxtGo.SetActive(true);
				SceneManager.LoadScene("Main");
			} else {
				Application.Quit();
			}
		});
	}

	/// <summary>
	/// 非同期でシーンを読み込む(画面が固まる)
	/// </summary>
	/// <returns></returns>
	IEnumerator loadScene()
	{
		var async = SceneManager.LoadSceneAsync("Main");
		async.allowSceneActivation = false;

		while (async.progress < 0.9f) {
			Debug.Log(async.progress);
			yield return new WaitForEndOfFrame();
		}

		Debug.Log("Scene Loaded");

		yield return new WaitForSeconds(1);

		async.allowSceneActivation = true;
	}
}

