using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 変身時にプレイヤーの周囲のオブジェクトを破壊するクラス
/// </summary>
public class DestroyCollider : MonoBehaviour
{
	/// <summary>
	/// 自身のコライダ
	/// </summary>
	SphereCollider col;

	/// <summary>
	/// 破壊するかどうかのフラグ
	/// </summary>
	bool isDestroy;

	/// <summary>
	/// 破壊するオブジェクトをチェックする回数
	/// </summary>
	readonly ReactiveProperty<int> contactCnt = new ReactiveProperty<int>(0);

	/// <summary>
	/// 最大破壊数(89が看板が安定して消える最低ライン)
	/// </summary>
	const int Max_Contact = 89;

	/// <summary>
	/// Launcher
	/// </summary>
	[SerializeField]
	Launcher Launcher;
	/// <summary>
	/// エフェクト用の弾を発射するTransform
	/// </summary>
	[SerializeField]
	Transform LaucherTfm;

	/// <summary>
	/// 発射する弾
	/// </summary>
	[SerializeField]
	GameObject Bullet;
	/// <summary>
	/// 発射した弾を格納するGameObjectのTransform
	/// </summary>
	[SerializeField]
	Transform BulletParent;

	/// <summary>
	/// ランダムな座標を作る時の範囲
	/// </summary>
	const float Pos_Range = 10.0f;

	void Start ()
	{
		col = GetComponent<SphereCollider>();
		isDestroy = false;
		col.enabled = false;
		var contactGoArray = new GameObject[Max_Contact];

		col.OnTriggerStayAsObservable().Where(colGo => !!isDestroy && contactCnt.Value < Max_Contact)
			.Subscribe(colGo => {
				contactGoArray[contactCnt.Value++] = colGo.gameObject;
			})
			.AddTo(this);

		contactCnt.AsObservable().Where(val => val >= Max_Contact)
			.Subscribe(_ => {
				for (var i = 0; i < Max_Contact; ++i) {
					Destroy(contactGoArray[i]);
				}
				contactCnt.Value = 0;
				col.enabled = false;
				isDestroy = false;

				for (var i = 0; i < 10; ++i) {
					var go = Launcher.launch(Bullet, randomVec(), 13, BulletParent, randomVec());
					Destroy(go, 5.0f);
				}
			})
			.AddTo(this);
	}

	/// <summary>
	/// 破壊のフラグを立てる
	/// </summary>
	public void destroy()
	{
		isDestroy = true;
		col.enabled = true;
	}

	/// <summary>
	/// ランダムなベクトルを返す
	/// </summary>
	/// <returns>ランダムなベクトル</returns>
	Vector3 randomVec()
	{		
		var vec = new Vector3(Random.Range(transform.localPosition.x - Pos_Range, transform.localPosition.x + Pos_Range), 
			Random.Range(transform.localPosition.y, transform.localPosition.y + Pos_Range), 
			transform.localPosition.z);
		return vec;
	}
}
