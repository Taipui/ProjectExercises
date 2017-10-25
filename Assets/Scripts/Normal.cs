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
}
