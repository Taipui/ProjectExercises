using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// ボスに関するクラス
/// </summary>
public class Boss : Enemy
{
	/// <summary>
	/// GrayちゃんのモデルのGameObject(ダメージを受けた時に点滅させるため)
	/// </summary>
	[SerializeField]
	GameObject ModelGo;

	/// <summary>
	/// デフォルトの体力
	/// </summary>
	const int Default_Hp = 3;

	/// <summary>
	/// 移動速度
	/// </summary>
	const float Move_Speed = 0.05f;
	/// <summary>
	/// プレイヤーからどれだけ離れると移動を始めるかのしきい値
	/// </summary>
	const float Move_Threshold = 1.0f;

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		hp = Default_Hp;

		if (!!IsAlways) {
			permitLaunch();
		}
	}

	protected override void Start ()
	{
		base.Start();

		init();

		Assert.IsNotNull(PlayerTfm, "PlayerTfm is null");
		PlayerTfm.UpdateAsObservable().Where(x => !!canInput)
			.Subscribe(_ => {
				//move();
		})
		.AddTo(this);

		Observable.Interval(System.TimeSpan.FromSeconds(0.2f)).Where(x => !!isPlay() && !!enableLaunch)
			.Subscribe(_ => {
				if (Random.Range(0, 10) == 0) {
					launch();
				}
			})
		.AddTo(this);
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	protected override void launch()
	{
		Assert.IsNotNull(LauncherParent, "LauncherParent is null");
		foreach (Transform child in LauncherParent) {
			if (Random.Range(0, 2) == 0) {
				continue;
			}
			Assert.IsNotNull(PlayerTfm, "PlayerTfm is null");
			var childLauncher = child.gameObject.GetComponent<Launcher>();
			Assert.IsNotNull(childLauncher, "Child object are not attached to Launcher");
			Assert.IsNotNull(BulletParentTfm, "BulletParentTfm is null");
			//var worldPos = transform.TransformPoint(child.position);
			AI.ShootFixedAngle(new Vector3(child.position.x, child.position.y), PlayerTfm.position, 60.0f, childLauncher, Bullet, BulletParentTfm);
			//AI.ShootFixedAngle(new Vector3(worldPos.x, worldPos.y), PlayerTfm.position, 60.0f, childLauncher, Bullet, BulletParentTfm);
			base.launch();
		}
	}

	/// <summary>
	/// キャラクターを動かす
	/// </summary>
	void move()
	{
		Assert.IsNotNull(PlayerTfm, "PlayerTfm is null");
		var diff = PlayerTfm.position.y - transform.position.y;
		if (diff < -Move_Threshold) {
			transform.Translate(new Vector3(0.0f, -Move_Speed));
		} else if (diff > Move_Threshold) {
			transform.Translate(new Vector3(0.0f, Move_Speed));
		}
	}

	/// <summary>
	/// 点滅を始める
	/// </summary>
	protected override void startFlick()
	{
		flickCoroutine = StartCoroutine(flick(ModelGo));
	}

	/// <summary>
	/// 点滅を止める
	/// </summary>
	protected override void stopFlick()
	{
		StopCoroutine(flickCoroutine);
		ModelGo.SetActive(true);
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		base.dead();
		Main.clr();
	}
}
