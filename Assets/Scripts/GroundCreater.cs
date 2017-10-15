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

	/// <summary>
	/// 1回のcreateで生成する列数
	/// </summary>
	int wNum;
	/// <summary>
	/// 現在まで生成した列数
	/// </summary>
	int currentWNum;

	/// <summary>
	/// ゲーム開始時に生成する列数
	/// </summary>
	const int Pre_Create_Num = 100;

	/// <summary>
	/// 現在のステージ
	/// </summary>
	int stage;

	/// <summary>
	/// 前フレームのSinの符号
	/// </summary>
	int prevSign;
	/// <summary>
	/// 波を細かくしていく度合い
	/// </summary>
	const float Wave_Fineness_Add_Val = 0.005f;
	float waveFineness;

	void Start ()
	{
		wPos = First_WPos;
		wNum = Pre_Create_Num;
		currentWNum = 0;
		stage = 1;
		prevSign = 1;
		waveFineness = 0.1f;

		preCreate();
	}

	/// <summary>
	/// 事前に生成する
	/// </summary>
	void preCreate()
	{
		stage1();
	}

	/// <summary>
	/// ステージを生成する
	/// </summary>
	public void create()
	{
		wNum += 2;
		stage1();
		stage2();
		stage3();
		stage4();
		stage5();
	}

	/// <summary>
	/// ステージ1(少し平坦なマップ)
	/// </summary>
	void stage1()
	{
		if (stage != 1) {
			return;
		}
		normalStage(new Color32(117, 91, 165, 255));
	}

	/// <summary>
	/// ステージ2(坂を上ったり下がったり反応を楽しむステージ)
	/// </summary>
	void stage2()
	{
		if (stage != 2) {
			return;
		}
		normalStage(new Color32(224, 84, 128, 255));
	}

	/// <summary>
	/// ステージ3(風の能力が使いやすいステージ)
	/// </summary>
	void stage3()
	{
		if (stage != 3) {
			return;
		}
		normalStage(new Color32(243, 154, 28, 255));
	}

	/// <summary>
	/// ステージ4(凹凸の激しいマップ)
	/// </summary>
	void stage4()
	{
		if (stage != 4) {
			return;
		}
		for (; currentWNum < wNum; ++currentWNum) {
			if (wNum >= Width_Num) {
				stageTransition();
				return;
			}
			var hPos = First_HPos;
			for (var h = 0; h < Mathf.Lerp(0.0f, Height_Num, (Mathf.Sin(currentWNum * 3) + 1.0f) / 2); ++h) {
				var obj = Instantiate(GroundChip, new Vector3(wPos, hPos), Quaternion.identity);
				obj.transform.SetParent(transform);
				hPos += GroundChip.transform.localScale.y / 100;

				obj.GetComponent<SpriteRenderer>().color = new Color32(103, 183, 52, 255);
			}
			wPos += GroundChip.transform.localScale.x / 100;
		}
	}

	/// <summary>
	/// ステージ5(すべての力を使ってぐれちゃんを倒すマップ)
	/// </summary>
	void stage5()
	{
		if (stage != 5) {
			return;
		}
		for (; currentWNum < wNum; ++currentWNum) {
			if (wNum >= Width_Num) {
				stageTransition();
				return;
			}
			var hPos = First_HPos;
			for (var h = 0; h < Mathf.Lerp(0.0f, Height_Num, (Mathf.Sin(currentWNum * waveFineness) + 2.0f) / 3); ++h) {
				var obj = Instantiate(GroundChip, new Vector3(wPos, hPos), Quaternion.identity);
				obj.transform.SetParent(transform);
				hPos += GroundChip.transform.localScale.y / 100;

				obj.GetComponent<SpriteRenderer>().color = new Color32(19, 152, 214, 255);
			}
			if (prevSign == -1 && prevSign != (int)Mathf.Sign(Mathf.Sin(currentWNum))) {
				waveFineness += 0.005f;
			}
			prevSign = (int)Mathf.Sign(Mathf.Sin(currentWNum));
			wPos += GroundChip.transform.localScale.x / 100;
			hPos = First_HPos;
		}
	}

	/// <summary>
	/// ステージ遷移に必要な処理
	/// </summary>
	void stageTransition()
	{
		++stage;
		wNum = 0;
		currentWNum = 0;
	}

	/// <summary>
	/// ただ平坦なステージ
	/// </summary>
	void normalStage(Color32 col)
	{
		for (; currentWNum < wNum; ++currentWNum) {
			if (wNum >= Width_Num) {
				stageTransition();
				return;
			}
			var hPos = First_HPos;
			for (var h = 0; h < Height_Num; ++h) {
				var obj = Instantiate(GroundChip, new Vector3(wPos, hPos), Quaternion.identity);
				obj.transform.SetParent(transform);
				hPos += GroundChip.transform.localScale.y / 100;

				obj.GetComponent<SpriteRenderer>().color = col;
			}
			wPos += GroundChip.transform.localScale.x / 100;
		}
	}

	/// <summary>
	/// サイン波のテストステージ
	/// </summary>
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
}
