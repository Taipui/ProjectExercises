using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Character : MonoBehaviour
{
	/// <summary>
	/// キャラクターのColliderのTransform
	/// </summary>
	[SerializeField]
	protected Transform CharacterColliderTfm;

	/// <summary>
	/// ジャンプ中かどうか
	/// </summary>
	protected bool isJumping;

	/// <summary>
	/// 発射する弾
	/// </summary>
	[SerializeField]
	protected GameObject Bullet;

	/// <summary>
	/// 弾の発射位置
	/// </summary>
	[SerializeField]
	protected Transform LaunchTfm;

	protected virtual void Start ()
	{
		CharacterColliderTfm.position = transform.position;

		this.FixedUpdateAsObservable().Subscribe(_ => {
			transform.position = CharacterColliderTfm.position;
		})
		.AddTo(this);

		//this.FixedUpdateAsObservable().Where(x => !isJumping)
		//	.Subscribe(_ => {
		//		transform.position = CharacterColliderTfm.position;
		//	})
		//.AddTo(this);

		//this.FixedUpdateAsObservable().Where(x => !!isJumping)
		//	.Subscribe(_ => {
		//		CharacterColliderTfm.position = transform.position;
		//	})
		//.AddTo(this);
	}
}

