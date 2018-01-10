using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// スタッフロール用のカメラを動かすクラス
/// </summary>
public class StaffRollCameraMover : MonoBehaviour
{
	[SerializeField]
	Transform PlayerTfm;

	[SerializeField]
	StaffRollPlayerAct PlayerAct;

	[SerializeField]
	StaffRoll StaffRoll;

	const float Offset = 8.0f;

	void Start ()
	{
		var tfm = transform;

		PlayerTfm.LateUpdateAsObservable().Subscribe(_ => {
			tfm.localPosition = new Vector3(Mathf.Max(11.0f, PlayerTfm.localPosition.x + Offset), tfm.localPosition.y, tfm.localPosition.z);
		})
		.AddTo(this);

		transform.UpdateAsObservable().Where(x => tfm.localPosition.x > 11.0f)
			.First()
			.Subscribe(_ => {
				PlayerAct.setCanInput(true);
				StaffRoll.startStaffRoll();
			})
			.AddTo(this);
	}
}
