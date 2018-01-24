using UnityEngine;
using UnityEngine.Assertions;
using UniRx;
using UniRx.Triggers;
using System.Collections;

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

	#region SE関連

	/// <summary>
	/// SEのAudioSource
	/// </summary>
	AudioSource seAudioSource;

	/// <summary>
	/// 攻撃時に再生するSEの配列
	/// 11
	/// 7
	/// 8
	/// 9
	/// 10
	/// 11
	/// 12
	/// 13
	/// 14
	/// 19
	/// 20
	/// 36
	/// </summary>
	[SerializeField]
	AudioClip[] AtkSEs;
	/// <summary>
	/// 被弾時に再生するSEの配列
	/// 11
	/// 23
	/// 24
	/// 25
	/// 26
	/// 27
	/// 30
	/// 31
	/// 32
	/// 33
	/// 34
	/// 35
	/// </summary>
	[SerializeField]
	AudioClip[] DmgSEs;
	/// <summary>
	/// 敗北時に再生するSEの配列
	/// 3
	/// 29
	/// 50
	/// 62
	/// </summary>
	[SerializeField]
	AudioClip[] LoseSEs;

	#endregion

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		hp = Default_Hp;

		if (!!IsAlways) {
			permitLaunch();
		}

		seAudioSource = GetComponent<AudioSource>();
	}

	protected override void Start ()
	{
		base.Start();

		init();

		Assert.IsNotNull(PlayerTfm, "PlayerTfm is null");
		PlayerTfm.UpdateAsObservable().Where(x => !!canInput)
			.Subscribe(_ => {
				move();
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
			AI.ShootFixedAngle(child.position, PlayerTfm.position, 60.0f, childLauncher, Bullet, BulletParentTfm);
			base.launch();
			seAudioSource.PlayOneShot(AtkSEs[Random.Range(0, AtkSEs.Length)]);
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

	protected override IEnumerator dmg()
	{
		seAudioSource.PlayOneShot(DmgSEs[Random.Range(0, DmgSEs.Length)]);
		return base.dmg();
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		seAudioSource.PlayOneShot(LoseSEs[Random.Range(0, LoseSEs.Length)]);
		base.dead();
		Main.clr();
	}

	/// <summary>
	/// 発射の許可をする
	/// </summary>
	public new void permitLaunch()
	{
		Debug.Log("hoge");
		Main.playAppearSE();
		base.permitLaunch();
	}
}
