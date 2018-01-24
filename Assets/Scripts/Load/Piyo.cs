using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// Piyoのクラス
/// </summary>
public class Piyo : MonoBehaviour
{
	/// <summary>
	/// 下回ったら自身を消すY座標
	/// </summary>
	const float Kill_Y = -10.0f;

	/// <summary>
	/// Piyoの種類
	/// </summary>
	public int ColIndex { private set; get; }

	/// <summary>
	/// ParticleのPrefab
	/// </summary>
	GameObject particlePrefab;

	/// <summary>
	/// Particleの色の配列
	/// </summary>
	Color[] particleCols;

	/// <summary>
	/// Piyoに各種変数をセット
	/// </summary>
	/// <param name="index">種類</param>
	/// <param name="prefab">ParticleのPrefab</param>
	/// <param name="cols">Particleの色の配列</param>
	public void setVals(int index, GameObject prefab, Color[] cols)
	{
		ColIndex = index;
		particlePrefab = prefab;
		particleCols = cols;
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{

	}

	void Start ()
	{
		init();

		this.UpdateAsObservable().Where(x => transform.position.y <= Kill_Y)
			.Subscribe(_ => {
				Destroy(gameObject);
			})
			.AddTo(this);
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	public void dead()
	{
		var go = Instantiate(particlePrefab, new Vector3(transform.position.x, transform.position.y, -5.0f), Quaternion.identity);
		var goParticle = go.GetComponent<ParticleSystem>();
		var goCol = new ParticleSystem.MinMaxGradient();
		goCol.mode = ParticleSystemGradientMode.Color;
		goCol.color = particleCols[ColIndex];
		var main = goParticle.main;
		main.startColor = goCol;
		Destroy(go, goParticle.main.duration * 2);
		Destroy(gameObject);
	}
}
