using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
using TMPro;

/// <summary>
/// チュートリアルに関するクラス
/// </summary>
public class Tutorial : MonoBehaviour
{
	/// <summary>
	/// プレイヤーの頭上に表示されるメッセージ
	/// </summary>
	[SerializeField]
	TextMeshPro PlayerMes;

	/// <summary>
	/// チュートリアル用のメッセージのポップアップのTransform
	/// </summary>
	[SerializeField]
	Transform TutorialMesPop;
	[SerializeField]
	GameObject[] Messages;

	/// <summary>
	/// チュートリアル用のメッセージのポップアップを拡大/縮小させるTween
	/// </summary>
	Tweener popTween;

	/// <summary>
	/// 現在のメッセージ
	/// </summary>
	int currentMes;

	/// <summary>
	/// Player
	/// </summary>
	[SerializeField]
	Player Player;

	/// <summary>
	/// ページ送りをするかどうか
	/// </summary>
	[SerializeField]
	bool isFeedPage;

	/// <summary>
	/// メッセージを開いているかどうか(ページ送りをしない看板の設定)
	/// </summary>
	bool isOpen;

	/// <summary>
	/// 背景を暗くするためのImage
	/// </summary>
	[SerializeField]
	Image DarkPanelImg;
	/// <summary>
	/// 背景を暗くするTween
	/// </summary>
	Tweener darkTween;

	/// <summary>
	/// Main
	/// </summary>
	[SerializeField]
	Main Main;

	void Start ()
	{
		init();
		var col = GetComponent<BoxCollider>();

		col.OnTriggerEnterAsObservable().Where(colGo => !!isPlayer(colGo))
			.Subscribe(_ => {
				PlayerMes.text = "Press\n<b>F</b> Key";
				Player.setEnableChange(true);
				Main.nextStage(System.Convert.ToInt32(name.Substring(5, 1)));
			})
			.AddTo(this);

		col.OnTriggerExitAsObservable().Where(colGo => !!isPlayer(colGo))
			.Subscribe(_ => {
				PlayerMes.text = "";
				Player.setEnableChange(false);
			})
			.AddTo(this);
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		PlayerMes.text = "";
		TutorialMesPop.localScale = Vector3.zero;
		currentMes = 0;
		isOpen = false;
		foreach (var go in Messages) {
			go.SetActive(false);
		}

		DarkPanelImg.color = Color.clear;
	}

	/// <summary>
	/// メッセージ表示/非表示の処理
	/// </summary>
	public void execPop()
	{
		if (popTween != null) {
			popTween.Kill();
		}
		if (darkTween != null) {
			darkTween.Kill();
		}

		if (!!isFeedPage) {
			if (currentMes == 0) {
				openPop();
			} else if (currentMes == Messages.Length) {
				closePop();
				currentMes = 0;
			} else {
				Main.playSE(Main.SE.Feed, null);
				selectMes();
			}
		} else {
			if (!isOpen) {
				openPop();
				currentMes %= Messages.Length;
				isOpen = true;
			} else {
				closePop();
				isOpen = false;
			}
		}
	}

	/// <summary>
	/// チュートリアル用のメッセージのポップアップの表示
	/// </summary>
	void openPop()
	{
		selectMes();
		Time.timeScale = 0.0f;
		popTween = TutorialMesPop.DOScale(
			Vector3.one,
			0.5f
			).SetUpdate(true);

		darkTween = DOTween.ToAlpha(
			() => DarkPanelImg.color,
			color => DarkPanelImg.color = color,
			0.5f,
			0.5f
			).SetUpdate(true);
	}

	/// <summary>
	/// 次のメッセージへ切り替える
	/// </summary>
	void nextPop()
	{
		selectMes();
	}

	/// <summary>
	/// チュートリアル用のメッセージのポップアップの非表示
	/// </summary>
	void closePop()
	{
		popTween = TutorialMesPop.DOScale(
			Vector3.zero,
			0.5f
			).SetUpdate(true)
			.OnComplete(() => {
				Time.timeScale = 1.0f;
				clrMes();
			});

		darkTween = DOTween.ToAlpha(
			() => DarkPanelImg.color,
			color => DarkPanelImg.color = color,
			0.0f,
			0.5f
			).SetUpdate(true);
	}

	/// <summary>
	/// チュートリアル用のメッセージの内容を変える
	/// </summary>
	void selectMes()
	{
		clrMes();
		Messages[currentMes].SetActive(true);
		++currentMes;
	}

	/// <summary>
	/// メッセージを消去する
	/// </summary>
	void clrMes()
	{
		foreach (GameObject go in Messages) {
			go.SetActive(false);
		}
	}

	void OnDestroy()
	{
		PlayerMes.text = "";
	}

	/// <summary>
	/// 当たったものがプレイヤーかどうか
	/// </summary>
	/// <param name="col">当たったもの</param>
	/// <returns>プレイヤーならtrue</returns>
	bool isPlayer(Collider col)
	{
		return col.tag == "Player";
	}
}
