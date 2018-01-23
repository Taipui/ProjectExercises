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
	/// StaffRoll
	/// </summary>
	StaffRoll staffRoll;

	#region アイテム関連

	/// <summary>
	/// アイテムをドロップする力
	/// </summary>
	const float Item_Launch_Power = 2.0f;
	/// <summary>
	/// アイテムをドロップする角度の範囲
	/// </summary>
	const float Item_Launch_Angle_Range = 45.0f;

	#endregion

	/// <summary>
	/// StaffRollをセット
	/// </summary>
	/// <param name="staffRoll_">セットするStaffRoll</param>
	public void setStaffRoll(StaffRoll staffRoll_)
	{
		staffRoll = staffRoll_;
	}

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

		this.UpdateAsObservable().Where(x => !rb.isKinematic && transform.position.x > Camera.main.transform.position.x + 10.0f || transform.position.y <= Kill_Zone)
			.Subscribe(_ => {
				changeNoUse();
			})
			.AddTo(this);

		col.OnCollisionEnterAsObservable().Subscribe(colGo => {
			if (colGo.gameObject.tag == "Text" || colGo.gameObject.tag == "EndText") {
				staffRoll.playSE(StaffRoll.SE.Kill, null);

				Destroy(colGo.gameObject);

				spawnParticle();

				spawnItem();
			}

			destroy();
		})
		.AddTo(this);
	}

	/// <summary>
	/// パーティクルを出現させる
	/// </summary>
	void spawnParticle()
	{
		var particleGo = Instantiate(Resources.Load("Prefabs/PopStar2"), transform.position, Quaternion.identity);
		Destroy(particleGo, 1.0f);
	}

	/// <summary>
	/// アイテムを出現させる
	/// </summary>
	void spawnItem()
	{
		var r = Random.Range(0, GameManager.Instance.ItemSprites_.Length + 1);
		if (r <= 0) {
			return;
		}
		var itemGo = Instantiate(Resources.Load("Prefabs/Item") as GameObject, transform.position, Quaternion.identity);
		var vec = Vector3.up * Item_Launch_Power;
		vec = Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.Range(-Item_Launch_Angle_Range, Item_Launch_Angle_Range))) * vec;
		itemGo.GetComponent<Rigidbody>().AddForce(vec, ForceMode.Impulse);
		itemGo.tag = "Item" + r.ToString();
		var sr = itemGo.GetComponent<SpriteRenderer>();
		var index = r - 1;
		sr.sprite = GameManager.Instance.ItemSprites_[index];
		sr.material = GameManager.Instance.ItemMatsSprite_[index];
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
	public void changeNoUse()
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
