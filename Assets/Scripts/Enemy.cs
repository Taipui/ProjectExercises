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

	/// <summary>
	/// CameraMover(カメラを揺らすため)
	/// </summary>
	CameraMover camMover;

	void init()
	{
		MyBulletLayer = Common.EnemyBulletLayer;

		enableLaunch = false;

		hp = Default_Hp;

		camMover = Camera.main.GetComponent<CameraMover>();
	}

	protected override void Start ()
	{
		base.Start();

		init();

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
	/// ダメージ処理
	/// </summary>
	protected override IEnumerator dmg()
	{
		camMover.shake(0.5f);
		return base.dmg();
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		base.dead();
	}

	/// <summary>
	/// 発射の許可をする
	/// </summary>
	public void permitLaunch()
	{
		enableLaunch = true;
	}
}
