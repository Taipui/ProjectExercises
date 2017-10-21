using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ちょっとした地面を生成するクラス
/// </summary>
public class SmallGroundCreater : MonoBehaviour
{
	/// <summary>
	/// 並べるGameObject
	/// </summary>
	[SerializeField]
	GameObject GroundChip;
	/// <summary>
	/// 縦に並べる個数
	/// </summary>
	[SerializeField]
	int HNum;
	/// <summary>
	/// 横に並べる個数
	/// </summary>
	[SerializeField]
	int WNum;

	/// <summary>
	/// 並べるGameObjectの幅と高さ
	/// </summary>
	Vector2 groundChipeScale;

	void Awake()
	{
		groundChipeScale = GroundChip.transform.localScale;
		groundChipeScale /= 100;

		create();
	}

	void Start ()
	{
	}

	/// <summary>
	/// 地面を作成する
	/// </summary>
	void create()
	{
		var wPos = 0.0f;
		var hPos = 0.0f;
		for (var w = 0; w < WNum; ++w) {
			for (var h = 0; h < HNum; ++h) {
				var obj = Instantiate(GroundChip, new Vector3(wPos, hPos), Quaternion.identity);
				obj.transform.SetParent(transform);
				hPos += groundChipeScale.y;
			}
			hPos = 0.0f;
			wPos += groundChipeScale.x;
		}
	}
}
