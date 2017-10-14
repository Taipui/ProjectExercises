using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// Grayちゃんの弾の発射を許可するトリガーのクラス
/// </summary>
public class GrayTrigger : MonoBehaviour
{
	/// <summary>
	/// Grayちゃんのスクリプト
	/// </summary>
	[SerializeField]
	Boss GrayChan;

	/// <summary>
	/// 自身のコライダ
	/// </summary>
	BoxCollider col;

	void Start ()
	{
		col = GetComponent<BoxCollider>();

		col.OnTriggerEnterAsObservable().Where(colObj => colObj.gameObject.tag == "Player")
			.Subscribe(_ => {
				GrayChan.permitLaunch();
			})
			.AddTo(this);
	}
}
