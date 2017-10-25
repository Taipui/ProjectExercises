using System.Collections;
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
	readonly ReactiveProperty<int> hp = new ReactiveProperty<int>(1);

	/// <summary>
	/// GameManager
	/// </summary>
	[SerializeField]
	protected GameManager Gm;

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
	const float Invincible_Time = 0.1f;
	/// <summary>
	/// 無敵かどうか(弾の当たり判定が連続で来ないように)
	/// </summary>
	bool isInvinsible;

	/// <summary>
	/// 体力をセット
	/// </summary>
	/// <param name="val">セットする体力</param>
	protected void setHp(int val)
	{
		hp.Value = val;
	}

	protected virtual void Start ()
	{
		isInvinsible = false;
		//foreach (var obj in Decals) {
		//	obj.SetActive(false);
		//}
		for (var i = 0; i < Decals.Length; ++i) {
			Decals[i].SetActive(false);
		}


		hp.AsObservable().Where(val => val <= 0)
			.Subscribe(_ => {
				dead();
			})
			.AddTo(this);
	}

	/// <summary>
	/// ダメージ処理
	/// </summary>
	void damage()
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
		yield return new WaitForSeconds(Invincible_Time);
		isInvinsible = false;
	}

	/// <summary>
	/// ゲームプレイ中かどうか
	/// </summary>
	/// <returns>ゲームプレイ中ならtrue</returns>
	protected bool isPlay()
	{
		if (Gm == null) {
			return true;
		}
		return Gm.CurrentGameState == GameManager.GameState.Play;
	}

	void OnCollisionEnter(Collision col)
	{
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
}
