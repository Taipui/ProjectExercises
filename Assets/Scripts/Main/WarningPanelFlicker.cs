using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// WARNING時に背景を赤くするクラス
/// </summary>
public class WarningPanelFlicker : MonoBehaviour
{
	/// <summary>
	/// Image
	/// </summary>
	Image img;

	/// <summary>
	/// 色が変わるまでの時間
	/// </summary>
	const float Flick_Time = 0.5f;

	void Start ()
	{
		img = GetComponent<Image>();

		img.color = new Color(img.color.r, img.color.g, img.color.b, 0);
	}

	/// <summary>
	/// 色を変え始める
	/// </summary>
	public void play()
	{
		var seq = DOTween.Sequence();

		seq.Append(
			DOTween.ToAlpha(
				() => img.color,
				col => img.color = col,
				0.5f,
				Flick_Time
			)
		);

		seq.Append(
				DOTween.ToAlpha(
				() => img.color,
				col => img.color = col,
				0f,
				Flick_Time
			).OnComplete(() => {
				Destroy(gameObject);
			})
		);

		seq.Play();
	}
}
