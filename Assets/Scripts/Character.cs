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

	protected virtual void Start ()
	{
		CharacterColliderTfm.position = transform.position;

		this.FixedUpdateAsObservable().Subscribe(_ => {
			transform.position = CharacterColliderTfm.position;
		})
		.AddTo(this);
	}
}

