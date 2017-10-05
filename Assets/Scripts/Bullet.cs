using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Bullet : MonoBehaviour
{
	const float Kill_Zone = -6.0f;

	void Start ()
	{
		this.UpdateAsObservable().Where(x => transform.position.y < Kill_Zone)
			.Subscribe(_ => {
				Destroy(gameObject);
			})
			.AddTo(this);
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag != "Ground") {
			return;
		}
		Destroy(GetComponent<Rigidbody2D>());
		tag = "Ground";
	}
}

