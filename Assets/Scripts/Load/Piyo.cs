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
	/// Piyoの種類をセット
	/// </summary>
	/// <param name="index">セットする値</param>
	public void setColIndex(int index)
	{
		ColIndex = index;
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		ColIndex = -1;
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
	/// 自身を消す
	/// </summary>
	/// <param name="particle">消す時に生成するパーティクルのPrefab</param>
	/// <param name="col">パーティクルの色</param>
	public void dead(GameObject particle, Color col)
	{
		Destroy(gameObject);
		var go = Instantiate(particle, new Vector3(transform.position.x, transform.position.y, -5.0f), Quaternion.identity);
		var goParticle = go.GetComponent<ParticleSystem>();
		var goCol = new ParticleSystem.MinMaxGradient();
		goCol.mode = ParticleSystemGradientMode.Color;
		goCol.color = col;
		var main = goParticle.main;
		main.startColor = goCol;
		Destroy(go, goParticle.main.duration * 2);
	}
}
