using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// Grayちゃんの出現ボイスを再生するクラス
/// </summary>
public class GrayChanAppearTrigger : MonoBehaviour
{
	/// <summary>
	/// Main
	/// </summary>
	[SerializeField]
	Main Main;

	[SerializeField]
	WarningBarManager Wbm;

	[SerializeField]
	WarningTextMover Wtm;

	[SerializeField]
	WarningPanelFlicker Wpf;

	void Start ()
	{
		var col = GetComponent<BoxCollider>();

		col.OnCollisionEnterAsObservable().Where(colGo => colGo.gameObject.tag == "Player")
			.Subscribe(_ => {
				Main.playAppearSE();
				Wbm.play();
				Wtm.play();
				Wpf.play();
				Destroy(gameObject);
			})
			.AddTo(this);
	}
}

