using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// ボスに関するクラス
/// </summary>
public class Boss : Enemy
{
	protected override void Start ()
	{
		base.Start();

		this.UpdateAsObservable().Subscribe(_ => {
			move();
		})
		.AddTo(this);
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	protected override void launch()
	{
		foreach (Transform child in LauncherParent) {
			if (Random.Range(0, 2) == 0) {
				continue;
			}
			AI.ShootFixedAngle(child.position, PlayerTfm.position, 60.0f, child.gameObject.GetComponent<Launcher>(), Bullet, BulletParentTfm);
		}
	}

	/// <summary>
	/// キャラクターを動かす
	/// </summary>
	void move()
	{
		if (PlayerTfm == null) {
			return;
		}
		var bias = 1.0f;
		var moveSpeed = 0.05f;
		var diff = PlayerTfm.position.y - transform.position.y;
		if (diff < -bias) {
			transform.Translate(new Vector3(0.0f, -moveSpeed));
		} else if (diff > bias) {
			transform.Translate(new Vector3(0.0f, moveSpeed));
		}
	}
}
