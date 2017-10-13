using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// カメラを動かすクラス
/// </summary>
public class CameraMover : MonoBehaviour
{
	/// <summary>
	/// プレイヤーのTransform
	/// </summary>
	[SerializeField]
	Transform PlayerTfm;

	/// <summary>
	/// 地面のチップを消してからどれだけ動いたか
	/// </summary>
	readonly ReactiveProperty<float> diff = new ReactiveProperty<float>(0.0f);

	/// <summary>
	/// 地面のチップを消すまで移動する量
	/// </summary>
	const float Erase_Threshold = 20;

	[SerializeField]
	GroundCreater Gc;

	void Start ()
	{
		var maxX = PlayerTfm.position.x;
		var prevX = transform.position.x;

		PlayerTfm.UpdateAsObservable()
			.Subscribe(_ => {
				transform.position = new Vector3(Mathf.Max(maxX, PlayerTfm.position.x), transform.position.y, transform.position.z);
				maxX = transform.position.x;
				diff.Value = transform.position.x - prevX;
			})
			.AddTo(this);

		diff.AsObservable().Where(val => val >= Erase_Threshold)
			.Subscribe(_ => {
				Gc.create();
				prevX = transform.position.x;
			})
			.AddTo(this);
	}
}
