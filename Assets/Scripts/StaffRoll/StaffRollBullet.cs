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

	/// <summary>
	/// Rigidbody
	/// </summary>
	Rigidbody rb;

	/// <summary>
	/// 初期の雪弾の大きさ
	/// </summary>
	Vector2 defaultSize;

	/// <summary>
	/// 自身のコライダ
	/// </summary>
	SphereCollider col;

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		col = GetComponent<SphereCollider>();
		col.enabled = false;

		rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;

		defaultSize = transform.localScale;
	}

	/// <summary>
	/// 発射の処理をする
	/// </summary>
	public void launch()
	{
		col.enabled = false;
		rb.isKinematic = false;
		Invoke("enableCol", 0.1f);
	}

	/// <summary>
	/// コライダを有効にする
	/// </summary>
	void enableCol()
	{
		col.enabled = true;
	}

	void Start ()
	{
		init();


		this.UpdateAsObservable().Where(x => transform.position.x > Camera.main.transform.position.x + 10.0f || transform.position.y <= Kill_Zone)
			.Subscribe(_ => {
				changeNoUse();
			})
			.AddTo(this);

		col.OnCollisionEnterAsObservable().Subscribe(colGo => {
			destroy();
		})
		.AddTo(this);
	}

	/// <summary>
	/// 自身を消す(再使用可能の状態にする)
	/// </summary>
	void destroy()
	{
		var go = Instantiate(BulletEffect, transform.position, Quaternion.identity);
		Destroy(go, 0.5f * 2);
		changeNoUse();
	}

	/// <summary>
	/// 使われていない状態にする
	/// </summary>
	void changeNoUse()
	{
		rb.isKinematic = true;
		transform.position = Vector2.zero;
		transform.localScale = defaultSize;
	}

	/// <summary>
	/// 使用可能かどうか
	/// </summary>
	/// <returns>使用可能ならtrue</returns>
	public bool available()
	{
		return !!rb.isKinematic;
	}
}
