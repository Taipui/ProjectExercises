using System.Collections;
using UnityEngine;

/// <summary>
/// 敵全般に関するクラス
/// </summary>
public class Enemy : Character
{
	/// <summary>
	/// プレイヤーのTransform
	/// </summary>
	[SerializeField]
	protected Transform PlayerTfm;

	/// <summary>
	/// 発射可能かどうか
	/// </summary>
 	protected bool enableLaunch;
	/// <summary>
	/// 常に発射するか
	/// </summary>
	[SerializeField]
	protected bool IsAlways;

	/// <summary>
	/// デフォルトの体力
	/// </summary>
	const int Default_Hp = 1;

	/// <summary>
	/// CameraMover(カメラを揺らすため)
	/// </summary>
	CameraMover camMover;

	void init()
	{
		MyBulletLayer = Common.EnemyBulletLayer;

		enableLaunch = false;

		hp = Default_Hp;

		camMover = Camera.main.GetComponent<CameraMover>();
	}

	protected override void Start ()
	{
		base.Start();

		init();
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	protected virtual void launch()
	{
		if (Main == null) {
			return;
		}
		Main.playSE(Main.SE.Launch, audioSource);
	}

	/// <summary>
	/// ダメージ処理
	/// </summary>
	protected override IEnumerator dmg()
	{
		camMover.shake(0.5f);
		return base.dmg();
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		base.dead();
	}

	/// <summary>
	/// 発射の許可をする
	/// </summary>
	public void permitLaunch()
	{
		//return;
		enableLaunch = true;
	}
}
