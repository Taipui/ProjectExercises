using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スタッフロール用の地面生成クラス
/// </summary>
public class StaffRollGroundCreater : MonoBehaviour
{
	/// <summary>
	/// GroundChipのPrefab
	/// </summary>
	[SerializeField]
	GameObject GroundChipPrefab;

	/// <summary>
	/// GroundChipの幅
	/// </summary>
	float groundChipWidth;
	/// <summary>
	/// GroundChipの高さ
	/// </summary>
	float groundChipHeight;

	/// <summary>
	/// ゲーム開始時に生成する行数
	/// </summary>
	const int Pre_Create_Row_Num = 5;
	/// <summary>
	/// ゲーム開始時に生成する列数
	/// </summary>
	const int Pre_Create_Col_Num = 150;

	/// <summary>
	/// 現在まで生成した列数
	/// </summary>
	int currentCol;

	/// <summary>
	/// 一回の生成で生成する列数
	/// </summary>
	const int Create_Col = 4;

	/// <summary>
	/// 最大生成列数
	/// </summary>
	const int Max_Create_Col = 50000;

	/// <summary>
	/// 生成時に使用する変数
	/// </summary>
	float row;
	float col;

	/// <summary>
	/// 生成したGroundChipを格納するGameObjectのTransform
	/// </summary>
	[SerializeField]
	Transform GroundChipParent;

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		groundChipWidth = GroundChipPrefab.transform.localScale.x / 100;
		groundChipHeight = GroundChipPrefab.transform.localScale.y / 100;
	}

	void Start ()
	{
		init();
		preCreate();
	}

	/// <summary>
	/// 事前生成
	/// </summary>
	void preCreate()
	{
		for (col = 0.0f; col < groundChipWidth * Pre_Create_Col_Num; col += groundChipWidth) {
			for (var row = 0.0f; row < groundChipHeight * Pre_Create_Row_Num; row += groundChipHeight) {
				var go = Instantiate(GroundChipPrefab, new Vector3(col + groundChipWidth / 2, row + groundChipHeight / 2), Quaternion.identity);
				go.transform.SetParent(GroundChipParent);
			}
		}
	}

	/// <summary>
	/// 地面を生成する
	/// </summary>
	void createStage()
	{
		for (; col < groundChipWidth * currentCol; col += groundChipWidth) {
			for (var row = 0.0f; row < groundChipHeight * Pre_Create_Row_Num; row += groundChipHeight) {
				var go = Instantiate(GroundChipPrefab, new Vector3(col + groundChipWidth / 2, row + groundChipHeight / 2), Quaternion.identity);
				go.transform.SetParent(GroundChipParent);
			}
		}
	}

	/// <summary>
	/// 地面を生成する
	/// </summary>
	public void create()
	{
		if (currentCol >= Max_Create_Col) {
			return;
		}
		currentCol += Create_Col;
		createStage();
	}
}
