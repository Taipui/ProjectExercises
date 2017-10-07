using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class GroundChipEraser : MonoBehaviour
{
	/// <summary>
	/// デフォルトの座標
	/// </summary>
	Vector2 defaultPos;

	/// <summary>
	/// Rigidbody2D
	/// </summary>
	Rigidbody2D rb;

	/// <summary>
	/// Character
	/// </summary>
	[SerializeField]
	Character Character;

	void Start ()
	{
		defaultPos = transform.position;
		rb = GetComponent<Rigidbody2D>();
	}

	/// <summary>
	/// 地面のチップと接しているかのチェックを始める
	/// </summary>
	public void checkGroundChip()
	{
		rb.simulated = true;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (!rb.simulated) {
			return;
		}
		rb.simulated = false;
		Destroy(collision.gameObject);
		transform.position = defaultPos;
		Character.onErased();
	}
}
