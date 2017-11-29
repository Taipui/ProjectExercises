using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// キャラクターすべてに共通するクラス
/// </summary>
public class Character : MonoBehaviour
{
	#region 弾関連
	/// <summary>
	/// 発射する弾
	/// </summary>
	[SerializeField]
	protected GameObject Bullet;
	/// <summary>
	/// 発射した弾をまとめるGameObjectのTransform
	/// </summary>
	[SerializeField]
	protected Transform BulletParentTfm;
	/// <summary>
	/// 自分が発射する弾に設定するタグのID
	/// </summary>
	protected int MyBulletLayer;
	#endregion

	/// <summary>
	/// 体力
	/// </summary>
	protected int hp;

	/// <summary>
	/// Main
	/// </summary>
	[SerializeField]
	protected Main Main;

	/// <summary>
	/// Launcherの親となるGameObjectのTransform
	/// </summary>
	[SerializeField]
	protected Transform LauncherParent;

	/// <summary>
	/// デカール
	/// </summary>
	[SerializeField]
	GameObject[] Decals;

	#region 無敵関連
	/// <summary>
	/// 無敵時間
	/// </summary>
	const float Invincible_Time = 1.0f;
	/// <summary>
	/// 無敵かどうか(弾の当たり判定が連続で来ないように)
	/// </summary>
	bool isInvinsible;
	/// <summary>
	/// 無敵時に点滅する間隔
	/// </summary>
	protected const float Flick_Interval = 0.1f;
	/// <summary>
	/// 無敵時の点滅のコルーチン
	/// </summary>
	protected Coroutine flickCoroutine;
	#endregion

	/// <summary>
	/// アニメーター
	/// </summary>
	protected Animator anim;

	/// <summary>
	/// 入力を許可するかどうか
	/// </summary>
	protected bool canInput;
	/// <summary>
	/// 入力を許可しない時間
	/// </summary>
	protected const float Disable_Input_Time = 0.4f;

	/// <summary>
	/// キャラクターのRidigbody
	/// </summary>
	protected Rigidbody rb;

	/// <summary>
	/// AudioSource
	/// </summary>
	protected AudioSource audioSource;

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		if (GetComponent<Animator>() != null) {
			anim = GetComponent<Animator>();
		}
		if (GetComponent<Rigidbody>() != null) {
			rb = GetComponent<Rigidbody>();
		}

		isInvinsible = false;

		canInput = true;

		hp = 1;

		audioSource = GetComponent<AudioSource>();

		if (Decals.Length <= 0) {
			return;
		}
		Assert.AreEqual(Decals.Length, 3, "The number of attached decals is not 3");
		for (var i = 0; i < Decals.Length; ++i) {
			for (var j = i + 1; j < Decals.Length; ++j) {
				Assert.AreNotEqual(Decals[i], Decals[j], "Decals[" + i + "] and Decals[" + j + "] are the same");
			}
		}
		for (var i = 0; i < Decals.Length; ++i) {
			Assert.IsNotNull(Decals[i], "Decals[" + i + "] is null");

			Decals[i].SetActive(false);
		}
	}

	protected virtual void Start ()
	{
		init();
	}

	/// <summary>
	/// ダメージ処理
	/// </summary>
	protected virtual IEnumerator dmg()
	{
		Main.playSE(Main.SE.Hit, null);

		if (--hp <= 0) {
			dead();
			yield break;
		}

		isInvinsible = true;

		if (anim != null) {
			StartCoroutine("disableInput");
			anim.SetTrigger("Damage");
			if (rb != null) {
				rb.velocity = Vector2.zero;
			}
		}

		startFlick();

		yield return new WaitForSeconds(Invincible_Time);

		isInvinsible = false;

		stopFlick();
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected virtual void dead()
	{
		canInput = false;

		if (anim == null) {
			return;
		}

		anim.SetTrigger("Dead");
	}

	/// <summary>
	/// Deadアニメーションで立ち上がろうとする瞬間に呼ばれる
	/// </summary>
	void OnStandUp()
	{
		anim.speed = 0.0f;
	}

	/// <summary>
	/// 当たったものが相手の弾かどうかを調べる
	/// </summary>
	/// <param name="go">当たったもの</param>
	IEnumerator chechBullet(GameObject go)
	{
		if (!!isInvinsible) {
			yield break;
		}

		if (go.tag != "Bullet") {
			yield break;
		}

		if (go.layer == MyBulletLayer) {
			yield break;
		}

		StartCoroutine("dmg");

		activeDecal(go.transform.localPosition.y);

		go.transform.SetParent(transform);

		Destroy(go);
	}

	/// <summary>
	/// 点滅を始める
	/// </summary>
	protected virtual void startFlick()
	{
		flickCoroutine = StartCoroutine(flick(gameObject));
	}

	/// <summary>
	/// キャラクターを点滅させる
	/// </summary>
	/// <returns></returns>
	protected IEnumerator flick(GameObject go)
	{
		while (true) {
			go.SetActive(!go.activeSelf);
			yield return new WaitForSeconds(Flick_Interval);
		}
	}

	/// <summary>
	/// 点滅を止める
	/// </summary>
	protected virtual void stopFlick()
	{
		StopCoroutine(flickCoroutine);
		gameObject.SetActive(true);
	}

	/// <summary>
	/// ゲームプレイ中かどうか
	/// </summary>
	/// <returns>ゲームプレイ中ならtrue</returns>
	protected bool isPlay()
	{
		if (Main == null) {
			return true;
		}
		return Main.CurrentGameState == Main.GameState.Play;
	}

	protected virtual void OnCollisionEnter(Collision col)
	{
		if (!isPlay()) {
			return;
		}
		if (!gameObject.activeSelf) {
			return;
		}
		StartCoroutine(chechBullet(col.gameObject));
	}

	/// <summary>
	/// デカールを表示する
	/// </summary>
	/// <param name="posY">当たった弾のY座標(ローカル)</param>
	void activeDecal(float posY)
	{
		if (Decals.Length <= 0) {
			return;
		}
		if (posY >= 1.0f) {
			Decals[0].SetActive(true);
		} else if (posY >= 0.7f) {
			Decals[1].SetActive(true);
		} else {
			Decals[2].SetActive(true);
		}
	}

	/// <summary>
	/// 一時的に入力を無効にする
	/// </summary>
	/// <returns></returns>
	protected IEnumerator disableInput()
	{
		canInput = false;
		yield return new WaitForSeconds(Disable_Input_Time);
		canInput = true;
	}
}
