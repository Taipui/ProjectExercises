using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGMを裏読みするクラス
/// </summary>
public class BGMLoader : MonoBehaviour
{
	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		DontDestroyOnLoad(gameObject);
	}

	void Start()
	{
		init();

		StartCoroutine("loadBGM");
	}

	/// <summary>
	/// BGMを裏読みする
	/// </summary>
	/// <returns></returns>
	IEnumerator loadBGM()
	{
		GameManager.Instance.MainBGMs = new AudioClip[20];
		GameManager.Instance.StaffRollBGMs = new AudioClip[30];

		while (true) {
			GameManager.Instance.PrevLoadBGMIndex = GameManager.Instance.CurrentLoadBGMIndex;

			var resReq = Resources.LoadAsync<AudioClip>("BGM/Main/mainBgm" + (GameManager.Instance.CurrentLoadBGMIndex + 1).ToString());

			while (!resReq.isDone) {
				yield return 0;
			}

			GameManager.Instance.MainBGMs[GameManager.Instance.CurrentLoadBGMIndex] = resReq.asset as AudioClip;

			if (++GameManager.Instance.CurrentLoadBGMIndex >= GameManager.Instance.MainBGMs.Length) {
				break;
			}
		}

		yield return 0;

		while (true) {
			GameManager.Instance.PrevLoadBGMIndex = GameManager.Instance.CurrentLoadBGMIndex;

			var resReq = Resources.LoadAsync<AudioClip>("BGM/StaffRoll/staffRollBgm" + ((GameManager.Instance.CurrentLoadBGMIndex + 1) - GameManager.Instance.MainBGMs.Length).ToString());

			while (!resReq.isDone) {
				yield return 0;
			}

			GameManager.Instance.StaffRollBGMs[GameManager.Instance.CurrentLoadBGMIndex - GameManager.Instance.MainBGMs.Length] = resReq.asset as AudioClip;

			if (++GameManager.Instance.CurrentLoadBGMIndex >= GameManager.Instance.MainBGMs.Length + GameManager.Instance.StaffRollBGMs.Length) {
				break;
			}
		}
		Destroy(gameObject);
	}
}
