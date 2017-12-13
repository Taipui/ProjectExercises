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
		if (GameManager.Instance.BGMs_ == null) {
			Instantiate(Resources.Load("Prefabs/BGMLoader"));
		}

		AudioMixer.SetFloat("MasterVol", PlayerPrefs.GetFloat("Master", 100));
		AudioMixer.SetFloat("BGMVol", PlayerPrefs.GetFloat("BGM", 100));
		AudioMixer.SetFloat("SEVol", PlayerPrefs.GetFloat("SE", 100));

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
