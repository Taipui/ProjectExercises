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
	/// <summary>
	/// StaffRoll
	/// </summary>
	[SerializeField]
	StaffRoll StaffRoll;

	#region アニメーション関連
	/// <summary>
	/// Animatorの配列
	/// </summary>
	[SerializeField]
	Animator[] Anims;

	int currentChar;

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
	/// スタッフロール用のPlayerAct
	/// </summary>
	StaffRollPlayerAct playerAct;

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


		playerAct = GetComponent<StaffRollPlayerAct>();

		playerAct.randomChangeAvatar((currentChar_) => {
			currentChar = currentChar_;
			setWalkSpeed(1.1f * 0.9f);
		});
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
				StaffRoll.setPitch(StaffRoll.getPitch() * 1.5f);
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyUp(KeyCode.D) && !!canInput)
			.Subscribe(_ => {
				Anims[currentChar].SetBool("IsRun", false);
				speed = Walk_Speed;
				StaffRoll.setPitch(StaffRoll.getPitch() / 1.5f);
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
			StaffRoll.setPitch(StaffRoll.getPitch() * 1.5f);
		}
	}

	/// <summary>
	/// 走っている状態にする
	/// </summary>
	void setRun()
	{
		Anims[currentChar].SetBool("IsRun", true);
		speed = Run_Speed;
	}

	/// <summary>
	/// 歩く速度(歩くアニメーションの再生速度とキャラクターの動く速度)をセット
	/// </summary>
	/// <param name="speed">歩くアニメーションの再生速度とキャラクターの動く速度にかける値</param>
	public void setWalkSpeed(float speed_)
	{
		if (currentChar < 0) {
			currentChar = playerAct.CurrentChar;
		}
		Anims[currentChar].SetFloat("WalkSpeed", speed_);
		speed *= speed_;
	}

	protected override void OnCollisionEnter(Collision collision)
	{
		// 当たり判定の処理はPlayerActが行うので、ここでは何もしない
	}
}
