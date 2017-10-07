using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class GroundChipEraser : MonoBehaviour
{
	/// <summary>
	/// デフォルトのY座標
	/// </summary>
	float defaultYPos;

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
		defaultYPos = transform.localPosition.y;
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
		transform.localPosition = new Vector3(transform.localPosition.x, defaultYPos);
		Character.onErased();
	}
}
