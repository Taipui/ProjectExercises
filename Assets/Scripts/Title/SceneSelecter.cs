using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// どのタイトルシーンにするかをランダムで選ぶクラス
/// </summary>
public class SceneSelecter : MonoBehaviour
{
	/// <summary>
	/// AudioMixer
	/// </summary>
	[SerializeField]
	UnityEngine.Audio.AudioMixer AudioMixer;

	void Start ()
	{
		if (GameManager.Instance.MainBGMs == null) {
			Instantiate(Resources.Load("Prefabs/BGMLoader"));
		}

		AudioMixer.SetFloat("MasterVol", Mathf.Lerp(-80.0f, 0.0f, PlayerPrefs.GetFloat("Master", 100) / 100));
		AudioMixer.SetFloat("BGMVol", Mathf.Lerp(-80.0f, 0.0f, PlayerPrefs.GetFloat("BGM", 100) / 100));
		AudioMixer.SetFloat("BGM1Vol", 0.0f);
		AudioMixer.SetFloat("BGM2Vol", -80.0f);
		AudioMixer.SetFloat("SEVol", Mathf.Lerp(-80.0f, 0.0f, PlayerPrefs.GetFloat("SE", 100) / 100));

		var r = Random.Range(0, 3);
		//r = 0;
		switch (r) {
			default:
				Debug.Log("エラー");
				return;
			case 0:
				SceneManager.LoadScene("Title1");
				break;
			case 1:
				SceneManager.LoadScene("Title2");
				break;
			case 2:
				SceneManager.LoadScene("Title3");
				break;
		}		
	}
}
