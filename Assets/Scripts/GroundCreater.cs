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

	/// <summary>
	/// ステージ毎の地面のチップを置き始めるX座標
	/// </summary>
	float wPos;

	int wNum;
	int currentWNum;

	const int Pre_Create_Num = 100;

	int stage;

	void Start ()
	{
		wPos = First_WPos;
		wNum = Pre_Create_Num;
		currentWNum = 0;
		stage = 1;

		//for (var h = 0; h < Height_Num; ++h) {
		//	for (var w = 0; w < Width_Num; ++w) {
		//		var obj = Instantiate(GroundChip, new Vector3(wPos, hPos), Quaternion.identity);
		//		obj.transform.SetParent(transform);
		//		wPos += GroundChip.transform.localScale.x / 100;
		//	}
		//	hPos += GroundChip.transform.localScale.y / 100;
		//	wPos = First_WPos;
		//}

		//for (var w = 0; w < Width_Num; ++w) {
		//	for (var h = 0; h < Mathf.Lerp(0.0f, Height_Num, (Mathf.Sin(w * .1f) + 2.0f) / 3); ++h) {
		//		var obj = Instantiate(GroundChip, new Vector3(wPos, hPos), Quaternion.identity);
		//		obj.transform.SetParent(transform);
		//		hPos += GroundChip.transform.localScale.y / 100;
		//	}
		//	wPos += GroundChip.transform.localScale.x / 100;
		//	hPos = First_HPos;
		//}

		preCreate();

//		stage_2();
	}

	void preCreate()
	{
		stage1();
	}

	public void create()
	{
		wNum += 2;
		stage1();
		stage2();

		//		stage_2();
	}

	void stage1()
	{
		if (stage >= 2) {
			return;
		}
		for (; currentWNum < wNum; ++currentWNum) {
			if (wNum >= Width_Num) {
				++stage;
				wNum = 0;
				currentWNum = 0;
				return;
			}
			var hPos = First_HPos;
			for (var h = 0; h < Height_Num; ++h) {
				var obj = Instantiate(GroundChip, new Vector3(wPos, hPos), Quaternion.identity);
				obj.transform.SetParent(transform);
				hPos += GroundChip.transform.localScale.y / 100;
			}
			wPos += GroundChip.transform.localScale.x / 100;
		}
	}

	void stage2()
	{
		if (stage <= 1) {
			return;
		}
		//		stage1();
		sinStage();
	}

	void stage3()
	{
		stage1();
	}

	void stage4()
	{

	}

	void stage5()
	{

	}

	void sinStage()
	{
		var hPos = First_HPos;
		for (; currentWNum < wNum; ++currentWNum) {
			for (var h = 0; h < Mathf.Lerp(0.0f, Height_Num, (Mathf.Sin(currentWNum * .1f) + 2.0f) / 3); ++h) {
				var obj = Instantiate(GroundChip, new Vector3(wPos, hPos), Quaternion.identity);
				obj.transform.SetParent(transform);
				hPos += GroundChip.transform.localScale.y / 100;
			}
			wPos += GroundChip.transform.localScale.x / 100;
			hPos = First_HPos;
		}
	}

	void stage_2()
	{
		var hPos = First_HPos;
		for (var h = 0; h < Height_Num; ++h) {
			for (var w = 0; w < Width_Num; ++w) {
				var obj = Instantiate(GroundChip, new Vector3(wPos, hPos), Quaternion.identity);
				obj.transform.SetParent(transform);
				wPos += GroundChip.transform.localScale.x / 100;
			}
			hPos += GroundChip.transform.localScale.y / 100;
			wPos = First_WPos;
		}
	}
}
