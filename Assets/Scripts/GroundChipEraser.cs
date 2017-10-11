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
	/// Rigidbody
	/// </summary>
	Rigidbody rb;

	/// <summary>
	/// Character
	/// </summary>
	[SerializeField]
	Character Character;

	void Start ()
	{
		defaultYPos = transform.localPosition.y;
		rb = GetComponent<Rigidbody>();
	}

	/// <summary>
	/// 地面のチップと接しているかのチェックを始める
	/// </summary>
	public void checkGroundChip()
	{
		rb.isKinematic = false;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (!!rb.isKinematic) {
			return;
		}
		Destroy(collision.gameObject);
		rb.isKinematic= true;
		transform.localPosition = new Vector3(transform.localPosition.x, defaultYPos);
		Character.onErased();
	}
}
