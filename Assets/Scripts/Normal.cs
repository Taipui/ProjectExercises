using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// ザコ敵に関するクラス
/// </summary>
public class Normal : Enemy
{
	#region 発射関連
	/// <summary>
	/// ランダムな間隔で発射するかどうか
	/// </summary>
	[SerializeField]
	bool IsRandomInterval;
	/// <summary>
	/// 撃つ頻度(ランダムの時)(増やすほど撃たなくなる)
	/// </summary>
	[SerializeField]
	int Frequency;
	/// <summary>
	/// 発射頻度(ランダムでない時)
	/// </summary>
	[SerializeField]
	float LaunchInterval;
	/// <summary>
	/// ある位置にいる時だけ発射するようにするかどうか
	/// </summary>
	[SerializeField]
	bool IsPointLaunch;
	/// <summary>
	/// 発射する位置(IsPointLaunchがtrueの時のみ)(0がデフォルトの位置、1～がMovePointsのインデックス(-1))
	/// </summary>
	[SerializeField]
	int LaunchPosIndex;
	#endregion

	#region アイテム関連
	/// <summary>
	/// ドロップするアイテムのGameObject
	/// </summary>
	[SerializeField]
	GameObject ItemGo;
	/// <summary>
	/// アイテムをドロップする力
	/// </summary>
	const float Item_Launch_Power = 2.0f;
	/// <summary>
	/// アイテムをドロップする角度の範囲
	/// </summary>
	const float Item_Launch_Angle_Range = 45.0f;
	#endregion

	/// <summary>
	/// 死亡時に生成するパーティクルのGameObject
	/// </summary>
	[SerializeField]
	GameObject Particle;

	#region 移動関連
	/// <summary>
	/// 移動点の配列
	/// </summary>
	[SerializeField]
	Transform[] MovePoints;
	/// <summary>
	/// 初期位置
	/// </summary>
	Vector3 defaultPoint;
	/// <summary>
	/// 現在の地点
	/// </summary>
	int currentPoint;
	/// <summary>
	/// 目標地点まで移動する速度
	/// </summary>
	const float Move_Time = 1.0f;
	/// <summary>
	/// 目標地点まで到達後、再び移動するまで待つ時間
	/// </summary>
	const float Wait_Time = 1.0f;
	#endregion

	/// <summary>
	/// Title
	/// </summary>
	[SerializeField]
	TitleBase Title;

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		if (!!IsAlways) {
			permitLaunch();
		}

		if (MovePoints != null && MovePoints.Length > 0) {
			defaultPoint = transform.position;
			currentPoint = 0;

			StartCoroutine("movePoint");
		}
	}

	protected override void Start ()
	{
		base.Start();

		init();

		Observable.Interval(System.TimeSpan.FromSeconds(0.2f)).Where(x => !!isPlay() && !!IsRandomInterval && !IsPointLaunch && !!enableLaunch)
			.Subscribe(_ => {
				if (Random.Range(0, Frequency) == 0) {
					launch();
				}
			})
		.AddTo(this);

		Observable.Interval(System.TimeSpan.FromSeconds(LaunchInterval)).Where(x => !!isPlay() && !IsRandomInterval && !IsPointLaunch && !!enableLaunch)
			.Subscribe(_ => {
				launch();
			})
		.AddTo(this);
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	protected override void launch()
	{
		AI.ShootFixedAngle(transform.position, PlayerTfm.position, 60.0f, GetComponent<Launcher>(), Bullet, BulletParentTfm);
		if (Title != null) {
			Title.playSE(TitleBase.SE.Launch);
		}
		base.launch();
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		Destroy(gameObject);

		var go = Instantiate(Particle);
		go.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
		Destroy(go, go.GetComponent<ParticleSystem>().main.duration);

		Main.playSE(Main.SE.Kill, null);

		if (ItemGo == null) {
			return;
		}
		var r = Random.Range(0, 4);
		//r = 5;
		if (r <= 0) {
			return;
		}
		go = Instantiate(ItemGo, transform.position, Quaternion.identity);
		var vec = Vector3.up * Item_Launch_Power;
		vec = Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.Range(-Item_Launch_Angle_Range, Item_Launch_Angle_Range))) * vec;
		go.GetComponent<Rigidbody>().AddForce(vec, ForceMode.Impulse);
		go.tag = "Item" + r.ToString();
		go.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.ItemSprites_[r - 1];

		if (Particle == null) {
			return;
		}
	}

	/// <summary>
	/// 移動
	/// </summary>
	/// <returns></returns>
	IEnumerator movePoint()
	{
		while (true) {
			currentPoint = (currentPoint + 1) % (MovePoints.Length + 1);
			var dest = currentPoint == 0 ? defaultPoint : MovePoints[currentPoint - 1].position;
			transform.DOMove(
				dest,
				1.0f
			);
			yield return new WaitForSeconds(Move_Time);
			if (!!IsPointLaunch && currentPoint == LaunchPosIndex) {
				launch();
			} else {
				yield return new WaitForSeconds(Wait_Time);
			}
		}
	}
}
