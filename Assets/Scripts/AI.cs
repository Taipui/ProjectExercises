using UnityEngine;

/// <summary>
/// 敵の自律行動に関するクラス
/// </summary>
public static class AI
{
	public static void ShootFixedAngle(Vector3 launchPos, Vector3 targetPos, float angle, Launcher launcher, GameObject bullet, Transform bulletParent)
	{
		var speedVec = ComputeVectorFromAngle(launchPos, targetPos, angle);
		if (speedVec <= 0.0f) {
			// その位置に着地させることは不可能
			Debug.LogWarning("!!");
			return;
		}

		var vec = ConvertVectorToVector3(launchPos, targetPos, angle, speedVec);
		launcher.createLaunch(bullet, targetPos, Common.EnemyBulletLayer, bulletParent, vec);
	}

	public static float ComputeVectorFromAngle(Vector3 launchPos, Vector3 targetPos, float angle)
	{
		var distance = Vector2.Distance(targetPos, launchPos);

		var x = distance;
		var g = Physics.gravity.y;
		var y0 = launchPos.y;
		var y = targetPos.y;

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

	public static Vector3 ConvertVectorToVector3(Vector3 launchPos, Vector3 targetPos, float angle, float i_v0)
	{
		launchPos.y = 0.0f;
		targetPos.y = 0.0f;

		var dir = (targetPos - launchPos).normalized;
		var yawRot = Quaternion.FromToRotation(Vector3.right, dir);
		var vec = i_v0 * Vector3.right;

		vec = yawRot * Quaternion.AngleAxis(angle, Vector3.forward) * vec;

		return vec;
	}
}
