using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ザコ敵に関するクラス
/// </summary>
public class Normal : Enemy
{
	/// <summary>
	/// 常に発射するか
	/// </summary>
	[SerializeField]
	bool IsAlways;

	#region アイテム関連
	/// <summary>
	/// ドロップするアイテムのGameObject
	/// </summary>
	[SerializeField]
	GameObject ItemGo;
	/// <summary>
	/// アイテムをドロップする力
	/// </summary>
	const float Item_Launch_Power = 2.0f;
	/// <summary>
	/// アイテムをドロップする角度の範囲
	/// </summary>
	const float Item_Launch_Angle_Range = 45.0f;
	/// <summary>
	/// アイテムをドロップするかどうか
	/// </summary>
	[SerializeField]
	bool IsDrop;
	#endregion

	/// <summary>
	/// 死亡時に生成するパーティクルのGameObject
	/// </summary>
	[SerializeField]
	GameObject Particle;

	protected override void Start ()
	{
		base.Start();
		if (!!IsAlways) {
			permitLaunch();
		}
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	protected override void launch()
	{
		AI.ShootFixedAngle(transform.position, PlayerTfm.position, 60.0f, GetComponent<Launcher>(), Bullet, BulletParentTfm);
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		Destroy(gameObject);

		var go = Instantiate(Particle);
		go.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
		Destroy(go, go.GetComponent<ParticleSystem>().main.duration);

		if (!IsDrop) {
			return;
		}
		var r = Random.Range(0, 4);
		//r = 5;
		if (r <= 0) {
			return;
		}
		go = Instantiate(ItemGo, transform.position, Quaternion.identity);
		var vec = Vector3.up * Item_Launch_Power;
		vec = Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.Range(-Item_Launch_Angle_Range, Item_Launch_Angle_Range))) * vec;
		go.GetComponent<Rigidbody>().AddForce(vec, ForceMode.Impulse);
		go.tag = "Item" + r.ToString();
		go.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.ItemSprites_[r - 1];

		if (Particle == null) {
			return;
		}
	}
}
