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
	const float First_Word_Pos_X = 30.0f;
	/// <summary>
	/// 文字の間隔
	/// </summary>
	const float Word_Interval = 1.5f;
	/// <summary>
	/// 文の間隔
	/// </summary>
	const float Txt_Interval = 10.0f;
	/// <summary>
	/// 2行目のインデント
	/// </summary>
	const float Indent = 2.0f;
	/// <summary>
	/// 1行目のY座標
	/// </summary>
	const float Line_1_Pos_Y = 7.0f;
	/// <summary>
	/// 2行目のY座標
	/// </summary>
	const float Line_2_Pos_Y = 5.0f;

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
			"ディレクター",
			"速水大秀、畑舜矢",
			"ゲームプランナー",
			"速水大秀、畑舜矢",
			"ゲームプログラマー",
			"速水大秀",
			"ゲームデザイナー",
			"速水大秀、畑舜矢",
			"",
			"参考文献",
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
		var prevWordPosXBegin = 0.0f;
		var prevWordPosXEnd = 0.0f;

		for (var txtIndex = 0; txtIndex < txtArray.Length; ++txtIndex) {
			var isNewLine = txtIndex % 2 == 1;
			if (!isNewLine) {
				prevWordPosXBegin = wordPosX;
			}
			var isCenter = txtArray[Mathf.Max(0, txtIndex - 1)].Length == 0;
			for (var wordIndex = 0; wordIndex < txtArray[txtIndex].Length; ++wordIndex) {
				var go = FlyingText.GetObject(txtArray[txtIndex][wordIndex].ToString());

				var staffRollTxt = go.AddComponent<StaffRollText>();
				staffRollTxt.setStaffRoll(this);

				var indent = 0.0f;
				var posY = Line_1_Pos_Y;
				if (!!isNewLine) {
					indent = Indent;
					posY = Line_2_Pos_Y;
				}
				if (!!isCenter) {
					posY = (Line_1_Pos_Y + Line_2_Pos_Y) / 2;
				}

				go.transform.localPosition = new Vector3(wordPosX + indent, posY);
				wordPosX += Word_Interval;
			}
			if (!!isNewLine) {
				wordPosX = Mathf.Max(wordPosX, prevWordPosXEnd) + Txt_Interval;
			} else {
				prevWordPosXEnd = wordPosX;
				wordPosX = prevWordPosXBegin;
			}
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
