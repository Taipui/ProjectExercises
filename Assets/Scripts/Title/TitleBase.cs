using System.Collections;
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
	/// ロード用のシーンへつなぐためのUIを表示するCanvasのPrefab
	/// </summary>
	[SerializeField]
	GameObject LoadCanvasPrefab;

	/// <summary>
	/// SEの配列
	/// </summary>
	[SerializeField]
	AudioClip[] SEs;
	AudioSource audioSource;
	public enum SE
	{
		/// <summary>
		/// 選択時のSE
		/// </summary>
		Select,
		/// <summary>
		/// 雪弾の発射時のSE
		/// </summary>
		Launch,
		/// <summary>
		/// 風を起こしている時のSE
		/// </summary>
		Wind,
		/// <summary>
		/// STARTが選択された時のSE
		/// </summary>
		Start1,
		Start2,
		/// <summary>
		/// QUITが選択された時のSE(
		/// </summary>
		Quit1,
		Quit2,
		Quit3,
		Quit4
	}

	/// <summary>
	/// 既に決定したかどうか
	/// </summary>
	bool isDecided;

	/// <summary>
	/// 現在選択しているメニューの番号をセット
	/// </summary>
	/// <param name="val">セットする番号</param>
	public void setCurrentSelect(int val)
	{
		currentSelect.Value = val;
		decide();
	}

	void init()
	{
		Common.setCursor();
		currentSelect.Value = 0;
		for (var i = 0; i < Particles.Length; ++i) {
			Particles[i].SetActive(false);
		}
		FadePanel.color = Color.clear;

		audioSource = GetComponent<AudioSource>();

		for (var i = 0; i < TxtGoRenderers.Length; ++i) {
			TxtGoRenderers[i].material = Mats[1];
		}
		TxtGoRenderers[currentSelect.Value].material = Mats[0];
	}

	protected virtual void Start ()
	{
		init();

		this.UpdateAsObservable().Where(x => !!isNext() && currentSelect.Value > 0)
			.Subscribe(_ => {
				--currentSelect.Value;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPrev() && currentSelect.Value < TxtGoRenderers.Length - 1)
			.Subscribe(_ => {
				++currentSelect.Value;
			})
			.AddTo(this);

		currentSelect.SkipLatestValueOnSubscribe()
			.AsObservable().Subscribe(val => {
			for (var i = 0; i < TxtGoRenderers.Length; ++i) {
				TxtGoRenderers[i].material = Mats[1];
			}
			TxtGoRenderers[val].material = Mats[0];
			playSE(SE.Select);
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
		if (!!isDecided) {
			return;
		}
		isDecided = true;
		TxtGoRenderers[currentSelect.Value].enabled = false;
		Particles[currentSelect.Value].SetActive(true);
		//StartCoroutine("loadScene");
		if (currentSelect.Value == 0) {
			playSE((SE)System.Enum.ToObject(typeof(SE), Random.Range((int)SE.Start1, ((int)SE.Start2) + 1)));
		} else {
			playSE((SE)System.Enum.ToObject(typeof(SE), Random.Range((int)SE.Quit1, ((int)SE.Quit3) + 1)));
			//playSE(SE.Quit4);
		}
		DOTween.ToAlpha(
			() => FadePanel.color,
			color => FadePanel.color = color,
			1.0f,
			1.0f
		).OnComplete(() => {
			if (currentSelect.Value == 0) {
				Instantiate(LoadCanvasPrefab);
				Cursor.visible = false;
				SceneManager.LoadScene(Common.Load_Scene);
			} else {
				Application.Quit();
			}
		});
	}

	/// <summary>
	/// SEを鳴らす
	/// </summary>
	/// <param name="se">鳴らすSE</param>
	public void playSE(SE se)
	{
		audioSource.PlayOneShot(SEs[(int)se]);
	}
}
