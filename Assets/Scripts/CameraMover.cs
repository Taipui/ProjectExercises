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
	const float Erase_Threshold = 0.1f;

	/// <summary>
	/// GroundCreater
	/// </summary>
	[SerializeField]
	GroundCreater Gc;

	/// <summary>
	/// X方向にカメラをオフセットする量
	/// </summary>
	const float X_Offset = 5.0f;

	/// <summary>
	/// カメラが動く制限
	/// </summary>
	const float X_Limit = 142.0f;

	void Start ()
	{
		var maxX = 0.4f;
		var prevX = transform.position.x;

		PlayerTfm.UpdateAsObservable()
			.Subscribe(_ => {
				transform.position = new Vector3(Mathf.Clamp(PlayerTfm.position.x + X_Offset, maxX, X_Limit), transform.position.y, transform.position.z);
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
