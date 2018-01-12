using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class StaffRoll : MonoBehaviour
{
	const float Create_Word_Interval = 1.0f;

	int strCnt;

	StaffRollCameraMover staffRollCamMover;

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
	}
	/// <summary>
	/// SEの配列
	/// </summary>
	[SerializeField]
	AudioClip[] SEs;

	UnityEngine.Audio.AudioMixer audioMixer;

	#endregion

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

	/// <summary>
	/// プレイヤーのアクションに関連するスクリプト
	/// </summary>
	[SerializeField]
	StaffRollPlayerAct Player;

	void init()
	{
		strCnt = 0;

		staffRollCamMover = Camera.main.GetComponent<StaffRollCameraMover>();

		audioMixer = SEAudioSource.outputAudioMixerGroup.audioMixer;

		audioMixer.SetFloat("MasterVol", Mathf.Lerp(-80.0f, 0.0f, PlayerPrefs.GetFloat("Master", 100) / 100));
		audioMixer.SetFloat("BGMVol", Mathf.Lerp(-80.0f, 0.0f, PlayerPrefs.GetFloat("BGM", 100) / 100));
		audioMixer.SetFloat("SEVol", Mathf.Lerp(-80.0f, 0.0f, PlayerPrefs.GetFloat("SE", 100) / 100));

		OptionCanvasGo.SetActive(false);
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void Start ()
	{
		init();

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.O))
			.Subscribe(_ => {
				if (!OptionCanvasGo.activeSelf) {
					OptionCanvasGo.SetActive(true);
					prevTimeScale = Time.timeScale;
					Time.timeScale = 0.0f;
					Player.setCanInput(false);
				} else {
					OptionCanvasGo.SetActive(false);
					Time.timeScale = prevTimeScale;
					Player.setCanInput(true);
				}
			})
			.AddTo(this);
	}

	public void createStr()
	{
		Debug.Log("createStr");
		var str = "";
		switch (strCnt++) {
			case 0:
				str = "あいうえお";
				break;
			case 1:
				str = "かきくけこ";
				break;
		}
		StartCoroutine(createWords(str));
	}

	IEnumerator createWords(string str)
	{
		for (var i = 0; i < str.Length; ++i) {
			var go = FlyingText.GetObject(str[i].ToString());
			var staffRollTxt = go.AddComponent<StaffRollText>();
			staffRollTxt.setStaffRoll(this);
			yield return new WaitForSeconds(Create_Word_Interval);
		}
		staffRollCamMover.resetWordDist();
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
	/// オプションを閉じると呼ばれる
	/// </summary>
	public void endOption()
	{
		Time.timeScale = prevTimeScale;
		Player.setCanInput(true);
	}
}
