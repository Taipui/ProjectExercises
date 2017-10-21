using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 球状の風を発生させるクラス
/// </summary>
public class Wind : MonoBehaviour
{
	/// <summary>
	/// 絵を回転させる速度
	/// </summary>
	const float Rotate_Speed = 100.0f;

	/// <summary>
	/// 消えるまでの時間
	/// </summary>
	const float Destroy_Time = 10.0f;

	/// <summary>
	/// 吸引力
	/// </summary>
	const float Turbulence = 100.0f;

	void Start ()
	{
		var col = GetComponent<SphereCollider>();
		Destroy(gameObject, Destroy_Time);

		this.UpdateAsObservable().Subscribe(_ => {
			transform.Rotate(new Vector3(0.0f, 0.0f, Rotate_Speed * Time.deltaTime));
		})
		.AddTo(this);

		col.OnTriggerStayAsObservable().Where(colGo => !!isBullet(colGo))
			.Subscribe(colGo => {
				colGo.gameObject.GetComponent<Rigidbody>().AddForce((transform.position - colGo.transform.position) * Turbulence);
			})
			.AddTo(this);
	}

	/// <summary>
	/// 当たったものが弾かどうか
	/// </summary>
	/// <param name="col">当たったもの</param>
	/// <returns>弾ならtrue</returns>
	bool isBullet(Collider col)
	{
		return col.tag == "Bullet";
	}
}
