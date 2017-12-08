using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Assertions;
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
	/// ロードの進捗を表す
	/// </summary>
	float tmpProgress;
	public float TmpProgress {
		get
		{
			return tmpProgress;
		}
		set
		{
			tmpProgress = value;
			LoadProgressSlider.value = tmpProgress;
			LoadProgressTxt.text = "<mspace=1.15em>" + tmpProgress.ToString("F1") + "</mspace>" + '%';
		}
	}
	/// <summary>
	/// ロードの進捗の%のアニメーションの速度
	/// </summary>
	const float Load_Progress_Anim_Speed = 1.5f;

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		LoadDoneImgGo.SetActive(false);
		FadeImg.color = Color.clear;
	}

	IEnumerator Start()
	{
		init();

		if (!IsLoad) {
			yield break;
		}

		//var cnt = 0;
		//if (GameManager.Instance.BGMs_ == null) {
		//	// BGMのロード
		//	GameManager.Instance.BGMs_ = new AudioClip[20];
		//	while (true) {
		//		var resReq = Resources.LoadAsync<AudioClip>("BGM/bgm" + (cnt + 1).ToString());

		//		LoadTxt.text = "Loading BGM(" + (cnt + 1).ToString() + '/' + GameManager.Instance.BGMs_.Length.ToString() + ")...";
		//		DOTween.To(
		//			() => TmpProgress,
		//			(x) => TmpProgress = x,
		//			//(100 * (cnt + 1)) / (GameManager.Instance.BGMs_.Length + 1),
		//			(100 * (cnt + 1)) / GameManager.Instance.BGMs_.Length,
		//			Load_Progress_Anim_Speed
		//		).SetEase(Ease.Linear);

		//		while (!resReq.isDone) {
		//			yield return 0;
		//		}

		//		GameManager.Instance.BGMs_[cnt] = resReq.asset as AudioClip;

		//		if (++cnt >= GameManager.Instance.BGMs_.Length) {
		//			break;
		//		}
		//	}
		//} else {
		//	TmpProgress = 100.0f;
		//	cnt = 20;
		//}

		Tween progressTween = null;

		var bgmLoaderGo = GameObject.Find("BGMLoader");
		if (bgmLoaderGo == null) {
			yield break;
		}
		var bgmLoader = bgmLoaderGo.GetComponent<BGMLoader>();
		Assert.IsNotNull(bgmLoader, "bgmLoader is not attached \"BGMLoader\" GameObject");
		Assert.IsNotNull(GameManager.Instance.BGMs_, "BGMLoader is not generated");
		//while (bgmLoader.CurrentLoadIndex < GameManager.Instance.BGMs_.Length) {
		//	if (bgmLoader.PrevLoadIndex >= bgmLoader.CurrentLoadIndex) {
		//		yield return 0;
		//	}
		//	progressTween = DOTween.To(
		//		() => TmpProgress,
		//		(x) => TmpProgress = x,
		//		//(100 * (cnt + 1)) / (GameManager.Instance.BGMs_.Length + 1),
		//		(100 * (bgmLoader.CurrentLoadIndex + 1)) / GameManager.Instance.BGMs_.Length,
		//		Load_Progress_Anim_Speed
		//	).SetEase(Ease.Linear);
		//	LoadTxt.text = "Loading BGM(" + (bgmLoader.CurrentLoadIndex + 1).ToString() + '/' + GameManager.Instance.BGMs_.Length.ToString() + ")...";

		//	yield return 0;
		//}

		// シーンのロード
		yield return new WaitForEndOfFrame();
		var async = SceneManager.LoadSceneAsync(Common.Main_Scene);
		async.allowSceneActivation = false;

		//LoadTxt.text = "Loading scene...";
		LoadDoneImgGo.SetActive(true);
		LoadTxt.text = "<align=center>Done!";
		progressTween.Kill();
		TmpProgress = 100.0f;

		while (async.progress < 0.9f) {
			yield return 0;
		}

		DOTween.ToAlpha(
			() => FadeImg.color,
			color => FadeImg.color = color,
			1.0f,
			1.0f
		).OnComplete(() => {
			async.allowSceneActivation = true;
		});

		//DOTween.To(
		//	() => TmpProgress,
		//	(x) => TmpProgress = x,
		//	(100 * (cnt + 1)) / (GameManager.Instance.BGMs_.Length + 1),
		//	Load_Progress_Anim_Speed
		//	).SetEase(Ease.Linear)
		//	.OnComplete(() => {
		//		LoadTxt.text = "<align=center>Done!";

		//		DOTween.ToAlpha(
		//			() => FadeImg.color,
		//			color => FadeImg.color = color,
		//			1.0f,
		//			1.0f
		//		).OnComplete(() => {
		//			async.allowSceneActivation = true;
		//		});
		//	});
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
