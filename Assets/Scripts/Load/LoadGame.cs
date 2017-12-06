using UnityEngine;
using UnityEngine.Assertions;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// ロード中にゲームを行うクラス
/// </summary>
public class LoadGame : MonoBehaviour
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
	/// Piyoを生成する間隔
	/// </summary>
	const float Spawn_Interval = 1.0f;
	/// <summary>
	/// Piyoをランダムに生成するX方向の範囲
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
	/// クリックしたPiyoのスクリプト
	/// </summary>
	Piyo clickedPiyo;

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

		Observable.Interval(System.TimeSpan.FromSeconds(Spawn_Interval)).Subscribe(_ => {
			spawnPiyo();
		})
		.AddTo(this);

		this.UpdateAsObservable().Where(x => !!checkPiyo())
			.Subscribe(_ => {
				kill();
			})
			.AddTo(this);
	}

	/// <summary>
	/// Piyoを生成する
	/// </summary>
	void spawnPiyo()
	{
		var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.SetPositionAndRotation(new Vector3(Random.Range(-Spawn_X_Range, Spawn_X_Range), 10.0f), Random.rotation);

		var r = Random.Range(0, PiyoMats.Length);
		sphere.AddComponent<Piyo>().setColIndex(r);
		sphere.GetComponent<Renderer>().material = PiyoMats[r];

		var rb = sphere.AddComponent<Rigidbody>();
		rb.constraints = RigidbodyConstraints.FreezePositionZ;
		rb.AddTorque(Vector3.up * Random.Range(-Rotate_Force, Rotate_Force), ForceMode.Impulse);
		rb.mass = Mass;

		sphere.GetComponent<SphereCollider>().material = PiyoPm;
	}

	/// <summary>
	/// マウスオーバーしたオブジェクトをチェックする
	/// </summary>
	/// <returns>マウスオーバーしたものの情報</returns>
	RaycastHit checkMouseOverObj()
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		var hit = new RaycastHit();
		Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity);
		return hit;
	}

	/// <summary>
	/// Piyoをクリックしたかどうか
	/// </summary>
	/// <returns>クリックしたものがPiyoならtrue</returns>
	bool checkPiyo()
	{
		if (!Input.GetMouseButtonDown(0)) {
			return false;
		}
		var col = checkMouseOverObj().collider;
		if (col == null) {
			return false;
		}
		clickedPiyo = col.gameObject.GetComponent<Piyo>();
		return clickedPiyo != null;
	}

	/// <summary>
	/// Piyoを消す
	/// </summary>
	void kill()
	{
		clickedPiyo.dead(DeadParticlePrefab, ParticleCols[clickedPiyo.ColIndex]);
	}
}
