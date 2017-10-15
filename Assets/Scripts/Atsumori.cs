using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 熱盛と出てしまうスクリプト
/// </summary>
public class Atsumori : MonoBehaviour
{
	/// <summary>
	/// 熱盛の画像のTransform
	/// </summary>
	[SerializeField]
	Transform ImgTfm;
	/// <summary>
	/// 熱盛の音声を再生するコンポーネント
	/// </summary>
	[SerializeField]
	AudioSource AudioSource;

	void Start ()
	{
		ImgTfm.localScale = Vector3.zero;

		StartCoroutine("atsumoriLoop");
	}

	/// <summary>
	/// 不定期に熱盛を出す
	/// </summary>
	/// <returns></returns>
	IEnumerator atsumoriLoop()
	{
		while (true) {
			yield return new WaitForSeconds(Random.Range(5.0f, 10.0f));

			AudioSource.Play();

			ImgTfm.DOScale(
				Vector3.one / 2,
				0.2f
				).SetEase(Ease.InElastic);

			yield return new WaitForSeconds(2.5f);
			ImgTfm.localScale = Vector3.zero;
		}
	}
}
