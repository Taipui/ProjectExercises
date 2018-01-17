using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スタッフロール用の地面生成クラス
/// </summary>
public class StaffRollGroundCreater : MonoBehaviour
{
	[SerializeField]
	GameObject GroundChipPrefab;

	float groundChipWidth;
	float groundChipHeight;

	/// <summary>
	/// ゲーム開始時に生成する行数
	/// </summary>
	const int Pre_Create_Row_Num = 5;
	/// <summary>
	/// ゲーム開始時に生成する列数
	/// </summary>
	const int Pre_Create_Col_Num = 10000;

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
		for (var row = 0.0f; row < groundChipWidth * Pre_Create_Col_Num; row += groundChipWidth) {
			for (var col = 0.0f; col < groundChipHeight * Pre_Create_Row_Num; col += groundChipHeight) {
				var go = Instantiate(GroundChipPrefab, new Vector3(row + groundChipWidth / 2, col + groundChipHeight / 2), Quaternion.identity);
				//go.GetComponent<SpriteRenderer>().color = Color.red;
			}
		}
	}
}
