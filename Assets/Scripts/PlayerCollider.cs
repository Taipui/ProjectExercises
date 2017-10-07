using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
	/// <summary>
	/// 最初に接した地面のチップのGameObject
	/// </summary>
	GameObject contactGroundChip;

	void Start ()
	{

	}

	/// <summary>
	/// プレイヤーと接触している地面のチップを消す
	/// </summary>
	public void eraseGroundChip()
	{
		if (contactGroundChip == null) {
			return;
		}
		Destroy(contactGroundChip);
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision == null) {
			return;
		}
		if (collision.gameObject.tag != "Ground") {
			return;
		}
		contactGroundChip = collision.gameObject;
	}
}

