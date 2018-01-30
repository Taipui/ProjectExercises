using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// スタッフロール用のプレイヤーの移動に関するクラス
/// </summary>
public class StaffRollPlayerMove : Character
{
	#region アニメーション関連

	/// <summary>
	/// アニメーションの再生速度
	/// </summary>
	const float Anim_Speed = 1.5f;

	/// <summary>
	/// 歩く速度
	/// </summary>
	const float Walk_Speed = 0.7f;
	/// <summary>
	/// 走る速度
	/// </summary>
	const float Run_Speed = 4.0f;

	#endregion

	/// <summary>
	/// キャラクターが動く速度
	/// </summary>
	float speed;

	/// <summary>
	/// canInputに値をセットする
	/// </summary>
	/// <param name="val">セットする値</param>
	public void setCanInput(bool val)
	{
		canInput = val;
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		canInput = false;

		speed = Walk_Speed;

		setWalkSpeed(1.45f * 10);
	}

	protected override void Start ()
	{
		base.Start();

		init();

		var tfm = transform;

		this.FixedUpdateAsObservable().Subscribe(_ => {
			tfm.localPosition += new Vector3(speed * Time.fixedDeltaTime, 0.0f);
		})
		.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.D) && !!canInput)
			.Subscribe(_ => {
				setRun();
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyUp(KeyCode.D) && !!canInput)
			.Subscribe(_ => {
				anim.SetBool("IsRun", false);
				speed = Walk_Speed;
			})
			.AddTo(this);
	}

	/// <summary>
	/// Dキーを押していたら走る
	/// </summary>
	public void runCheck()
	{
		if (!!Input.GetKey(KeyCode.D)) {
			setRun();
		}
	}

	/// <summary>
	/// 走っている状態にする
	/// </summary>
	void setRun()
	{
		anim.SetBool("IsRun", true);
		speed = Run_Speed;
	}

	/// <summary>
	/// 歩く速度(歩くアニメーションの再生速度とキャラクターの動く速度)をセット
	/// </summary>
	/// <param name="speed">歩くアニメーションの再生速度とキャラクターの動く速度にかける値</param>
	public void setWalkSpeed(float speed_)
	{
		anim.SetFloat("WalkSpeed", speed_);
		speed *= speed_;
	}

	protected override void OnCollisionEnter(Collision collision)
	{
		// 何もしない
	}
}
