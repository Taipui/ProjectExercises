﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// ボスに関するクラス
/// </summary>
public class Boss : Enemy
{
	/// <summary>
	/// GrayちゃんのモデルのGameObject
	/// </summary>
	[SerializeField]
	GameObject ModelGo;

	protected override void Start ()
	{
		base.Start();
		setHp(3);

		if (PlayerTfm == null) {
			return;
		}
		PlayerTfm.UpdateAsObservable().Where(x => !!canInput)
			.Subscribe(_ => {
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
			AI.ShootFixedAngle(new Vector3(child.position.x, child.position.y), PlayerTfm.position, 60.0f, child.gameObject.GetComponent<Launcher>(), Bullet, BulletParentTfm);
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

	/// <summary>
	/// キャラクターの点滅を始める
	/// </summary>
	protected override void startFlick()
	{
		StartCoroutine("flick");
	}

	/// <summary>
	/// キャラクターを点滅させる
	/// </summary>
	/// <returns></returns>
	protected override IEnumerator flick()
	{
		while (true) {
			ModelGo.SetActive(!ModelGo.activeSelf);
			yield return new WaitForSeconds(Flick_Interval);
		}
	}

	/// <summary>
	/// 点滅を止める
	/// </summary>
	protected override void stopFlick()
	{
		StopCoroutine("flick");
		ModelGo.SetActive(true);
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		base.dead();
		Main.clr();
	}
}
