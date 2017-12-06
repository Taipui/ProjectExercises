using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 左に見切れたGameObjectを削除するクラス
/// </summary>
public class Eraser : MonoBehaviour
{
	void Start ()
	{
		var col = GetComponent<BoxCollider>();

		col.OnTriggerExitAsObservable().Where(colGo => !!isErase(colGo))
			.Subscribe(colGo => {
				Destroy(colGo.gameObject);
			})
			.AddTo(this);
	}

	/// <summary>
	/// 削除する対象かどうか
	/// </summary>
	/// <param name="col">当たったもの</param>
	/// <returns>削除対象ならtrue</returns>
	bool isErase(Collider col)
	{
		var isErase = col.tag == "Ground" || col.tag == "Signboard" || col.tag == "Obstacle" || col.tag == "Item";
		return isErase || (LayerMask.LayerToName(col.gameObject.layer) == "Item");
	}
}
