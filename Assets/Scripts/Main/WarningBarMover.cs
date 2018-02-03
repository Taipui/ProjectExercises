using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

/// <summary>
/// 帯を動かすクラス
/// </summary>
public class WarningBarMover : MonoBehaviour
{
	/// <summary>
	/// 上の帯かどうか
	/// </summary>
	[SerializeField]
	bool isUpper;

	/// <summary>
	/// 帯のスライドインが終わる時のY座標
	/// </summary>
	float distY;
	/// <summary>
	/// 帯の初期のY座標
	/// </summary>
	float defaultY;
	/// <summary>
	/// 帯のスライドインが終わった状態からどれだけ動かすと隠れるかのY座標
	/// </summary>
	const float Default_Offset = 100.0f;

	/// <summary>
	/// 帯の動く速度
	/// </summary>
	float moveSpeed;
	/// <summary>
	/// 帯がスライドイン/スライドアウトする速度
	/// </summary>
	float slideSpeed;

	void Start ()
	{
		moveSpeed = 0;

		var tfm = transform;

		distY = tfm.position.y;

		var defaultOffset = !!isUpper ? Default_Offset : -Default_Offset;
		tfm.position = new Vector3(tfm.position.x, tfm.position.y + defaultOffset, tfm.position.z);
		defaultY = tfm.position.y;

		this.UpdateAsObservable().Where(x => moveSpeed != 0)
			.Subscribe(_ => {
			tfm.Translate(new Vector3(moveSpeed * Time.deltaTime, 0));
		})
		.AddTo(this);
	}

	/// <summary>
	/// 帯を動かす
	/// </summary>
	/// <param name="moveSpeed_">帯の動く速度</param>
	/// <param name="playTime">帯の再生時間</param>
	/// <param name="slideSpeed_">帯がスライドイン/スライドアウトする速度</param>
	public void play(float moveSpeed_, float playTime, float slideSpeed_)
	{
		moveSpeed = moveSpeed_;
		if (!!isUpper) {
			moveSpeed = -moveSpeed;
		}
		slideSpeed = slideSpeed_;
		Invoke("end", playTime);

		transform.DOMoveY(
			distY,
			slideSpeed
		);

	}

	/// <summary>
	/// 演出の終了
	/// </summary>
	void end()
	{
		transform.DOMoveY(
			defaultY,
			slideSpeed
		).OnComplete(() => {
			Destroy(gameObject);
		});

	}
}
