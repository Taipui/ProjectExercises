using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// スタッフロール用のキャラクターが発射した弾に関するクラス
/// </summary>
public class StaffRollBullet : MonoBehaviour
{
	/// <summary>
	/// オブジェクトを消すY座標
	/// </summary>
	const float Kill_Zone = -6.0f;

	/// <summary>
	/// 雪弾が地面に当たった時のエフェクト
	/// </summary>
	[SerializeField]
	GameObject BulletEffect;

	void Start ()
	{
		var col = GetComponent<SphereCollider>();
		col.enabled = false;

		this.UpdateAsObservable().Where(x => transform.position.x > Camera.main.transform.position.x + 10.0f || transform.position.y <= Kill_Zone)
			.Subscribe(_ => {
				Destroy(gameObject);
			})
			.AddTo(this);

		col.OnCollisionEnterAsObservable().Subscribe(colGo => {
			Destroy(gameObject);
		})
		.AddTo(this);

		Observable.Timer(System.TimeSpan.FromSeconds(0.1f))
			.Subscribe(_ => {
				col.enabled = true;
			})
			.AddTo(this);
	}

	void OnDestroy()
	{
		var go = Instantiate(BulletEffect, transform.position, Quaternion.identity);
		Destroy(go, 0.5f * 2);
	}
}
