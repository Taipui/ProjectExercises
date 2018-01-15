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

		PlayerTfm.LateUpdateAsObservable().Subscribe(_ => {
			var prevPosX = tfm.localPosition.x;
			tfm.localPosition = new Vector3(Mathf.Max(11.0f, PlayerTfm.localPosition.x + Offset), tfm.localPosition.y, tfm.localPosition.z);
		})
		.AddTo(this);

		transform.UpdateAsObservable().Where(x => tfm.localPosition.x > 11.0f)
			.First()
			.Subscribe(_ => {
				playerAct.setCanInput(true);
				playerMove.runCheck();
			})
			.AddTo(this);
	}
}
