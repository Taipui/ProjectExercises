using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Mainシーンを読み込む
/// </summary>
public class SceneLoader : MonoBehaviour
{
	/// <summary>
	/// ロードの進捗を表すスライダー
	/// </summary>
	[SerializeField]
	Slider LoadProgressSlider;
	/// <summary>
	/// 現在のロード状況を表すテキスト
	/// </summary>
	[SerializeField]
	TextMeshProUGUI LoadProgressTxt;
	/// <summary>
	/// Loadingのテキスト
	/// </summary>
	[SerializeField]
	TextMeshProUGUI LoadTxt;
	/// <summary>
	/// ロード中を表す円のImage
	/// </summary>
	[SerializeField]
	Image LoadingCircleImg;
	/// <summary>
	/// ロードが完了した時に表示する画像のGameObject
	/// </summary>
	[SerializeField]
	GameObject LoadDoneImgGo;

	/// <summary>
	/// シーンをロードするかどうか
	/// </summary>
	[SerializeField]
	bool IsLoad;

	/// <summary>
	/// フェードアウトに使用するPanelのImage
	/// </summary>
	[SerializeField]
	Image FadeImg;

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		Cursor.visible = true;
		LoadDoneImgGo.SetActive(false);
		FadeImg.color = Color.clear;
	}

	IEnumerator Start ()
	{
		init();

		if (!IsLoad) {
			yield break;
		}
		var async = SceneManager.LoadSceneAsync(Common.Main_Scene);
		async.allowSceneActivation = false;

		var loadTimer = 0.0f;

		StartCoroutine("animLoadTxt");
		StartCoroutine("animLoadCircle");

		var loadTime = 30.0f;

		while (loadTimer < loadTime) {
			loadTimer += Time.deltaTime;
			var progress = (100 * loadTimer) / loadTime;
			LoadProgressSlider.value = progress;
			LoadProgressTxt.text = "<mspace=1.15em>" + progress.ToString("F1") + "%</mspace>";

			yield return new WaitForEndOfFrame();
		}
		StopCoroutine("animLoadTxt");
		LoadDoneImgGo.SetActive(true);
		LoadProgressSlider.value = 100.0f;
		LoadTxt.text = "<align=center>Done!";

		DOTween.ToAlpha(
			() => FadeImg.color,
			color => FadeImg.color = color,
			1.0f,
			1.0f
		).OnComplete(() => {
			async.allowSceneActivation = true;
		});
	}

	/// <summary>
	/// Loadingの文字をアニメーションする
	/// </summary>
	/// <returns></returns>
	IEnumerator animLoadTxt()
	{
		var cnt = 0;
		while (true) {
			switch (cnt++) {
				case 0:
					LoadTxt.text = "Loading.";
					break;
				case 1:
					LoadTxt.text = "Loading..";
					break;
				case 2:
					LoadTxt.text = "Loading...";
					break;
			}
			cnt %= 3;
			yield return new WaitForSeconds(1.0f);
		}
	}

	/// <summary>
	/// ロード中を表す円をアニメーションする
	/// </summary>
	/// <returns></returns>
	IEnumerator animLoadCircle()
	{
		var fillAmount = 0.0f;
		while (true) {
			fillAmount += Time.deltaTime;
			if (fillAmount >= 1.0f) {
				fillAmount = 0.0f;
			}
			LoadingCircleImg.fillAmount = fillAmount;
			yield return new WaitForEndOfFrame();
		}
	}
}
