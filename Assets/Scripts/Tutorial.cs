using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
	/// 自身のコライダ
	/// </summary>
	BoxCollider col;

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

	void Start ()
	{
		col = GetComponent<BoxCollider>();
		PlayerMes.text = "";
		TutorialMesPop.localScale = Vector3.zero;
		currentMes = 0;
		isOpen = false;

		col.OnTriggerEnterAsObservable().Where(colObj => colObj.gameObject.tag == "Player")
			.Subscribe(_ => {
				PlayerMes.text = "Push Enter";
				Player.setEnableChange(true);
			})
			.AddTo(this);

		col.OnTriggerExitAsObservable().Where(colObj => colObj.gameObject.tag == "Player")
			.Subscribe(_ => {
				PlayerMes.text = "";
				Player.setEnableChange(false);
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => PlayerMes != null && PlayerMes.text != "" && !!Input.GetKeyDown(KeyCode.Return))
			.Subscribe(_ => {
			})
			.AddTo(this);
	}

	/// <summary>
	/// メッセージ表示/非表示の処理
	/// </summary>
	public void execPop()
	{
		if (popTween != null) {
			popTween.Kill();
		}

		if (!!isFeedPage) {
			if (currentMes == 0) {
				openPop();
			} else if (currentMes == Messages.Length) {
				closePop();
				currentMes = 0;
			} else {
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
			});
		clrMes();
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
		foreach (GameObject obj in Messages) {
			obj.SetActive(false);
		}
	}

	void OnDestroy()
	{
		PlayerMes.text = "";
	}
}
