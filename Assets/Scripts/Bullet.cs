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

	void Start ()
	{
		this.UpdateAsObservable().Where(x => transform.position.y < Kill_Zone)
			.Subscribe(_ => {
				Destroy(gameObject);
			})
			.AddTo(this);
	}

	void OnCollisionEnter(Collision collision)
	{
		var obj = Instantiate(BulletEffect, transform.position, Quaternion.identity);
		Destroy(obj, 0.5f * 2);
		if (collision.gameObject.tag != "Ground") {
			return;
		}
		if (!!isCollide) {
			return;
		}
		isCollide = true;
		Destroy(gameObject);
		Instantiate(GroundBullet, transform.position, Quaternion.identity);
	}
}

