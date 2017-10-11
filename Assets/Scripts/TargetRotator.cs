using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// あるオブジェクトの周りを回転するクラス
/// </summary>
public class TargetRotator : MonoBehaviour
{
	/// <summary>
	/// 一秒当たりの回転角度
	/// </summary>
	const float Angle = 90.0f;
	/// <summary>
	/// 回転の中心となるTransform
	/// </summary>
	Transform Parent;

	void Start ()
	{
		Parent = transform.parent.transform;

		transform.LookAt(Parent.transform);

		this.UpdateAsObservable().Subscribe(_ => {
			var axis = transform.TransformDirection(Vector3.up);
			transform.RotateAround(Parent.position, axis, Angle * Time.deltaTime);
		})
		.AddTo(this);
	}
}

