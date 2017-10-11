using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 敵に関するクラス
/// </summary>
public class Enemy : Character
{
	/// <summary>
	/// プレイヤーのTransform
	/// </summary>
	[SerializeField]
	Transform PlayerTfm;

	protected override void Start ()
	{
		base.Start();

		MyBulletTag = "EnemyBullet";

		Observable.Interval(System.TimeSpan.FromSeconds(1.0f)).Where(x => Gm.CurrentGameState == GameManager.GameState.Play)
			.Subscribe(_ => {
			if (Random.Range(0, 2) == 0) {
					onErased();
			}
		})
		.AddTo(this);
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	protected override void launch()
	{
		ShootFixedAngle(PlayerTfm.position, Random.Range(30.0f, 80.0f));
	}

	Vector3 ConvertVectorToVector3(float i_v0, float angle, Vector3 targetPos_)
	{
		var startPos = LaunchTfm.position;
		var targetPos = targetPos_;
		startPos.y = 0.0f;
		targetPos.y = 0.0f;

		var dir = (targetPos - startPos).normalized;
		var yawRot = Quaternion.FromToRotation(Vector3.right, dir);
		var vec = i_v0 * Vector3.right;

		vec = yawRot * Quaternion.AngleAxis(angle, Vector3.forward) * vec;

		return vec;
	}

	/// <summary>
	/// 発射する弾を作成
	/// </summary>
	/// <param name="i_shootVector"></param>
	void InstantiateShootObject(Vector3 i_shootVector)
	{
		if (Bullet == null) {
			throw new System.NullReferenceException("m_shootObject");
		}

		if (LaunchTfm == null) {
			throw new System.NullReferenceException("m_shootPoint");
		}

		var obj = Instantiate<GameObject>(Bullet, LaunchTfm.position, Quaternion.identity);
		var rb = obj.GetComponent<Rigidbody>();
		obj.transform.SetParent(BulletParentTfm);
		obj.tag = MyBulletTag;

		// 速さベクトルのままAddForce()を渡してはいけないぞ。力(速さ×重さ)に変換するんだ
		var force = i_shootVector * rb.mass;

		rb.AddForce(force, ForceMode.Impulse);
	}

	void ShootFixedAngle(Vector3 targetPos_, float angle)
	{
		var speedVec = ComputeVectorFromAngle(targetPos_, angle);
		if (speedVec <= 0.0f) {
			// その位置に着地させることは不可能
			Debug.LogWarning("!!");
			return;
		}

		var vec = ConvertVectorToVector3(speedVec, angle, targetPos_);
		InstantiateShootObject(vec);
	}

	float ComputeVectorFromAngle(Vector3 targetPos_, float angle)
	{
		// xz平面の距離を計算
		Vector2 startPos = LaunchTfm.position;
		Vector2 targetPos = PlayerTfm.position;
		var distance = Vector2.Distance(targetPos, startPos);

		var x = distance;
		var g = Physics2D.gravity.y;
		var y0 = PlayerTfm.position.y;
		var y = targetPos_.y;

		// Mathf.Cos()、Mathf.Tan()に渡す値の単位はラジアンなので変換
		var rad = angle * Mathf.Deg2Rad;

		var cos = Mathf.Cos(rad);
		var tan = Mathf.Tan(rad);

		var v0Square = g * x * x / (2 * cos * cos * (y - y0 - x * tan));

		// 負数を平方根計算すると虚数になってしまう。
		// 虚数はfloatでは表現できない。
		// こういう場合はこれ以上の計算は打ち切ろう。
		if (v0Square <= 0.0f) {
			return 0.0f;
		}

		var v0 = Mathf.Sqrt(v0Square);
		return v0;
	}

	/// <summary>
	/// 地面のチップが消されたら呼ばれる
	/// </summary>
	public override void onErased()
	{
		launch();
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		Destroy(gameObject);
	}

	void OnCollisionEnter(Collision collision)
	{
		chechBullet(collision.gameObject);
	}
}
