using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// どのタイトルシーンにするかをランダムで選ぶクラス
/// </summary>
public class SceneSelecter : MonoBehaviour
{
	void Start ()
	{
		switch (Random.Range(0, 3)) {
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
