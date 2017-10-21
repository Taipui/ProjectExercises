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

	protected override void Start ()
	{
		base.Start();

		MyBulletLayer = "EnemyBullet";

		enableLaunch = false;

		setHp(Default_Hp);

		Observable.Interval(System.TimeSpan.FromSeconds(0.2f)).Where(x => !!isPlay() && !!enableLaunch)
			.Subscribe(_ => {
				if (Random.Range(0, 2) == 0) {
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
	}

	/// <summary>
	/// 発射の許可をする
	/// </summary>
	public void permitLaunch()
	{
		enableLaunch = true;
	}
}
