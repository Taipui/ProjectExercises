using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 左に見切れたGameObjectを削除するクラス
/// </summary>
public class Eraser : MonoBehaviour
{
	/// <summary>
	/// 自身のコライダ
	/// </summary>
	BoxCollider col;

	void Start ()
	{
		col = GetComponent<BoxCollider>();

		col.OnTriggerExitAsObservable().Where(colObj => !!isErase(colObj.tag))
			.Subscribe(colObj => {
				Destroy(colObj.gameObject);
			})
			.AddTo(this);
	}

	/// <summary>
	/// 削除する対象かどうか
	/// </summary>
	/// <param name="tag">当たったGamoObjectのtag</param>
	/// <returns>削除対象ならtrue</returns>
	bool isErase(string tag)
	{
		return tag == "Ground" || tag == "Signboard" || tag == "Obstacle";
	}
}
