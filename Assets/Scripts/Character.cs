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

