using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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

	IEnumerator Start ()
	{
		LoadDoneImgGo.SetActive(false);

		var async = SceneManager.LoadSceneAsync(Common.Main_Scene);
		async.allowSceneActivation = false;

		var loadTimer = 0.0f;

		StartCoroutine("animLoadTxt");
		StartCoroutine("animLoadCircle");

		LoadProgressTxt.text = "Initializing...";

		while (async.progress < 0.9f) {
			LoadProgressSlider.value = async.progress;
			LoadProgressTxt.text = ((int)(async.progress * 100)).ToString() + '%';
			Debug.Log(async.progress);
			Debug.Log((int)(async.progress * 100) + '%');
			loadTimer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		LoadProgressSlider.value = 1.0f;
		LoadProgressTxt.text = "100%";
		LoadDoneImgGo.SetActive(true);
		//Debug.Log("SceneLoadTime:" + loadTimer);

		//yield return new WaitForSeconds(1);

		async.allowSceneActivation = true;
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
