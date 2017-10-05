//
//Player.cs
//プレイヤーに関するスクリプト
//2017/10/4 Taipui
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

public class Player : Character
{
	/// <summary>
	/// 発射する弾
	/// </summary>
	[SerializeField]
	GameObject Bullet;

	/// <summary>
	/// 弾の発射位置
	/// </summary>
	[SerializeField]
	Transform LaunchTfm;

	/// <summary>
	/// PlayerColliderへの参照
	/// </summary>
	[SerializeField]
	PlayerCollider Pc;

	/// <summary>
	/// 横方向の入力
	/// </summary>
	readonly ReactiveProperty<float> dx = new ReactiveProperty<float>(0.0f);

	/// <summary>
	/// プレイヤーの移動速度
	/// </summary>
	const float Move_Speed = 2.0f;

	/// <summary>
	/// プレイヤーが回転する時のTween
	/// </summary>
	Tweener rotateTween;

	/// <summary>
	/// プレイヤーの向き
	/// </summary>
	string dir;
	/// <summary>
	/// プレイヤーの前回までの向き
	/// </summary>
	string dirOld;

	/// <summary>
	/// プレイヤーが回転中かどうか
	/// </summary>
	bool isRotating;

	protected override void Start ()
	{
		base.Start();

		dir = "D";
		dirOld = dir;
		isRotating = false;

		var anim = GetComponent<Animator>();

		this.FixedUpdateAsObservable().Subscribe(_ => {
			dx.Value = Input.GetAxis("Horizontal");

			anim.SetFloat("Speed", Mathf.Abs(dx.Value));
			anim.speed = 1.5f;

			var velocity = new Vector3(dx.Value, 0);

			velocity *= Move_Speed;

			CharacterColliderTfm.localPosition += velocity * Time.fixedDeltaTime;
		})
		.AddTo(this);

		dx.AsObservable().Where(val => val != 0.0f)
			.Subscribe(val => {
				if (val < 0) {
					dir = "A";
					rotate();
				} else if (val > 0) {
					dir = "D";
					rotate();
				}
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetMouseButtonDown(0))
			.Subscribe(_ => {
				launch();
			})
			.AddTo(this);
	}

	/// <summary>
	/// プレイヤーの向きを変える
	/// </summary>
	void rotate()
	{
		if (dir == dirOld) {
			return;
		}
		if (!!isRotating) {
			return;
		}
		isRotating = true;
		var rotateVal = dir == "D" ? 90.0f : -90.0f;
		if (rotateTween != null) {
			rotateTween.Kill();
		}
		rotateTween = transform.DORotate(
			new Vector3(0.0f, rotateVal),
			0.1f
			).OnComplete(() => {
				dirOld = dir;
				isRotating = false;
		});
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	void launch()
	{
		var obj = Instantiate(Bullet, LaunchTfm.position, Quaternion.identity);
		var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		var direction = mousePos - LaunchTfm.position;
		obj.GetComponent<Rigidbody2D>().velocity = direction.normalized * 40.0f;
		Pc.eraseGroundChip();
	}
}
