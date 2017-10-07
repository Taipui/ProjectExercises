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

		Observable.Interval(System.TimeSpan.FromSeconds(3.0f)).Subscribe(_ => {
			launch();
		})
		.AddTo(this);
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	void launch()
	{
//		var obj = Instantiate(Bullet, LaunchTfm.position, Quaternion.identity);
		//		var direction = new Vector3(Random.Range(-8.0f, 8.0f), Random.Range(-5.0f, 5.0f), -10.0f) - LaunchTfm.position;
		//		var direction = new Vector3(-0.1f, 2.3f, -10.0f) - transform.position;
		//		obj.GetComponent<Rigidbody2D>().velocity = direction.normalized * 40.0f;
		ShootFixedAngle(PlayerTfm.position, 60.0f);
		Pc.eraseGroundChip();
	}

	private void ShootFixedSpeedInPlaneDirection(Vector3 i_targetPosition, float i_speed)
	{
		if (i_speed <= 0.0f) {
			// その位置に着地させることは不可能のようだ！
			Debug.LogWarning("!!");
			return;
		}

		// xz平面の距離を計算。
		Vector2 startPos = LaunchTfm.position;
		Vector2 targetPos = PlayerTfm.position;
		float distance = Vector2.Distance(targetPos, startPos);

		float time = distance / i_speed;

		ShootFixedTime(i_targetPosition, time);
	}

	private void ShootFixedTime(Vector3 i_targetPosition, float i_time)
	{
		float speedVec = ComputeVectorFromTime(i_targetPosition, i_time);
		float angle = ComputeAngleFromTime(i_targetPosition, i_time);

		if (speedVec <= 0.0f) {
			// その位置に着地させることは不可能のようだ！
			Debug.LogWarning("!!");
			return;
		}

		Vector3 vec = ConvertVectorToVector3(speedVec, angle, i_targetPosition);
		InstantiateShootObject(vec);
	}

	private float ComputeVectorFromTime(Vector3 i_targetPosition, float i_time)
	{
		Vector2 vec = ComputeVectorXYFromTime(i_targetPosition, i_time);

		float v_x = vec.x;
		float v_y = vec.y;

		float v0Square = v_x * v_x + v_y * v_y;
		// 負数を平方根計算すると虚数になってしまう。
		// 虚数はfloatでは表現できない。
		// こういう場合はこれ以上の計算は打ち切ろう。
		if (v0Square <= 0.0f) {
			return 0.0f;
		}

		float v0 = Mathf.Sqrt(v0Square);

		return v0;
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

	private float ComputeAngleFromTime(Vector3 i_targetPosition, float i_time)
	{
		Vector2 vec = ComputeVectorXYFromTime(i_targetPosition, i_time);

		float v_x = vec.x;
		float v_y = vec.y;

		float rad = Mathf.Atan2(v_y, v_x);
		float angle = rad * Mathf.Rad2Deg;

		return angle;
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
}

