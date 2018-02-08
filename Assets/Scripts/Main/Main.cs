using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using TMPro;

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
	/// ロード画面
	/// </summary>
	[SerializeField]
	GameObject LoadGo;

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
	/// 現在流れているBGMの名前を表示するテキスト
	/// </summary>
	[SerializeField]
	TextMeshProUGUI NowPlayingBGMTxt;

	/// <summary>
	/// 現在のステージ
	/// </summary>
	int currentStage;
	/// <summary>
	/// 既に選ばれたBGMのIDを格納するリスト
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
		/// <summary>
		/// 風を置けない場所に風を置こうとした時のSE(1030)
		/// </summary>
		Ban1,
		/// <summary>
		/// 風を置けない場所に風を置こうとした時のSE(1031)
		/// </summary>
		Ban2,
		/// <summary>
		/// 風を置けない場所に風を置こうとした時のSE(1181)
		/// </summary>
		Ban3,
	}
	/// <summary>
	/// SEの配列
	/// </summary>
	[SerializeField]
	AudioClip[] SEs;

	/// <summary>
	/// Grayちゃんの登場時に再生するSEの配列
	/// 2
	/// 51
	/// 52
	/// </summary>
	[SerializeField]
	AudioClip[] GrayChanAppearSEs;
	/// <summary>
	/// Grayちゃんの敗北時に再生するSEの配列
	/// 3
	/// 29
	/// 50
	/// 62
	/// </summary>
	[SerializeField]
	AudioClip[] GrayChanLoseSEs;

	#endregion

	/// <summary>
	/// 風が置けないことを表す画像のGameObject
	/// </summary>
	[SerializeField]
	GameObject BanImgGo;

	#region オプション関連

	/// <summary>
	/// オプション画面
	/// </summary>
	[SerializeField]
	GameObject OptionCanvasGo;
	/// <summary>
	/// オプションを開く前のtimeScale;
	/// </summary>
	float prevTimeScale;

	#endregion

	#region チート関連

	/// <summary>
	/// プレイヤーのスクリプト
	/// </summary>
	[SerializeField]
	PlayerAct PlayerAct;
	/// <summary>
	/// HPを表すGameObject
	/// </summary>
	[SerializeField]
	GameObject HPGo;
	/// <summary>
	/// 無敵を表すGameObject
	/// </summary>
	[SerializeField]
	GameObject InvincibleTxtGo;

	#endregion

	/// <summary>
	/// プレゼンモードかどうか
	/// </summary>
	[SerializeField]
	bool PresentationMode;

	/// <summary>
	/// ゲームオーバーの処理
	/// </summary>
	public void gameOver()
	{
		CurrentGameState = GameState.GameOver;
		GameOverGo.SetActive(true);
		BGMAudioSource1.Stop();
		BGMAudioSource2.Stop();
		Cursor.visible = false;
	}

	/// <summary>
	/// クリアの処理
	/// </summary>
	public void clr()
	{
		CurrentGameState = GameState.Clr;
		Time.timeScale = Slow_Speed;
		var useBGM1 = BGMAudioSource1.clip != null;
		AudioSource bgmAudioSource = null;
		bgmAudioSource = !!useBGM1 ? BGMAudioSource1 : BGMAudioSource2;
		DOTween.To(
			() => bgmAudioSource.volume,
			val => bgmAudioSource.volume = val,
			0,
			Fade_Time
		).SetUpdate(true);
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

		playLoseSE();
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		CurrentGameState = GameState.Play;
		GameOverGo.SetActive(false);
		LoadGo.SetActive(false);
		FadeImg.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

		audioMixer = BGMAudioSource1.outputAudioMixerGroup.audioMixer;
		currentStage = 1;

		HPGo.SetActive(true);
		InvincibleTxtGo.SetActive(false);

		BanImgGo.SetActive(false);

		OptionCanvasGo.SetActive(false);

		audioMixer.SetFloat("MasterVol", Mathf.Lerp(-80.0f, 0.0f, PlayerPrefs.GetFloat("Master", 100) / 100));
		audioMixer.SetFloat("BGMVol", Mathf.Lerp(-80.0f, 0.0f, PlayerPrefs.GetFloat("BGM", 100) / 100));
		BGMAudioSource2.volume = 0;
		audioMixer.SetFloat("SEVol", Mathf.Lerp(-80.0f, 0.0f, PlayerPrefs.GetFloat("SE", 100) / 100));

		NowPlayingBGMTxt.text = "";

		if (GameManager.Instance.MainBGMs != null) {
			selectedBGMIds = new List<int>();
			BGMAudioSource1.clip = GameManager.Instance.MainBGMs[chooseBGMID()];
			BGMAudioSource1.Play();		
		}

		Common.setCursor();
		Cursor.visible = true;

	}

	void Start ()
	{
		init();

		this.UpdateAsObservable().Where(x => CurrentGameState == GameState.GameOver && !!Input.anyKeyDown)
			.Subscribe(_ => {
				LoadGo.SetActive(true);
				SceneManager.LoadScene(Common.Title_Scene);
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.Alpha1))
			.Subscribe(_ => {
				var isInvincible = PlayerAct.changeInvincible();
				HPGo.SetActive(!isInvincible);
				InvincibleTxtGo.SetActive(!!isInvincible);
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.O))
			.Subscribe(_ => {
				if (!OptionCanvasGo.activeSelf) {
					OptionCanvasGo.SetActive(true);
					prevTimeScale = Time.timeScale;
					Time.timeScale = 0.0f;
					PlayerAct.setCanInput(false);
				} else {
					OptionCanvasGo.SetActive(false);
					Time.timeScale = prevTimeScale;
					PlayerAct.setCanInput(true);
				}
			})
			.AddTo(this);
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
	/// Grayちゃん出現時のSEを再生する
	/// </summary>
	public void playAppearSE()
	{
		SEAudioSource.PlayOneShot(GrayChanAppearSEs[Random.Range(0, GrayChanAppearSEs.Length)]);
	}

	/// <summary>
	/// Grayちゃんの敗北時のSEを再生する
	/// </summary>
	void playLoseSE()
	{
		SEAudioSource.PlayOneShot(GrayChanLoseSEs[Random.Range(0, GrayChanLoseSEs.Length)]);
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
			if (GameManager.Instance.MainBGMs != null) {
				crossFade();
			}
		} else {
			changeBossBGM();
		}
	}

	/// <summary>
	/// 以前選ばれたIDを回避してランダムでBGMのIDを選ぶ
	/// </summary>
	/// <returns>BGMのID</returns>
	int chooseBGMID()
	{
		var isDuplication = false;
		var r = 0;
		do {
			isDuplication = false;
			r = Random.Range(0, GameManager.Instance.MainBGMs.Length);
			for (var i = 0; i < selectedBGMIds.Count; ++i) {
				if (r == selectedBGMIds[i]) {
					isDuplication = true;
					break;
				}
			}
		} while (!!isDuplication);
		selectedBGMIds.Add(r);
		StartCoroutine(showNowPlayingBGM(r));
		return r;
	}

	/// <summary>
	/// 現在流れている曲のタイトルを表示
	/// </summary>
	/// <param name="index">選ばれた曲のインデックス</param>
	/// <param name="isBoss">ボス曲かどうか</param>
	/// <returns></returns>
	IEnumerator showNowPlayingBGM(int index, bool isBoss = false)
	{
		if (!!PresentationMode) {
			yield break;
		}

		if (!!isBoss) {
			yield return new WaitForSeconds(0.5f);
		}

		NowPlayingBGMTxt.color = new Color(NowPlayingBGMTxt.color.r, NowPlayingBGMTxt.color.g, NowPlayingBGMTxt.color.b, 1.0f);
		NowPlayingBGMTxt.text = "<sprite=\"Note\" name=\"Note\">" + Common.Main_BGM_Title_List[index];

		yield return new WaitForSeconds(1.0f);

		DOTween.ToAlpha(
			() => NowPlayingBGMTxt.color,
			color => NowPlayingBGMTxt.color = color,
			0.0f,
			3.0f
		);
	}

	/// <summary>
	/// クロスフェードを行う
	/// </summary>
	void crossFade()
	{
		var fadeTime = 3.0f;
		AudioSource bgmAudioSource1 = null;
		AudioSource bgmAudioSource2 = null;
		var useBGM1 = BGMAudioSource1.clip != null;
		if (!!useBGM1) {
			bgmAudioSource1 = BGMAudioSource1;
			bgmAudioSource2 = BGMAudioSource2;
		} else {
			bgmAudioSource1 = BGMAudioSource2;
			bgmAudioSource2 = BGMAudioSource1;
		}
		var clip = GameManager.Instance.MainBGMs[chooseBGMID()];
		bgmAudioSource2.clip = clip;
		bgmAudioSource2.Play();
		DOTween.To(
			() => bgmAudioSource1.volume,
			val => bgmAudioSource1.volume = val,
			0,
			fadeTime
		);
		DOTween.To(
			() => bgmAudioSource2.volume,
			val => bgmAudioSource2.volume = val,
			1.0f,
			fadeTime
		).OnComplete(() => {
			bgmAudioSource1.clip = null;
		});
	}

	/// <summary>
	/// ボス用のBGMに切り替える
	/// </summary>
	void changeBossBGM()
	{
		var fadeTime = 1.0f;
		AudioSource bgmAudioSource1 = null;
		AudioSource bgmAudioSource2 = null;
		var useBGM1 = BGMAudioSource1.clip != null;
		if (!!useBGM1) {
			bgmAudioSource1 = BGMAudioSource1;
			bgmAudioSource2 = BGMAudioSource2;
		} else {
			bgmAudioSource1 = BGMAudioSource2;
			bgmAudioSource2 = BGMAudioSource1;
		}
		var clip = BossBGM;
		bgmAudioSource2.clip = clip;
		DOTween.To(
			() => bgmAudioSource1.volume,
			val => bgmAudioSource1.volume = val,
			0,
			fadeTime
		).OnComplete(() => {
			bgmAudioSource2.volume = 1;
			bgmAudioSource2.Play();
			StartCoroutine(showNowPlayingBGM(Common.Main_BGM_Title_List.Count - 1, true));
		});
	}

	/// <summary>
	/// 風が置けないことを表す
	/// </summary>
	/// <returns></returns>
	public IEnumerator banWind()
	{
		SEAudioSource.PlayOneShot(SEs[Random.Range((int)SE.Ban1, ((int)SE.Ban3) + 1)]);
		BanImgGo.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
		BanImgGo.SetActive(true);
		yield return new WaitForSeconds(1.0f);
		BanImgGo.SetActive(false);
	}

	/// <summary>
	/// オプションを閉じると呼ばれる
	/// </summary>
	public void endOption()
	{
		Time.timeScale = prevTimeScale;
		PlayerAct.setCanInput(true);
	}
}
