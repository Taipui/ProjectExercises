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
	/// キャラクターのColliderのTransform
	/// </summary>
	[SerializeField]
	protected Transform CharacterColliderTfm;

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
	#endregion
	/// <summary>
	/// 消す地面のチップを判断するためのレイの長さ
	/// </summary>
	const float Erase_GroundChip_Ray_Dist = -0.05f;

	/// <summary>
	/// GroundChipErase
	/// </summary>
	[SerializeField]
	protected GroundChipEraser Gce;

	/// <summary>
	/// 体力
	/// </summary>
	readonly ReactiveProperty<int> hp = new ReactiveProperty<int>(1);

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
		CharacterColliderTfm.position = transform.position;

		this.FixedUpdateAsObservable().Subscribe(_ => {
			transform.position = CharacterColliderTfm.position;
		})
		.AddTo(this);

		this.UpdateAsObservable().Subscribe(_ => {
			Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - Erase_GroundChip_Ray_Dist), Vector3.down, Color.red);
		})
		.AddTo(this);

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
	public void damage()
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
}

