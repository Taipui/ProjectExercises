using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

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

	/// <summary>
	/// カメラが揺れた時、地面が消えてしまうのを防ぐため
	/// </summary>
	[SerializeField]
	BoxCollider GroundChipChecker;

	void Start ()
	{
		var tfm = transform;
		var maxX = 0.4f;
		var prevX = tfm.localPosition.x;
		var cachedLocalPos = Vector3.zero;
		GroundChipChecker.enabled = true;

		PlayerTfm.UpdateAsObservable()
			.Subscribe(_ => {
				cachedLocalPos = new Vector3(Mathf.Clamp(PlayerTfm.localPosition.x + X_Offset, maxX, X_Limit), tfm.localPosition.y, tfm.localPosition.z);
				tfm.localPosition = cachedLocalPos;
				maxX = tfm.localPosition.x;
				diff.Value = tfm.localPosition.x - prevX;
			})
			.AddTo(this);

		diff.AsObservable().Where(val => val >= Erase_Threshold)
			.Subscribe(_ => {
				Gc.create();
				prevX = tfm.localPosition.x;
			})
			.AddTo(this);
	}

	/// <summary>
	/// カメラを揺らす
	/// </summary>
	public void shake()
	{
		GroundChipChecker.enabled = false;
		transform.DOShakeRotation(
			0.5f,
			1.0f
		).OnComplete(() => {
			GroundChipChecker.enabled = true;
		});
	}
}
