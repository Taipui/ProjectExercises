using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// Mainシーンを読み込む
/// </summary>
public class SceneLoader : MonoBehaviour
{
	/// <summary>
	/// 現在の項目のロードの進捗を表すスライダー
	/// </summary>
	[SerializeField]
	Slider CurrentLoadProgressSlider;
	/// <summary>
	/// 現在の項目のロードの進捗を表すテキスト
	/// </summary>
	[SerializeField]
	TextMeshProUGUI CurrentLoadProgressTxt;
	/// <summary>
	/// 全体のロードの進捗を表すスライダー
	/// </summary>
	[SerializeField]
	Slider TotalLoadProgressSlider;
	/// <summary>
	/// 全体のロードの進捗を表すテキスト
	/// </summary>
	[SerializeField]
	TextMeshProUGUI TotalLoadProgressTxt;
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
	/// スタッフロール用のBGMをロードするかどうか
	/// </summary>
	[SerializeField]
	bool IsLoadStaffRollBGM;

	/// <summary>
	/// フェードアウトに使用するPanelのImage
	/// </summary>
	[SerializeField]
	Image FadeImg;

	/// <summary>
	/// 現在の項目のロードの進捗を表す
	/// </summary>
	float tmpCurrentProgress;
	public float TmpCurrentProgress {
		get
		{
			return tmpCurrentProgress;
		}
		set
		{
			tmpCurrentProgress = value;
			CurrentLoadProgressSlider.value = tmpCurrentProgress;
			CurrentLoadProgressTxt.text = "<mspace=1.15em>" + tmpCurrentProgress.ToString("F1") + "</mspace>" + '%';
		}
	}
	/// <summary>
	/// 全体のロードの進捗を表す
	/// </summary>
	float tmpTotalProgress;
	public float TmpTotalProgress {
		get
		{
			return tmpTotalProgress;
		}
		set
		{
			tmpTotalProgress = value;
			TotalLoadProgressSlider.value = tmpTotalProgress;
			TotalLoadProgressTxt.text = "<mspace=1.15em>" + tmpTotalProgress.ToString("F1") + "</mspace>" + '%';
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
		Cursor.visible = true;
	}

	void Start()
	{
		init();

		StartCoroutine("load");
	}

	IEnumerator load()
	{
		if (!IsLoad) {
			yield break;
		}

		Tween currentProgressTween = null;
		Tween totalProgressTween = null;

		// Mainシーン用のBGMのロード
		var bgmLoaderGo = GameObject.Find("BGMLoader(Clone)");
		if (bgmLoaderGo == null) {
			yield break;
		}
		var bgmLoader = bgmLoaderGo.GetComponent<BGMLoader>();
		Assert.IsNotNull(bgmLoader, "bgmLoader is not attached \"BGMLoader\" GameObject");
		Assert.IsNotNull(GameManager.Instance.MainBGMs, "BGMLoader is not generated");
		while (GameManager.Instance.CurrentLoadBGMIndex < GameManager.Instance.MainBGMs.Length) {
			if (GameManager.Instance.PrevLoadBGMIndex >= GameManager.Instance.CurrentLoadBGMIndex) {
				yield return 0;
			}
			currentProgressTween.Kill();
			currentProgressTween = DOTween.To(
				() => TmpCurrentProgress,
				(x) => TmpCurrentProgress = x,
				(100 * (GameManager.Instance.CurrentLoadBGMIndex + 1)) / GameManager.Instance.MainBGMs.Length,
				Load_Progress_Anim_Speed
			).SetEase(Ease.Linear);

			totalProgressTween.Kill();
			totalProgressTween = DOTween.To(
				() => TmpTotalProgress,
				(x) => TmpTotalProgress = x,
				(100 * (GameManager.Instance.CurrentLoadBGMIndex + 1)) / (GameManager.Instance.MainBGMs.Length + GameManager.Instance.StaffRollBGMs.Length),
				Load_Progress_Anim_Speed
			).SetEase(Ease.Linear);

			LoadTxt.text = "Loading MainBGM(" + (GameManager.Instance.CurrentLoadBGMIndex + 1).ToString() + '/' + GameManager.Instance.MainBGMs.Length.ToString() + ")...";

			yield return 0;
		}

		if (!!IsLoadStaffRollBGM) {
			TmpCurrentProgress = 0;

			while (GameManager.Instance.CurrentLoadBGMIndex < GameManager.Instance.MainBGMs.Length + GameManager.Instance.StaffRollBGMs.Length) {
				if (GameManager.Instance.PrevLoadBGMIndex >= GameManager.Instance.CurrentLoadBGMIndex) {
					yield return 0;
				}
				currentProgressTween.Kill();
				currentProgressTween = DOTween.To(
					() => TmpCurrentProgress,
					(x) => TmpCurrentProgress = x,
					(100 * ((GameManager.Instance.CurrentLoadBGMIndex + 1) - GameManager.Instance.MainBGMs.Length)) / GameManager.Instance.StaffRollBGMs.Length,
					Load_Progress_Anim_Speed
				).SetEase(Ease.Linear);

				totalProgressTween.Kill();
				totalProgressTween = DOTween.To(
					() => TmpTotalProgress,
					(x) => TmpTotalProgress = x,
					(100 * (GameManager.Instance.CurrentLoadBGMIndex + 1)) / (GameManager.Instance.MainBGMs.Length + GameManager.Instance.StaffRollBGMs.Length),
					Load_Progress_Anim_Speed
				).SetEase(Ease.Linear);

				LoadTxt.text = "Loading StaffRollBGM(" + ((GameManager.Instance.CurrentLoadBGMIndex + 1) - GameManager.Instance.MainBGMs.Length).ToString() + '/' + GameManager.Instance.StaffRollBGMs.Length.ToString() + ")...";

				yield return 0;
			}
		}

		LoadDoneImgGo.SetActive(true);
		LoadTxt.text = "<align=center>Done!";
		currentProgressTween.Kill();
		TmpCurrentProgress = 100.0f;
		totalProgressTween.Kill();
		TmpTotalProgress = 100.0f;

		DOTween.ToAlpha(
			() => FadeImg.color,
			color => FadeImg.color = color,
			1.0f,
			1.0f
		).OnComplete(() => {
			bgmLoader.StopCoroutine("loadBGM");
			SceneManager.LoadScene(Common.Main_Scene);
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
