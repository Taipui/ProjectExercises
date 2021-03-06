﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;
using TMPro;

/// <summary>
/// オプションに関するクラス
/// </summary>
public class Option : MonoBehaviour
{
	/// <summary>
	/// タイトルのスクリプト
	/// </summary>
	[SerializeField]
	TitleBase Title;
	/// <summary>
	/// メインシーンのスクリプト
	/// </summary>
	[SerializeField]
	Main Main;
	/// <summary>
	/// スタッフロールのスクリプト
	/// </summary>
	[SerializeField]
	StaffRoll StaffRoll;

	/// <summary>
	/// 戻るボタン
	/// </summary>
	[SerializeField]
	Button BackBtn;

	/// <summary>
	/// スタッフロールボタンのGameObject(タイトルのみ)
	/// </summary>
	[SerializeField]
	GameObject StaffRollBtnGo;

	#region Master関連

	/// <summary>
	/// Masterの音量を調整するスライダー
	/// </summary>
	[SerializeField]
	Slider MasterSlider;
	/// <summary>
	/// Masterの現在の音量を表すテキスト
	/// </summary>
	[SerializeField]
	TextMeshProUGUI MasterValTxt;

	#endregion

	#region BGM関連

	/// <summary>
	/// BGMの音量を調整するスライダー
	/// </summary>
	[SerializeField]
	Slider BGMSlider;
	/// <summary>
	/// BGMの現在の音量を表すテキスト
	/// </summary>
	[SerializeField]
	TextMeshProUGUI BGMValTxt;

	#endregion

	#region SE関連

	/// <summary>
	/// SEの音量を調整するスライダー
	/// </summary>
	[SerializeField]
	Slider SESlider;
	/// <summary>
	/// SEの現在の音量を表すテキスト
	/// </summary>
	[SerializeField]
	TextMeshProUGUI SEValTxt;

	#endregion

	/// <summary>
	/// AudioMixer
	/// </summary>
	[SerializeField]
	UnityEngine.Audio.AudioMixer AudioMixer;

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		var val = PlayerPrefs.GetFloat("Master", 100);
		MasterSlider.value = val;
		MasterValTxt.text = val.ToString();

		val = PlayerPrefs.GetFloat("BGM", 100);
		BGMSlider.value = val;
		BGMValTxt.text = val.ToString();

		val = PlayerPrefs.GetFloat("SE", 100);
		SESlider.value = val;
		SEValTxt.text = val.ToString();
	}

	/// <summary>
	/// アサートチェック
	/// </summary>
	void checkAssert()
	{
		//Assert.IsFalse(Title == null && Main == null, "Both Title and Main, both null");
	}

	void Start ()
	{
		init();

		checkAssert();

		Button staffRollBtn = null;
		if (StaffRollBtnGo != null) {
			StaffRollBtnGo.SetActive(GameManager.Instance.IsClr);
			staffRollBtn = StaffRollBtnGo.GetComponent<Button>();
		}

		BackBtn.OnClickAsObservable().Subscribe(_ => {
			gameObject.SetActive(false);
			if (Title != null) {
				Title.endOption();
			} else if (Main != null) {
				Main.endOption();
			} else {
				StaffRoll.endOption();
			}
		})
		.AddTo(this);

		MasterSlider.OnValueChangedAsObservable().Subscribe(val => {
			PlayerPrefs.SetFloat("Master", val);
			MasterValTxt.text = ((int)val).ToString();
			AudioMixer.SetFloat("MasterVol", Mathf.Lerp(-80.0f, 0.0f, val / 100));
		})
		.AddTo(this);

		BGMSlider.OnValueChangedAsObservable().Subscribe(val => {
			PlayerPrefs.SetFloat("BGM", val);
			BGMValTxt.text = ((int)val).ToString();
			AudioMixer.SetFloat("BGMVol", Mathf.Lerp(-80.0f, 0.0f, val / 100));
		})
		.AddTo(this);

		SESlider.OnValueChangedAsObservable().Subscribe(val => {
			PlayerPrefs.SetFloat("SE", val);
			SEValTxt.text = ((int)val).ToString();
			AudioMixer.SetFloat("SEVol", Mathf.Lerp(-80.0f, 0.0f, val / 100));
		}).AddTo(this);

		if (StaffRollBtnGo != null) {
			staffRollBtn.OnMouseOverAsObservable().Subscribe(_ => {
				Debug.Log("mouse over");
			})
			.AddTo(this);
		}

		if (StaffRollBtnGo != null) {
			staffRollBtn.OnMouseExitAsObservable().Subscribe(_ => {
				Debug.Log("mouse exit");
			})
			.AddTo(this);
		}

		if (StaffRollBtnGo != null) {
			staffRollBtn.OnClickAsObservable().Subscribe(_ => {
				Time.timeScale = 1.0f;
				SceneManager.LoadScene(Common.StaffRoll_Scene);
			})
			.AddTo(this);
		}

		BackBtn.OnMouseOverAsObservable().Subscribe(_ => {
			Debug.Log("hoge");
		})
		.AddTo(this);
	}
}
