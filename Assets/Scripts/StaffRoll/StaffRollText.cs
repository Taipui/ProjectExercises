using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;


public class StaffRollText : MonoBehaviour
{
	const float Move_Speed = 1.0f;

	void Start ()
	{
		transform.localPosition = new Vector3(25.0f, 6.0f);

		this.UpdateAsObservable().Subscribe(_ => {
			transform.Translate(new Vector2(-Move_Speed * Time.deltaTime, 0.0f));
		})
		.AddTo(this);
	}
}
