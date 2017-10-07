using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Enemy : Character
{
	/// <summary>
	/// PlayerColliderへの参照
	/// </summary>
	[SerializeField]
	PlayerCollider Pc;

	/// <summary>
	/// プレイヤーのTransform
	/// </summary>
	[SerializeField]
	Transform PlayerTfm;

	protected override void Start ()
	{
		base.Start();

		Observable.Interval(System.TimeSpan.FromSeconds(1.0f)).Subscribe(_ => {
			//			launch();
			if (Random.Range(0, 2) == 0) {
				Gce.checkGroundChip();
			}
		})
		.AddTo(this);
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	protected override void launch()
	{
		//if (Random.Range(0, 2) == 0) {
		//	return;
		//}
		ShootFixedAngle(PlayerTfm.position, Random.Range(30.0f, 80.0f));
//		eraseGroundChip();
	}

	private Vector2 ComputeVectorXYFromTime(Vector3 i_targetPosition, float i_time)
	{
		// 瞬間移動はちょっと……。
		if (i_time <= 0.0f) {
			return Vector2.zero;
		}


		// xz平面の距離を計算。
		Vector2 startPos = LaunchTfm.position;
		Vector2 targetPos = i_targetPosition;
		float distance = Vector2.Distance(targetPos, startPos);

		float x = distance;
		// な、なぜ重力を反転せねばならないのだ...
		float g = -Physics2D.gravity.y;
		float y0 = LaunchTfm.position.y;
		float y = i_targetPosition.y;
		float t = i_time;

		float v_x = x / t;
		float v_y = (y - y0) / t + (g * t) / 2;

		return new Vector2(v_x, v_y);
	}

	private Vector3 ConvertVectorToVector3(float i_v0, float i_angle, Vector3 i_targetPosition)
	{
		Vector3 startPos = LaunchTfm.position;
		Vector3 targetPos = i_targetPosition;
		startPos.y = 0.0f;
		targetPos.y = 0.0f;

		Vector3 dir = (targetPos - startPos).normalized;
		Quaternion yawRot = Quaternion.FromToRotation(Vector3.right, dir);
		Vector3 vec = i_v0 * Vector3.right;

		vec = yawRot * Quaternion.AngleAxis(i_angle, Vector3.forward) * vec;

		return vec;
	}

	private void InstantiateShootObject(Vector3 i_shootVector)
	{
		if (Bullet == null) {
			throw new System.NullReferenceException("m_shootObject");
		}

		if (LaunchTfm == null) {
			throw new System.NullReferenceException("m_shootPoint");
		}

		var obj = Instantiate<GameObject>(Bullet, LaunchTfm.position, Quaternion.identity);
		var rb = obj.GetComponent<Rigidbody2D>();
//		var rigidbody = obj.AddComponent<Rigidbody>();

		// 速さベクトルのままAddForce()を渡してはいけないぞ。力(速さ×重さ)に変換するんだ
		Vector3 force = i_shootVector * rb.mass;

		rb.AddForce(force, ForceMode2D.Impulse);
	}

	private void ShootFixedAngle(Vector3 i_targetPosition, float i_angle)
	{
		float speedVec = ComputeVectorFromAngle(i_targetPosition, i_angle);
		if (speedVec <= 0.0f) {
			// その位置に着地させることは不可能のようだ！
			Debug.LogWarning("!!");
			return;
		}

		Vector3 vec = ConvertVectorToVector3(speedVec, i_angle, i_targetPosition);
		InstantiateShootObject(vec);
	}

	private float ComputeVectorFromAngle(Vector3 i_targetPosition, float i_angle)
	{
		// xz平面の距離を計算。
		Vector2 startPos = LaunchTfm.position;
		Vector2 targetPos = PlayerTfm.position;
		float distance = Vector2.Distance(targetPos, startPos);

		float x = distance;
		float g = Physics2D.gravity.y;
		float y0 = PlayerTfm.position.y;
		float y = i_targetPosition.y;

		// Mathf.Cos()、Mathf.Tan()に渡す値の単位はラジアンだ。角度のまま渡してはいけないぞ！
		float rad = i_angle * Mathf.Deg2Rad;

		float cos = Mathf.Cos(rad);
		float tan = Mathf.Tan(rad);

		float v0Square = g * x * x / (2 * cos * cos * (y - y0 - x * tan));

		// 負数を平方根計算すると虚数になってしまう。
		// 虚数はfloatでは表現できない。
		// こういう場合はこれ以上の計算は打ち切ろう。
		if (v0Square <= 0.0f) {
			return 0.0f;
		}

		float v0 = Mathf.Sqrt(v0Square);
		return v0;
	}

	public override void onErased()
	{
		launch();
	}
}
