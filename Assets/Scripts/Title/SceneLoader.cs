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

	IEnumerator Start ()
	{
		var async = SceneManager.LoadSceneAsync(Common.Main_Scene);
		async.allowSceneActivation = false;

		var loadTimer = 0.0f;

		while (async.progress < 0.9f) {
			LoadProgressSlider.value = async.progress;
			LoadProgressTxt.text = ((int)(async.progress * 100)).ToString() + '%';
			loadTimer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		LoadProgressSlider.value = 1.0f;
		LoadProgressTxt.text = "100%";
		Debug.Log("SceneLoadTime:" + loadTimer);

		//yield return new WaitForSeconds(1);

		async.allowSceneActivation = true;		
	}
}
