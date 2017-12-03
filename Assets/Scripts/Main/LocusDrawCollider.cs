using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 軌跡を描くために発射するコライダのクラス
/// </summary>
public class LocusDrawCollider : MonoBehaviour
{
	/// <summary>
	/// Player
	/// </summary>
	[SerializeField]
	Player Player;
	/// <summary>
	/// 自身のコライダ
	/// </summary>
	SphereCollider col;

	void Start ()
	{
		col = GetComponent<SphereCollider>();
		var rect = new Rect(0, 0, 1, 1);

		col.OnCollisionEnterAsObservable().Where(x => Player != null)
			.Subscribe(_ => {
			Player.launchLocusDrawCol();
		})
		.AddTo(this);

		this.UpdateAsObservable().Where(x => !rect.Contains(Camera.main.WorldToViewportPoint(transform.position)) && Player != null)
			.Subscribe(_ => {
				Player.launchLocusDrawCol();
			})
			.AddTo(this);
	}
}
