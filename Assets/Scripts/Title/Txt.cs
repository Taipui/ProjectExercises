﻿using UnityEngine;

/// <summary>
/// 3D文字に関するクラス
/// </summary>
public class Txt : MonoBehaviour
{
	/// <summary>
	/// Title1
	/// </summary>
	[SerializeField]
	TitleBase Title;

	/// <summary>
	/// メニューの番号
	/// </summary>
	[SerializeField]
	int Index;
	public int Index_ {
		set
		{
			Index = value;
		}
		get
		{
			return Index;
		}
	}

	/// <summary>
	/// 当たり判定をチェックするかどうか
	/// </summary>
	[SerializeField]
	bool IsCheckCol;

	void OnCollisionEnter(Collision col)
	{
		if (!IsCheckCol) {
			return;
		}
		if (col.gameObject.tag != "Bullet") {
			return;
		}
		if (!Title.canInput) {
			return;
		}
		Title.setCurrentSelect(Index);
		Destroy(col.gameObject);
	}
}
