using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// スタッフロール用のキャラクターが発射した弾に関するクラス
/// </summary>
public class StaffRollBullet : Bullet
{
	void Start ()
	{
		this.UpdateAsObservable().Where(x => transform.position.x > Camera.main.transform.position.x + 10.0f)
			.Subscribe(_ => {
				Destroy(gameObject);
			})
			.AddTo(this);
	}
}
