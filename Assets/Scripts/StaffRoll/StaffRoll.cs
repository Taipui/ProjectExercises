using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// スタッフロールに関するクラス
/// </summary>
public class StaffRoll : MonoBehaviour
{
	#region スタッフロール関連

	/// <summary>
	/// 最初の文字のX座標
	/// </summary>
	const float First_Word_Pos_X = 25.0f;
	/// <summary>
	/// 文字の間隔
	/// </summary>
	const float Word_Interval = 2.0f;
	/// <summary>
	/// 文の間隔
	/// </summary>
	const float Txt_Interval = 2.0f;

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
		var audioMixer = SEAudioSource.outputAudioMixerGroup.audioMixer;

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
		createWords(createStrArray());

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

	/// <summary>
	/// 文のリストを作成
	/// </summary>
	/// <returns>文をまとめた配列</returns>
	string[] createStrArray()
	{
		var txtArray = new string[] {
			"あいうえお",
			"かきくけこ"
		};
		return txtArray;
	}

	/// <summary>
	/// 流れる文字を作る
	/// </summary>
	/// <param name="txtArray">文をまとめた配列</param>
	void createWords(string[] txtArray)
	{
		var wordPosX = First_Word_Pos_X;
		for (var txtIndex = 0; txtIndex < txtArray.Length; ++txtIndex) {
			for (var wordIndex = 0; wordIndex < txtArray[txtIndex].Length; ++wordIndex) {
				var go = FlyingText.GetObject(txtArray[txtIndex][wordIndex].ToString());
				var staffRollTxt = go.AddComponent<StaffRollText>();
				staffRollTxt.setStaffRoll(this);
				go.transform.localPosition = new Vector3(wordPosX, 6.0f);
				wordPosX += Word_Interval;
			}
			wordPosX += Txt_Interval;
		}
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
