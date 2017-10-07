using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Character : MonoBehaviour
{
	/// <summary>
	/// キャラクターのColliderのTransform
	/// </summary>
	[SerializeField]
	protected Transform CharacterColliderTfm;

	/// <summary>
	/// ジャンプ中かどうか
	/// </summary>
	protected bool isJumping;

	/// <summary>
	/// 発射する弾
	/// </summary>
	[SerializeField]
	protected GameObject Bullet;

	/// <summary>
	/// 弾の発射位置
	/// </summary>
	[SerializeField]
	protected Transform LaunchTfm;

	/// <summary>
	/// 消す地面のチップを判断するためのレイの長さ
	/// </summary>
	const float Erase_GroundChip_Ray_Dist = -0.05f;

	/// <summary>
	/// GroundChipErase
	/// </summary>
	[SerializeField]
	protected GroundChipEraser Gce;

	protected virtual void Start ()
	{
		CharacterColliderTfm.position = transform.position;

		this.FixedUpdateAsObservable().Subscribe(_ => {
			transform.position = CharacterColliderTfm.position;
		})
		.AddTo(this);

		this.UpdateAsObservable().Subscribe(_ => {
			Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - Erase_GroundChip_Ray_Dist), Vector3.down, Color.red);
		})
		.AddTo(this);

		//this.FixedUpdateAsObservable().Where(x => !isJumping)
		//	.Subscribe(_ => {
		//		transform.position = CharacterColliderTfm.position;
		//	})
		//.AddTo(this);

		//this.FixedUpdateAsObservable().Where(x => !!isJumping)
		//	.Subscribe(_ => {
		//		CharacterColliderTfm.position = transform.position;
		//	})
		//.AddTo(this);
	}

	/// <summary>
	/// プレイヤーの足元にある地面のチップを消す
	/// </summary>
	protected void eraseGroundChip()
	{
		//var offsetPos = new Vector2(transform.position.x, transform.position.y - Erase_GroundChip_Ray_Dist);
		//var raycast = Physics2D.Raycast(offsetPos, Vector2.down);
		//if (!!raycast && raycast.collider.tag == "Ground") {
		//	Destroy(raycast.collider.gameObject);
		//}

		Gce.checkGroundChip();
	}

	/// <summary>
	/// 地面のチップが消されたら呼ばれる
	/// </summary>
	public virtual void onErased()
	{
		launch();
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	protected virtual void launch()
	{

	}
}

