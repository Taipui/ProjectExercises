using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// ザコ敵に関するクラス
/// </summary>
public class Enemy : Character
{
	/// <summary>
	/// プレイヤーのTransform
	/// </summary>
	[SerializeField]
	Transform PlayerTfm;

	/// <summary>
	/// 発射可能かどうか
	/// </summary>
	bool enableLaunch;

	/// <summary>
	/// 自身のコライダ
	/// </summary>
	BoxCollider col;

	protected override void Start ()
	{
		base.Start();

		MyBulletLayer = "EnemyBullet";

		enableLaunch = false;

		col = GetComponent<BoxCollider>();

		col.OnTriggerEnterAsObservable().Subscribe(colObj => {
			chechBullet(colObj.gameObject);
		})
		.AddTo(this);

		Observable.Interval(System.TimeSpan.FromSeconds(0.2f)).Where(x => !!isPlay() && !!enableLaunch)
			.Subscribe(_ => {
				if (Random.Range(0, 2) == 0) {
					onErased();
				}
			})
		.AddTo(this);
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	protected override void launch()
	{
		AI.ShootFixedAngle(transform.position, PlayerTfm.position, 60.0f, GetComponent<Launcher>(), Bullet, BulletParentTfm);
	}

	/// <summary>
	/// 地面のチップが消されたら呼ばれる
	/// </summary>
	public override void onErased()
	{
		launch();
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		Destroy(gameObject);
	}

	void OnCollisionEnter(Collision col)
	{
		chechBullet(col.gameObject);
	}

	/// <summary>
	/// 発射の許可をする
	/// </summary>
	public void permitLaunch()
	{
		enableLaunch = true;
	}
}
