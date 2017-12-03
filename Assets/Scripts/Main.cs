﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

/// <summary>
/// ゲーム全般に関するクラス
/// </summary>
public class Main : MonoBehaviour
{
	/// <summary>
	/// ゲームオーバー時に表示するGameObject
	/// </summary>
	[SerializeField]
	GameObject GameOverGo;
	/// <summary>
	/// クリア時に表示するGameObject
	/// </summary>
	[SerializeField]
	GameObject ClrGo;

	/// <summary>
	/// ゲームの状態
	/// </summary>
	public enum GameState
	{
		/// <summary>
		/// ゲーム中
		/// </summary>
		Play,
		/// <summary>
		/// ゲームオーバー
		/// </summary>
		GameOver,
		/// <summary>
		/// クリア
		/// </summary>
		Clr
	}

	/// <summary>
	/// 現在のゲームの状態
	/// </summary>
	public GameState CurrentGameState { private set; get; }

	/// <summary>
	/// ゲームがスローになる速度(倍)
	/// </summary>
	const float Slow_Speed = 0.3f;

	#region フェード関連
	/// <summary>
	/// フェードのImage
	/// </summary>
	[SerializeField]
	Image FadeImg;
	/// <summary>
	/// フェードする時間(何秒でフェードするか)
	/// </summary>
	const float Fade_Time = 3.0f;
	#endregion

	#region BGM関連
	/// <summary>
	/// BGMのAudioSource1
	/// </summary>
	[SerializeField]
	AudioSource BGMAudioSource1;
	/// <summary>
	/// BGMのAudioSource2
	/// </summary>
	[SerializeField]
	AudioSource BGMAudioSource2;

	/// <summary>
	/// 現在のステージ
	/// </summary>
	int currentStage;
	/// <summary>
	/// 既に選ばれたBGMのIDを格納する配列
	/// </summary>
	List<int> selectedBGMIds;
	/// <summary>
	/// AudioMixer
	/// </summary>
	UnityEngine.Audio.AudioMixer audioMixer;

	/// <summary>
	/// ボス戦のBGM
	/// </summary>
	[SerializeField]
	AudioClip BossBGM;
	#endregion

	#region SE関連
	/// <summary>
	/// SEのAudioSource
	/// </summary>
	[SerializeField]
	AudioSource SEAudioSource;
	/// <summary>
	/// SE
	/// </summary>
	public enum SE
	{
		/// <summary>
		/// 雪弾がキャラクターに当たった時のSE
		/// </summary>
		Hit,
		/// <summary>
		/// アイテムを取得した時のSE
		/// </summary>
		Item,
		/// <summary>
		/// 変身した時のSE
		/// </summary>
		Transform,
		/// <summary>
		/// 敵を倒した時のSE
		/// </summary>
		Kill,
		/// <summary>
		/// 雪弾の発射時のSE
		/// </summary>
		Launch,
		/// <summary>
		/// ショットガンの発射時のSE
		/// </summary>
		ShotgunLaunch,
		/// <summary>
		/// 風を起こしている時のSE
		/// </summary>
		Wind,
		/// <summary>
		/// 瞬間移動した時のSE
		/// </summary>
		Teleportation,
		/// <summary>
		/// ページ送りのSE
		/// </summary>
		Feed,
	}
	/// <summary>
	/// SEの配列
	/// </summary>
	[SerializeField]
	AudioClip[] SEs;
	#endregion

	/// <summary>
	/// ゲームオーバーの処理
	/// </summary>
	public void gameOver()
	{
		CurrentGameState = GameState.GameOver;
		GameOverGo.SetActive(true);
		BGMAudioSource1.Stop();
		BGMAudioSource2.Stop();
	}

	/// <summary>
	/// クリアの処理
	/// </summary>
	public void clr()
	{
		CurrentGameState = GameState.Clr;
		Time.timeScale = Slow_Speed;
		var useBGM1 = BGMAudioSource1.clip == null;
		var mixerParam = "BGMVol";
		if (!!useBGM1) {
			mixerParam = mixerParam.Insert(3, "2");
		} else {
			mixerParam = mixerParam.Insert(3, "1");
		}
		var vol = 0.0f;
		DOTween.To(
			() => vol,
			val => vol = val,
			-80.0f,
			Fade_Time
		).OnUpdate(() => {
			audioMixer.SetFloat(mixerParam, vol);
		});
		DOTween.ToAlpha(
			() => FadeImg.color,
			color => FadeImg.color = color,
			1.0f,
			Fade_Time
		).SetUpdate(true)
		.OnComplete(() => {
			Time.timeScale = 1.0f;
			SceneManager.LoadScene(Common.Clr_Scene);
		});
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		Common.setCursor();
		CurrentGameState = GameState.Play;
		if (GameOverGo != null) {
			GameOverGo.SetActive(false);
		}
		if (ClrGo != null) {
			ClrGo.SetActive(false);
		}

		FadeImg.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

		audioMixer = BGMAudioSource1.outputAudioMixerGroup.audioMixer;
		currentStage = 1;
		selectedBGMIds = new List<int>();
		BGMAudioSource1.clip = GameManager.Instance.BGMClips_[chooseBGMID()];
		BGMAudioSource1.Play();

	}

	void Start ()
	{
		init();
	}

	/// <summary>
	/// SEを再生する
	/// </summary>
	/// <param name="se">SEの種類</param>
	public void playSE(SE se, AudioSource source)
	{
		if (source == null) {
			source = SEAudioSource;
		}
		source.PlayOneShot(SEs[(int)se]);
	}

	/// <summary>
	/// 次のステージへ
	/// </summary>
	/// <param name="stageNum">次のステージ</param>
	public void nextStage(int stageNum)
	{
		if (currentStage >= stageNum) {
			return;
		}
		++currentStage;
		if (currentStage < 5) {
			crossFade();
		} else {
			changeBossBGM();
		}
	}

	/// <summary>
	/// 以前選ばれたIDを回避してランダムでBGMのIDを選ぶ
	/// </summary>
	/// <returns></returns>
	int chooseBGMID()
	{
		var isDuplication = false;
		var r = 0;
		do {
			isDuplication = false;
			r = Random.Range(0, GameManager.Instance.BGMClips_.Length);
			for (var i = 0; i < selectedBGMIds.Count; ++i) {
				if (r == selectedBGMIds[i]) {
					isDuplication = true;
					break;
				}
			}
		} while (!!isDuplication);
		selectedBGMIds.Add(r);
		return r;
	}

	/// <summary>
	/// クロスフェードを行う
	/// </summary>
	void crossFade()
	{
		var fadeTime = 3.0f;
		var vol1 = 0.0f;
		var mixerParam1 = "BGMVol";
		var mixerParam2 = mixerParam1;
		var useBGM1 = BGMAudioSource1.clip == null;
		if (!!useBGM1) {
			mixerParam1 = mixerParam1.Insert(3, "2");
			mixerParam2 = mixerParam2.Insert(3, "1");
		} else {
			mixerParam1 = mixerParam1.Insert(3, "1");
			mixerParam2 = mixerParam2.Insert(3, "2");
		}
		var clip = GameManager.Instance.BGMClips_[chooseBGMID()];
		if (!!useBGM1) {
			BGMAudioSource1.clip = clip;
			BGMAudioSource1.Play();
		} else {
			BGMAudioSource2.clip = clip;
			BGMAudioSource2.Play();
		}
		audioMixer.SetFloat(mixerParam1, vol1);
		DOTween.To(
			() => vol1,
			val => vol1 = val,
			-80.0f,
			fadeTime
		).OnUpdate(() => {
			audioMixer.SetFloat(mixerParam1, vol1);
		});
		var vol2 = -80.0f;
		audioMixer.SetFloat(mixerParam2, vol2);
		DOTween.To(
			() => vol2,
			val => vol2 = val,
			0.0f,
			fadeTime
		).OnUpdate(() => {
			audioMixer.SetFloat(mixerParam2, vol2);
		})
		.OnComplete(() => {
			if (!!useBGM1) {
				BGMAudioSource2.clip = null;
			} else {
				BGMAudioSource1.clip = null;
			}
		});
	}

	void changeBossBGM()
	{
		var fadeTime = 1.0f;
		var vol1 = 0.0f;
		var mixerParam1 = "BGMVol";
		var mixerParam2 = mixerParam1;
		var useBGM1 = BGMAudioSource1.clip == null;
		if (!!useBGM1) {
			mixerParam1 = mixerParam1.Insert(3, "2");
			mixerParam2 = mixerParam2.Insert(3, "1");
		} else {
			mixerParam1 = mixerParam1.Insert(3, "1");
			mixerParam2 = mixerParam2.Insert(3, "2");
		}
		var clip = BossBGM;
		if (!!useBGM1) {
			BGMAudioSource1.clip = clip;
		} else {
			BGMAudioSource2.clip = clip;
		}
		audioMixer.SetFloat(mixerParam1, vol1);
		DOTween.To(
			() => vol1,
			val => vol1 = val,
			-80.0f,
			fadeTime
		).OnUpdate(() => {
			audioMixer.SetFloat(mixerParam1, vol1);
		}).OnComplete(() => {
			audioMixer.SetFloat(mixerParam2, 0.0f);
			if (!!useBGM1) {
				BGMAudioSource1.Play();
			} else {
				BGMAudioSource2.Play();
			}
		});
	}
}
