using UnityEngine;

/// <summary>
/// 地面に当たった雪弾
/// </summary>
public class GroundBullet : MonoBehaviour
{
	/// <summary>
	/// 消えるまでの時間
	/// </summary>
	const float Destroy_Time = 180.0f;

	/// <summary>
	/// 消える時のエフェクト
	/// </summary>
	GameObject destroyEffectGo;

	/// <summary>
	/// 消える時のエフェクトをセット
	/// </summary>
	/// <param name="destroyEffectGo_">消える時のエフェクト</param>
	public void setDestroyEffectGo(GameObject destroyEffectGo_)
	{
		destroyEffectGo = destroyEffectGo_;
	}

	/// <summary>
	/// 一定時間後に消す
	/// </summary>
	public void destroy()
	{
		Destroy(gameObject, Destroy_Time);
	}

	void OnDestroy()
	{
		Instantiate(destroyEffectGo, transform.position, Quaternion.identity);
	}
}
