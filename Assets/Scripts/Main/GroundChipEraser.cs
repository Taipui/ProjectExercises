using UnityEngine;
using UniRx;
using UniRx.Triggers;

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
	/// Player
	/// </summary>
	[SerializeField]
	PlayerAct Player;
	[SerializeField]
	StaffRollPlayerAct StaffRollPlayer;

	/// <summary>
	/// 地面のチップを消す用のCollider
	/// </summary>
	SphereCollider groundChipEraserCollider;

	/// <summary>
	/// 移動中かどうか
	/// </summary>
	bool isMove;

	/// <summary>
	/// 移動速度
	/// </summary>
	const float Move_Speed = 2.0f;

	void Start ()
	{
		defaultYPos = transform.localPosition.y;
		groundChipEraserCollider = GetComponent<SphereCollider>();
		groundChipEraserCollider.enabled = false;
		isMove = false;

		this.UpdateAsObservable().Where(x => !!isMove)
			.Subscribe(_ => {
				transform.Translate(new Vector3(0.0f, -Move_Speed * Time.deltaTime));
			})
			.AddTo(this);
	}

	/// <summary>
	/// 地面のチップと接しているかのチェックを始める
	/// </summary>
	public void checkGroundChip()
	{
		groundChipEraserCollider.enabled = true;
		isMove = true;
	}

	void OnTriggerEnter(Collider col)
	{
		Destroy(col.gameObject);
		isMove = false;
		groundChipEraserCollider.enabled = false;
		transform.localPosition = new Vector3(transform.localPosition.x, defaultYPos);
		if (Player != null) {
			Player.onErased();
			return;
		}
		StaffRollPlayer.onErased();
	}
}
