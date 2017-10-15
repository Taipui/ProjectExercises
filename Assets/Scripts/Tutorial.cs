using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

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
	/// プレイヤーの頭上に表示されるメッセージのGameObject
	/// </summary>
	[SerializeField]
	GameObject PlayerMes;

	/// <summary>
	/// チュートリアル用のメッセージのポップアップのTransform
	/// </summary>
	[SerializeField]
	Transform TutorialMesPop;

	/// <summary>
	/// チュートリアル用のメッセージのポップアップを表示する前のimeScale
	/// </summary>
	float prevTimeScale;

	/// <summary>
	/// チュートリアル用のメッセージのポップアップを拡大/縮小させるTween
	/// </summary>
	Tweener popTween;

	/// <summary>
	/// チュートリアル用のメッセージのポップアップが開いているかどうか
	/// </summary>
	bool isOpen;

	void Start ()
	{
		col = GetComponent<BoxCollider>();
		PlayerMes.SetActive(false);
		TutorialMesPop.localScale = Vector3.zero;
		isOpen = false;

		col.OnTriggerEnterAsObservable().Where(colObj => colObj.gameObject.tag == "Player")
			.Subscribe(_ => {
				PlayerMes.SetActive(true);
			})
			.AddTo(this);

		col.OnTriggerExitAsObservable().Where(colObj => colObj.gameObject.tag == "Player")
			.Subscribe(_ => {
				PlayerMes.SetActive(false);
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!PlayerMes.activeSelf && !!Input.GetKeyDown(KeyCode.Return))
			.Subscribe(_ => {
				if (!isOpen) {
					openPop();
				} else {
					closePop();
				}
				isOpen = !isOpen;
			})
			.AddTo(this);
	}

	/// <summary>
	/// チュートリアル用のメッセージのポップアップの表示
	/// </summary>
	void openPop()
	{
		prevTimeScale = Time.timeScale;
		Time.timeScale = 0.0f;
		TutorialMesPop.DOScale(
			Vector3.one,
			0.5f
			).SetUpdate(true);
	}

	/// <summary>
	/// チュートリアル用のメッセージのポップアップの非表示
	/// </summary>
	void closePop()
	{
		TutorialMesPop.DOScale(
			Vector3.zero,
			0.5f
			).SetUpdate(true)
			.OnComplete(() => {
				Time.timeScale = prevTimeScale;
			});
	}
}
