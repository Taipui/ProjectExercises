using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スタッフロール用の雪弾を管理するクラス
/// </summary>
public class StaffRollBulletManager : MonoBehaviour
{
	/// <summary>
	/// スタッフロール用の雪弾のPrefab
	/// </summary>
	[SerializeField]
	GameObject StaffRollBulletPrefab;

	/// <summary>
	/// 初期生成する雪弾の数
	/// </summary>
	const int Pre_Create_Bullet_Num = 30;

	/// <summary>
	/// StaffRoll
	/// </summary>
	[SerializeField]
	StaffRoll StaffRoll;

	void Start ()
	{
		for (var i = 0; i < Pre_Create_Bullet_Num; ++i) {
			var bulletGo = Instantiate(StaffRollBulletPrefab, transform);
			bulletGo.GetComponent<StaffRollBullet>().setStaffRoll(StaffRoll);
		}
	}

	/// <summary>
	/// 使用可能な雪弾のTransformを返す
	/// </summary>
	/// <returns>使用可能な雪弾があれば、そのTransform、なければnull</returns>
	public Transform getBullet()
	{
		foreach (Transform child in transform) {
			if (!!child.GetComponent<StaffRollBullet>().available()) {
				return child;
			}
		}
		return null;
	}
}

