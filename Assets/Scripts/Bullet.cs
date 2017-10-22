using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// キャラクターが発射した弾に関するクラス
/// </summary>
public class Bullet : MonoBehaviour
{
	/// <summary>
	/// 地面に落ちた用のBullet
	/// </summary>
	[SerializeField]
	GameObject GroundBullet;

	/// <summary>
	/// オブジェクトを消すY座標
	/// </summary>
	const float Kill_Zone = -6.0f;

	/// <summary>
	/// 雪玉が地面に当たった時のエフェクト
	/// </summary>
	[SerializeField]
	GameObject BulletEffect;

	void Start ()
	{
		var col = GetComponent<SphereCollider>();
		var isCollide = false;

		this.UpdateAsObservable().Where(x => transform.position.y < Kill_Zone)
			.Subscribe(_ => {
				Destroy(gameObject);
			})
			.AddTo(this);

		col.OnCollisionEnterAsObservable().Subscribe(colGo => {
			var go = Instantiate(BulletEffect, transform.position, Quaternion.identity);
			Destroy(go, 0.5f * 2);
			if (colGo.gameObject.tag != "Ground") {
				return;
			}
			if (!!isCollide) {
				return;
			}
			isCollide = true;
			Destroy(gameObject);
			Instantiate(GroundBullet, transform.position, Quaternion.identity);
		})
		.AddTo(this);
	}
}
