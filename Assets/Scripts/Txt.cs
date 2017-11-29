using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	void Start ()
	{

	}

	void OnCollisionEnter(Collision col)
	{
		if (!!Title.IsDecided) {
			return;
		}
		if (!IsCheckCol) {
			return;
		}
		if (col.gameObject.tag != "Bullet") {
			return;
		}
		Title.setCurrentSelect(Index);
		Destroy(col.gameObject);
		Title.setIsDecided();
	}
}
