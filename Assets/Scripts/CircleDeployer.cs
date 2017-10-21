using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子のオブジェクトを円状に配置するクラス
/// </summary>
public class CircleDeployer : MonoBehaviour
{
	/// <summary>
	/// 半径
	/// </summary>
	const float Radius = 1.0f;

	void Start ()
	{
		var childList = new List<GameObject>();
		foreach (Transform child in transform) {
			childList.Add(child.gameObject);
		}

		var angleDiff = 360.0f / childList.Count;

		for (var i = 0; i < childList.Count; ++i) {
			var childPos = transform.position;

			var angle = (90.0f - angleDiff * i) * Mathf.Deg2Rad;
			childPos.x += Radius * Mathf.Cos(angle);
			childPos.z += Radius * Mathf.Sin(angle);

			childList[i].transform.position = childPos;
		}
	}
}
