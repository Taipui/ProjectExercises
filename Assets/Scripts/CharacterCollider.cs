using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// キャラクターの当たり判定に関するクラス
/// </summary>
public class CharacterCollider : MonoBehaviour
{
	/// <summary>
	/// キャラクターのスクリプト
	/// </summary>
	[SerializeField]
	Character Character;

	void Start ()
	{

	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (LayerMask.LayerToName(collision.gameObject.layer) != "Bullet") {
			return;
		}
		if (collision.gameObject.tag == Character.MyBulletTag) {
			return;
		}

		Character.damage();
	}
}

