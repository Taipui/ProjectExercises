﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

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
	/// 自分が発射する弾に設定するタグの名前
	/// </summary>
	public int MyBulletLayer { protected set; get; }
	#endregion

	/// <summary>
	/// 体力
	/// </summary>
	readonly protected ReactiveProperty<int> hp = new ReactiveProperty<int>(1);

	/// <summary>
	/// Main
	/// </summary>
	[SerializeField]
	protected Main Main;

	/// <summary>
	/// Launcherの親となるオブジェクト
	/// </summary>
	[SerializeField]
	protected Transform LauncherParent;

	/// <summary>
	/// デカール
	/// </summary>
	[SerializeField]
	GameObject[] Decals;

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
	/// 体力をセット
	/// </summary>
	/// <param name="val">セットする体力</param>
	protected void setHp(int val)
	{
		hp.Value = val;
	}

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

		for (var i = 0; i < Decals.Length; ++i) {
			Decals[i].SetActive(false);
		}

		canInput = true;

	}

	protected virtual void Start ()
	{
		init();

		hp.AsObservable().Where(val => val <= 0)
			.Subscribe(_ => {
				dead();
			})
			.AddTo(this);
	}

	/// <summary>
	/// ダメージ処理
	/// </summary>
	protected virtual void damage()
	{
		--hp.Value;
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected virtual void dead()
	{
		// 派生クラスで実装
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

		damage();
		go.transform.SetParent(transform);
		activeDecal(go.transform.localPosition.y);
		Destroy(go);
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
	/// キャラクターの点滅を始める
	/// </summary>
	protected virtual void startFlick()
	{
		StartCoroutine("flick");
	}

	/// <summary>
	/// キャラクターを点滅させる
	/// </summary>
	/// <returns></returns>
	protected virtual IEnumerator flick()
	{
		while (true) {
			gameObject.SetActive(!gameObject.activeSelf);
			yield return new WaitForSeconds(Flick_Interval);
		}
	}

	/// <summary>
	/// 点滅を止める
	/// </summary>
	protected virtual void stopFlick()
	{
		StopCoroutine("flick");
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
		if (Decals.Length < 3) {
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
