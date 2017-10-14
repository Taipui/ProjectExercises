using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地面のチップを消すためのクラス
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
	/// Player
	/// </summary>
	[SerializeField]
	Player Player;

	/// <summary>
	/// 地面のチップを消す用のCollider
	/// </summary>
	MeshCollider groundChipEraserCollider;

	void Start ()
	{
		defaultYPos = transform.localPosition.y;
		rb = GetComponent<Rigidbody>();
		groundChipEraserCollider = GetComponent<MeshCollider>();
		groundChipEraserCollider.enabled = false;
	}

	/// <summary>
	/// 地面のチップと接しているかのチェックを始める
	/// </summary>
	public void checkGroundChip()
	{
		groundChipEraserCollider.enabled = true;
		rb.isKinematic = false;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (!!rb.isKinematic) {
			return;
		}

		Destroy(collision.gameObject);
		rb.isKinematic= true;
		groundChipEraserCollider.enabled = false;
		transform.localPosition = new Vector3(transform.localPosition.x, defaultYPos);
		Player.onErased();
	}
}
