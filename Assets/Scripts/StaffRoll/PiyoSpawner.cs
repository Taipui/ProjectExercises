using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Piyoを出現させるクラス
/// </summary>
public class PiyoSpawner : MonoBehaviour
{
	/// <summary>
	/// Sphereに適用するPiyoのマテリアルの配列
	/// </summary>
	[SerializeField]
	Material[] PiyoMats;
	/// <summary>
	/// Piyoに適用するPhysicsMaterial
	/// </summary>
	[SerializeField]
	PhysicMaterial PiyoPm;
	/// <summary>
	/// Piyoが消える時に生成するパーティクルのPrefab
	/// </summary>
	[SerializeField]
	GameObject DeadParticlePrefab;
	/// <summary>
	/// Piyoが消える時に生成するパーティクルの色の配列
	/// </summary>
	[SerializeField]
	Color[] ParticleCols;

	/// <summary>
	/// カメラからどれだけ離れた位置をPiyoの生成時の基準にするか
	/// </summary>
	const float Offset_X = 10.0f;
	/// <summary>
	/// Piyoをランダムに生成するX方向の範囲(Offset_X + Random.Range(0, Spawn_X_Range))
	/// </summary>
	const float Spawn_X_Range = 5.0f;
	/// <summary>
	/// Piyoを回転させる力
	/// </summary>
	const float Rotate_Force = 10.0f;
	/// <summary>
	/// Piyoの質量
	/// </summary>
	const float Mass = 1.0f;

	/// <summary>
	/// ランダムな間隔でPiyoを出現させる時の最低間隔
	/// </summary>
	const float Spawn_Interval_Base = 15.0f;
	/// <summary>
	/// ランダムな間隔でPiyoを出現させる時のランダムな間隔の範囲(Random_Interval_Base + Random.Range(0, Random_Interval_Random))
	/// </summary>
	const float Spawn_Interval_Random = 15.0f;

	/// <summary>
	/// 自身のTransform
	/// </summary>
	Transform tfm;

	/// <summary>
	/// アサートのチェック
	/// </summary>
	void assertCheck()
	{
		// PiyoMatsのチェック
		Assert.IsNotNull(PiyoMats, "PiyoMats is null");
		Assert.AreNotEqual(PiyoMats.Length, 0, "PiyoMats.Length is zero");
		for (var i = 0; i < PiyoMats.Length; ++i) {
			Assert.IsNotNull(PiyoMats[i], "PiyoMats[" + i + "] is null");
			for (var j = i + 1; j < PiyoMats.Length; ++j) {
				Assert.AreNotEqual(PiyoMats[i], PiyoMats[j], "PiyoMats[" + i + "] and PiyoMats[" + j + "] are the same");
			}
		}

		// PiyoPmのチェック
		Assert.IsNotNull(PiyoPm, "PiyoPm is null");

		// DeadParticlePrefabのチェック
		Assert.IsNotNull(DeadParticlePrefab, "DeadParticlePrefab is null");

		// ParticleColsのチェック
		Assert.IsNotNull(ParticleCols, "ParticleCols is null");
		Assert.AreNotEqual(ParticleCols.Length, 0, "ParticleCols.Length is zero");
		for (var i = 0; i < ParticleCols.Length; ++i) {
			for (var j = i + 1; j < PiyoMats.Length; ++j) {
				Assert.AreNotEqual(ParticleCols[i], ParticleCols[j], "ParticleCols[" + i + "] and ParticleCols[" + j + "] are the same");
			}
		}
	}

	void Start ()
	{
		assertCheck();

		tfm = transform;

		StartCoroutine("randomInterval");
	}

	/// <summary>
	/// ランダムな間隔で実行する
	/// </summary>
	/// <returns></returns>
	IEnumerator randomInterval()
	{
		while (true) {
			yield return new WaitForSeconds(Spawn_Interval_Base + Random.Range(0, Spawn_Interval_Random));
			spawn();
		}
	}

	/// <summary>
	/// Piyoを出現させる
	/// </summary>
	void spawn()
	{
		var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.SetPositionAndRotation(new Vector3(tfm.position.x + Offset_X + Random.Range(0, Spawn_X_Range), 10.0f), Random.rotation);

		var r = Random.Range(0, PiyoMats.Length);
		sphere.AddComponent<Piyo>().setVals(r, DeadParticlePrefab, ParticleCols);
		sphere.GetComponent<Renderer>().material = PiyoMats[r];

		var rb = sphere.AddComponent<Rigidbody>();
		rb.constraints = RigidbodyConstraints.FreezePositionZ;
		rb.AddTorque(Vector3.up * Random.Range(-Rotate_Force, Rotate_Force), ForceMode.Impulse);
		rb.mass = Mass;

		sphere.GetComponent<SphereCollider>().material = PiyoPm;

		sphere.tag = "Piyo";
	}
}

