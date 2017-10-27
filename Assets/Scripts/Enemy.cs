using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 敵全般に関するクラス
/// </summary>
public class Enemy : Character
{
	/// <summary>
	/// プレイヤーのTransform
	/// </summary>
	[SerializeField]
	protected Transform PlayerTfm;

	/// <summary>
	/// 発射可能かどうか
	/// </summary>
	bool enableLaunch;

	/// <summary>
	/// デフォルトの体力
	/// </summary>
	const int Default_Hp = 1;

	/// <summary>
	/// 撃つ頻度(増やすほど撃たなくなる)
	/// </summary>
	[SerializeField]
	int Frequency;

	#region アイテム関連
	/// <summary>
	/// ドロップするアイテムのGameObject
	/// </summary>
	[SerializeField]
	GameObject[] Items;
	/// <summary>
	/// アイテムをドロップする力
	/// </summary>
	const float Item_Launch_Power = 2.0f;
	/// <summary>
	/// アイテムをドロップする角度の範囲
	/// </summary>
	const float Item_Launch_Angle_Range = 45.0f;
	#endregion

	protected override void Start ()
	{
		base.Start();

		MyBulletLayer = Common.EnemyBulletLayer;

		enableLaunch = false;

		setHp(Default_Hp);

		Observable.Interval(System.TimeSpan.FromSeconds(0.2f)).Where(x => !!isPlay() && !!enableLaunch)
			.Subscribe(_ => {
				if (Random.Range(0, Frequency) == 0) {
					launch();
				}
			})
		.AddTo(this);
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	protected virtual void launch()
	{
		// 派生クラスで実装
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		Destroy(gameObject);
		var r = Random.Range(0, Items.Length + 1);
		r = Items.Length;
		if (r == Items.Length) {
			return;
		}
		var go = Instantiate(Items[r], transform.position, Quaternion.identity);
		var vec = Vector3.up * Item_Launch_Power;
		vec = Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.Range(-Item_Launch_Angle_Range, Item_Launch_Angle_Range))) * vec;
		go.GetComponent<Rigidbody>().AddForce(vec, ForceMode.Impulse);
		go.tag = "Item" + (r + 1).ToString();
	}

	/// <summary>
	/// 発射の許可をする
	/// </summary>
	public void permitLaunch()
	{
		enableLaunch = true;
	}
}
