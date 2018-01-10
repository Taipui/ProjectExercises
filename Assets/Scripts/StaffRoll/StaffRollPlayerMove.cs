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
	
	#endregion

	protected override void Start ()
	{
		base.Start();

		var tfm = transform;

		this.FixedUpdateAsObservable().Subscribe(_ => {
			tfm.localPosition += new Vector3(Walk_Speed * Time.fixedDeltaTime, 0.0f);
		})
		.AddTo(this);
	}
}
