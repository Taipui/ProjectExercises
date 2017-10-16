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
	/// 自身のコライダ
	/// </summary>
	SphereCollider col;

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
	const float Turbulence = 50.0f;

	void Start ()
	{
		col = GetComponent<SphereCollider>();
		Destroy(gameObject, Destroy_Time);

		this.UpdateAsObservable().Subscribe(_ => {
			transform.Rotate(new Vector3(0.0f, 0.0f, Rotate_Speed * Time.deltaTime));
		})
		.AddTo(this);

		col.OnTriggerStayAsObservable().Where(colObj => colObj.tag == "Bullet")
			.Subscribe(colObj => {
				colObj.gameObject.GetComponent<Rigidbody>().AddForce((transform.position - colObj.transform.position) * Turbulence);
			})
			.AddTo(this);
	}
}
