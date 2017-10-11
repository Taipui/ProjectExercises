using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地面の生成に関するクラス
/// </summary>
public class GroundCreater : MonoBehaviour
{
	/// <summary>
	/// 地面のチップ
	/// </summary>
	[SerializeField]
	GameObject GroundChip;

	/// <summary>
	/// 横に並べる個数
	/// </summary>
	const int Width_Num = 160;
	/// <summary>
	/// 縦に並べる個数
	/// </summary>
	const int Height_Num = 50;

	/// <summary>
	/// 初期の地面のチップのX座標
	/// </summary>
	const float First_WPos = -7.9f;
	/// <summary>
	/// 初期の地面のチップのY座標
	/// </summary>
	const float First_HPos = -4.985f;

	void Start ()
	{
		var wPos = First_WPos;
		var hPos = First_HPos;

		//for (var h = 0; h < Height_Num; ++h) {
		//	for (var w = 0; w < Width_Num; ++w) {
		//		var obj = Instantiate(GroundChip, new Vector3(wPos, hPos), Quaternion.identity);
		//		obj.transform.SetParent(transform);
		//		wPos += GroundChip.transform.localScale.x / 100;
		//	}
		//	hPos += GroundChip.transform.localScale.y / 100;
		//	wPos = First_WPos;
		//}

		for (var w = 0; w < Width_Num; ++w) {
			for (var h = 0; h < Mathf.Lerp(0.0f, Height_Num, (Mathf.Sin(w * .1f) + 2.0f) / 3); ++h) {
				var obj = Instantiate(GroundChip, new Vector3(wPos, hPos), Quaternion.identity);
				obj.transform.SetParent(transform);
				hPos += GroundChip.transform.localScale.y / 100;
			}
			wPos += GroundChip.transform.localScale.x / 100;
			hPos = First_HPos;
		}
	}
}
