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
	/// ロード画面
	/// </summary>
	[SerializeField]
	GameObject LoadGo;

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
		Quit4,
		/// <summary>
		/// オプション画面の表示/非表示
		/// </summary>
		Option
	}

	/// <summary>
	/// 既に決定したかどうか
	/// </summary>
	bool isDecided;

	#region オプション関連

	/// <summary>
	/// オプションのボタン
	/// </summary>
	[SerializeField]
	Button OptionBtn;
	/// <summary>
	/// オプション画面のGameObject
	/// </summary>
	[SerializeField]
	protected GameObject OptionCanvasGo;

	public bool canInput { private set; get; }

	#endregion

	/// <summary>
	/// 現在選択しているメニューの番号をセット
	/// </summary>
	/// <param name="val">セットする番号</param>
	public void setCurrentSelect(int val)
	{
		currentSelect.Value = val;
		decide();
	}

	public void setCanInput(bool val)
	{
		canInput = val;
	}

	void init()
	{
		Common.setCursor();
		Cursor.visible = true;
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

		LoadGo.SetActive(false);

		OptionCanvasGo.SetActive(false);

		canInput = true;
	}

	protected virtual void Start ()
	{
		init();

		this.UpdateAsObservable().Where(x => !!isNext() && currentSelect.Value > 0 && !!canInput)
			.Subscribe(_ => {
				--currentSelect.Value;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPrev() && currentSelect.Value < TxtGoRenderers.Length - 1 && !!canInput)
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

		this.UpdateAsObservable().Where(x => !!isEnter() && !!canInput)
			.Subscribe(_ => {
				decide();
			})
			.AddTo(this);

		OptionBtn.OnClickAsObservable().Subscribe(_ => {
			onClickOptionBtn();
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
		if (currentSelect.Value == 0) {
			playSE((SE)System.Enum.ToObject(typeof(SE), Random.Range((int)SE.Start1, ((int)SE.Start2) + 1)));
		} else {
			playSE((SE)System.Enum.ToObject(typeof(SE), Random.Range((int)SE.Quit1, ((int)SE.Quit3) + 1)));
		}
		DOTween.ToAlpha(
			() => FadePanel.color,
			color => FadePanel.color = color,
			1.0f,
			1.0f
		).OnComplete(() => {
			if (currentSelect.Value == 0) {
				LoadGo.SetActive(true);
				Cursor.visible = false;
				SceneManager.LoadScene(Common.Main_Scene);
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

	/// <summary>
	/// オプションボタンを押されると呼ばれる
	/// </summary>
	protected virtual void onClickOptionBtn()
	{
		OptionCanvasGo.SetActive(true);
		canInput = false;
		audioSource.PlayOneShot(SEs[(int)SE.Option]);
	}

	/// <summary>
	/// オプションを閉じると呼ばれる
	/// </summary>
	public virtual void endOption()
	{
		canInput = true;
		audioSource.PlayOneShot(SEs[(int)SE.Option]);
	}
}
