using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class GroundChipChecker : MonoBehaviour
{
	/// <summary>
	/// 自身のコライダ
	/// </summary>
	BoxCollider col;

	[SerializeField]
	GroundCreater Gc;

	float eraseGroundChipX;

	void Start ()
	{
		col = GetComponent<BoxCollider>();
		eraseGroundChipX = 0.0f;

		col.OnTriggerExitAsObservable().Where(colObj => colObj.gameObject.tag == "Ground")
			.Subscribe(colObj => {
				Destroy(colObj.gameObject);
				if (!!isCreate(col.gameObject.transform.position.x)) {
//					Gc.create();
				}
				eraseGroundChipX = col.gameObject.transform.position.x;
			})
			.AddTo(this);
	}

	bool isCreate(float colX)
	{
		return colX - eraseGroundChipX > 0.0f;
	}
}
