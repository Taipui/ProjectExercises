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
		GameManager.Instance.BGMs_ = new AudioClip[20];
		while (true) {
			var resReq = Resources.LoadAsync<AudioClip>("BGM/bgm" + (GameManager.Instance.CurrentLoadBGMIndex + 1).ToString());

			while (!resReq.isDone) {
				yield return 0;
			}

			GameManager.Instance.BGMs_[GameManager.Instance.CurrentLoadBGMIndex] = resReq.asset as AudioClip;

			if (++GameManager.Instance.CurrentLoadBGMIndex >= GameManager.Instance.BGMs_.Length) {
				break;
			}
		}
		Destroy(gameObject);
	}
}
