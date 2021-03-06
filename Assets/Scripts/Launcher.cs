﻿using UnityEngine;

/// <summary>
/// 弾の発射に関するクラス
/// </summary>
public class Launcher : MonoBehaviour
{
	/// <summary>
	/// 雪弾を生成して発射する
	/// </summary>
	/// <param name="bullet">発射する弾のGameObject</param>
	/// <param name="targetPos">弾の到達地点</param>
	/// <param name="layerNo">適用するレイヤーの番号</param>
	/// <param name="bulletParent">発射した弾を格納するGameObjectのTransform</param>
	/// <param name="vec">弾に力を加えるベクトル</param>
	/// <param name="scale">弾のスケール(倍)</param>
	/// <returns>発射した雪弾のGameObject</returns>
	public GameObject createLaunch(GameObject bullet, Vector3 targetPos, int layerNo, Transform bulletParent, Vector3 vec, float scale = 1.0f)
	{
		var go = Instantiate(bullet, bulletParent);
		var rb = go.GetComponent<Rigidbody>();
		go.transform.position = new Vector3(transform.position.x, transform.position.y);
		go.transform.localScale *= scale;
		if (layerNo == Common.PlayerBulletLayer) {
			rb.velocity = vec;
		} else {
			// 速さベクトルのままAddForce()を渡してはいけないぞ。力(速さ×重さ)に変換するんだ
			var force = vec * rb.mass;
			rb.AddForce(force, ForceMode.Impulse);
		}
		go.layer = layerNo;

		return go;
	}

	/// <summary>
	/// 雪弾を使い回して発射する
	/// </summary>
	/// <param name="bulletManager">StaffRollBulletManager</param>
	/// <param name="targetPos">弾の到達地点</param>
	/// <param name="vec">弾に力を加えるベクトル</param>
	/// <param name="scale">弾のスケール(倍)</param>
	public void cycleLaunch(StaffRollBulletManager bulletManager, Vector3 targetPos, Vector3 vec, float scale = 1.0f)
	{
		var bulletTfm = bulletManager.getBullet();
		if (bulletTfm == null) {
			Debug.Log("使用可能な雪弾がありません");
			return;
		}
		var rb = bulletTfm.GetComponent<Rigidbody>();
		bulletTfm.position = new Vector3(transform.position.x, transform.position.y);
		bulletTfm.localScale *= scale;
		rb.velocity = vec;
		bulletTfm.GetComponent<StaffRollBullet>().launch();
	}
}
