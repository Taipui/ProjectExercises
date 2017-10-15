using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Atsumori : MonoBehaviour
{
	[SerializeField]
	Transform ImgTfm;

	void Start ()
	{
		ImgTfm.localScale = Vector3.zero;

		StartCoroutine("atsumoriLoop");
	}

	IEnumerator atsumoriLoop()
	{
		while (true) {
			yield return new WaitForSeconds(Random.Range(5.0f, 10.0f));

			ImgTfm.DOScale(
				Vector3.one / 2,
				0.2f
				).SetEase(Ease.InElastic);

			yield return new WaitForSeconds(3.0f);
			ImgTfm.localScale = Vector3.zero;
		}
	}
}

