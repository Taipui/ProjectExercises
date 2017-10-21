using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 敵の弾の発射を許可するトリガーのクラス
/// </summary>
public class LaunchTrigger : MonoBehaviour
{
	/// <summary>
	/// 敵のスクリプト
	/// </summary>
	[SerializeField]
	Enemy[] Enemies;

	void Start ()
	{
		var col = GetComponent<BoxCollider>();

		col.OnTriggerEnterAsObservable().Where(colObj => colObj.gameObject.tag == "Player")
			.Subscribe(_ => {
				foreach (var enemy in Enemies) {
					enemy.permitLaunch();
				}
			})
			.AddTo(this);
	}
}
