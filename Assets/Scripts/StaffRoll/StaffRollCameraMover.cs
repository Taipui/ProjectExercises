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

	float wordDist;

	void Start ()
	{
		wordDist = 0.0f;

		var tfm = transform;

		PlayerTfm.LateUpdateAsObservable().Subscribe(_ => {
			var prevPosX = tfm.localPosition.x;
			tfm.localPosition = new Vector3(Mathf.Max(11.0f, PlayerTfm.localPosition.x + Offset), tfm.localPosition.y, tfm.localPosition.z);
			wordDist += tfm.localPosition.x - prevPosX;
		})
		.AddTo(this);

		transform.UpdateAsObservable().Where(x => tfm.localPosition.x > 11.0f)
			.First()
			.Subscribe(_ => {
				PlayerAct.setCanInput(true);
				StaffRoll.createStr();
				resetWordDist();
			})
			.AddTo(this);

		transform.UpdateAsObservable().Where(x => wordDist > 5.0f)
			.First()
			.Subscribe(_ => {
				StaffRoll.createStr();
			})
			.AddTo(this);
	}

	public void resetWordDist()
	{
		Debug.Log("resetWordDist");
		wordDist = 0.0f;
	}
}
