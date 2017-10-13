﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 敵に関するクラス
/// </summary>
public class Enemy : Character
{
	/// <summary>
	/// プレイヤーのTransform
	/// </summary>
	[SerializeField]
	Transform PlayerTfm;

	protected override void Start ()
	{
		base.Start();

		MyBulletLayer = "EnemyBullet";

		Observable.Interval(System.TimeSpan.FromSeconds(0.2f)).Where(x => !!isPlay())
			.Subscribe(_ => {
			if (Random.Range(0, 2) == 0) {
//					onErased();
			}
		})
		.AddTo(this);

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
	/// キャラクターを動かす
	/// </summary>
	void move()
	{
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
