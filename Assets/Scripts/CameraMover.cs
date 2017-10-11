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

	void Start ()
	{
		PlayerTfm.UpdateAsObservable()
			.Subscribe(_ => {
				transform.position = new Vector3(PlayerTfm.position.x, transform.position.y, transform.position.z);
		})
		.AddTo(this);
	}
}

