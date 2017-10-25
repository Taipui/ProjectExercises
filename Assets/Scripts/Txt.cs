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
	Title1 Title1;

	/// <summary>
	/// メニューの番号
	/// </summary>
	[SerializeField]
	int Index;

	void Start ()
	{
		
	}

	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag != "Bullet") {
			return;
		}
		Title1.setCurrentSelect(Index);
		Destroy(col.gameObject);
	}
}
