using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

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

	/// <summary>
	/// 地面と当たったかどうか
	/// </summary>
	bool isCollide;

	/// <summary>
	/// 自身のコライダ
	/// </summary>
	SphereCollider col;

	void Start ()
	{
		col = GetComponent<SphereCollider>();

		this.UpdateAsObservable().Where(x => transform.position.y < Kill_Zone)
			.Subscribe(_ => {
				Destroy(gameObject);
			})
			.AddTo(this);

		col.OnCollisionEnterAsObservable().Subscribe(colObj => {
			var obj = Instantiate(BulletEffect, transform.position, Quaternion.identity);
			Destroy(obj, 0.5f * 2);
			if (colObj.gameObject.tag != "Ground") {
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
