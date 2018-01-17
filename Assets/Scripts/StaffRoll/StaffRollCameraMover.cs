using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// スタッフロール用のカメラを動かすクラス
/// </summary>
public class StaffRollCameraMover : MonoBehaviour
{
	/// <summary>
	/// キャラクターのTransform
	/// </summary>
	[SerializeField]
	Transform PlayerTfm;

	/// <summary>
	/// スタッフロール用のPlayerMove
	/// </summary>
	StaffRollPlayerMove playerMove;
	/// <summary>
	/// スタッフロール用のPlayerAct
	/// </summary>
	StaffRollPlayerAct playerAct;

	/// <summary>
	/// Unityちゃんと画面の中央のX座標がどれだけ離れているか
	/// </summary>
	const float Offset = 8.0f;

	/// <summary>
	/// 地面のチップを消してからどれだけ動いたか
	/// </summary>
	readonly ReactiveProperty<float> diff = new ReactiveProperty<float>(0.0f);

	/// <summary>
	/// 地面のチップを消すまで移動する量
	/// </summary>
	const float Erase_Threshold = 0.1f;

	/// <summary>
	/// スタッフロール用のGroundCreater
	/// </summary>
	[SerializeField]
	StaffRollGroundCreater Gc;

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		playerMove = PlayerTfm.GetComponent<StaffRollPlayerMove>();
		playerAct = PlayerTfm.GetComponent<StaffRollPlayerAct>();
	}

	void Start ()
	{
		init();

		var tfm = transform;
		var prevX = tfm.localPosition.x;

		PlayerTfm.LateUpdateAsObservable().Subscribe(_ => {
			tfm.localPosition = new Vector3(Mathf.Max(11.0f, PlayerTfm.localPosition.x + Offset), tfm.localPosition.y, tfm.localPosition.z);
			diff.Value = tfm.localPosition.x - prevX;
		})
		.AddTo(this);

		transform.UpdateAsObservable().Where(x => tfm.localPosition.x > 11.0f)
			.First()
			.Subscribe(_ => {
				playerAct.setCanInput(true);
				playerMove.runCheck();
			})
			.AddTo(this);

		diff.AsObservable().Where(val => val >= Erase_Threshold)
			.Subscribe(_ => {
				Gc.create();
				prevX = tfm.localPosition.x;
			})
			.AddTo(this);
	}
}
