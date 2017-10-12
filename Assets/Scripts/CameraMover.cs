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
		var maxX = PlayerTfm.position.x;

		PlayerTfm.UpdateAsObservable()
			.Subscribe(_ => {
				transform.position = new Vector3(Mathf.Max(maxX, PlayerTfm.position.x), transform.position.y, transform.position.z);
				maxX = transform.position.x;
			})
			.AddTo(this);
	}
}
