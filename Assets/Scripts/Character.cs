using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 登場人物すべてに共通するクラス
/// </summary>
public class Character : MonoBehaviour
{
	/// <summary>
	/// ジャンプ中かどうか
	/// </summary>
	protected bool isJumping;

	#region 弾関連
	/// <summary>
	/// 発射する弾
	/// </summary>
	[SerializeField]
	protected GameObject Bullet;
	/// <summary>
	/// 弾の発射位置
	/// </summary>
	[SerializeField]
	protected Transform LaunchTfm;
	/// <summary>
	/// 発射した弾をまとめるGameObjectのTransform
	/// </summary>
	[SerializeField]
	protected Transform BulletParentTfm;
	/// <summary>
	/// 自分が発射する弾に設定するタグの名前
	/// </summary>
	public string MyBulletTag { protected set; get; }
	#endregion
	/// <summary>
	/// 消す地面のチップを判断するためのレイの長さ
	/// </summary>
	const float Erase_GroundChip_Ray_Dist = -0.05f;

	/// <summary>
	/// 体力
	/// </summary>
	readonly ReactiveProperty<int> hp = new ReactiveProperty<int>(1);

	/// <summary>
	/// GameManager
	/// </summary>
	[SerializeField]
	protected GameManager Gm;

	/// <summary>
	/// 体力をセット
	/// </summary>
	/// <param name="val">セットする体力</param>
	protected void setHp(int val)
	{
		hp.Value = val;
	}

	protected virtual void Start ()
	{
		hp.AsObservable().Where(val => val <= 0)
			.Subscribe(_ => {
				dead();
			})
			.AddTo(this);
	}

	/// <summary>
	/// 地面のチップが消されたら呼ばれる
	/// </summary>
	public virtual void onErased()
	{
		launch();
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	protected virtual void launch()
	{
		// 派生クラスで実装
	}

	/// <summary>
	/// ダメージ処理
	/// </summary>
	void damage()
	{
		--hp.Value;
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected virtual void dead()
	{
		// 派生クラスで実装
	}

	/// <summary>
	/// 当たったものが相手の弾かどうかを調べる
	/// </summary>
	/// <param name="obj">当たったもの</param>
	protected void chechBullet(GameObject obj)
	{
		if (LayerMask.LayerToName(obj.layer) != "Bullet") {
			return;
		}
		if (obj.tag == MyBulletTag) {
			return;
		}

		damage();
	}
}
