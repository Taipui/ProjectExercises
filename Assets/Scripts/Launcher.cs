using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弾の発射に関するクラス
/// </summary>
public class Launcher : MonoBehaviour
{
	void Start ()
	{
		
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	/// <param name="bullet">発射する弾のGameObject</param>
	/// <param name="targetPos">弾の到達地点</param>
	/// <param name="layerNo">適用するレイヤーの番号</param>
	/// <param name="bulletParent">発射した弾を格納するGameObjectのTransform</param>
	/// <param name="vec">弾に力を加えるベクトル</param>
	public void launch(GameObject bullet, Vector3 targetPos, int layerNo, Transform bulletParent, Vector3 vec)
	{
		var go = Instantiate(bullet, bulletParent);
		var rb = go.GetComponent<Rigidbody>();
		go.transform.position = new Vector3(transform.position.x, transform.position.y);
		if (layerNo == 13) {
			rb.velocity = vec;
		} else {
			// 速さベクトルのままAddForce()を渡してはいけないぞ。力(速さ×重さ)に変換するんだ
			var force = vec * rb.mass;
			rb.AddForce(force, ForceMode.Impulse);
		}
		go.layer = layerNo;
	}
}
