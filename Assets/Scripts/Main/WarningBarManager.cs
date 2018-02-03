using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WARNINGの帯をまとめるクラス
/// </summary>
public class WarningBarManager : MonoBehaviour
{
	/// <summary>
	/// WarningBarManagerの配列
	/// </summary>
	[SerializeField]
	WarningBarMover[] Wbms;

	/// <summary>
	/// 帯が動く速度
	/// </summary>
	const float Move_Speed = 80.0f;

	/// <summary>
	/// 帯の再生時間
	/// </summary>
	const float Play_Time = 0.8f;

	/// <summary>
	/// 帯がスライドイン/スライドアウトする速度
	/// </summary>
	const float Slide_Speed = 0.2f;

	void Start ()
	{
	}

	/// <summary>
	/// 帯を動かす
	/// </summary>
	public void play()
	{
		for (var i = 0; i < Wbms.Length; ++i) {
			Wbms[i].play(Move_Speed, Play_Time, Slide_Speed);
		}
	}
}
