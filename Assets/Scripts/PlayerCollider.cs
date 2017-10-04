using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
	GameObject contactGroundChip;

	void Start ()
	{
		
	}

	/// <summary>
	/// プレイヤーと接触している地面のチップを消す
	/// </summary>
	public void eraseGroundChip()
	{
		Destroy(contactGroundChip);
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		contactGroundChip = collision.gameObject;
	}
}

