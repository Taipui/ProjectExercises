using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// タイトルシーンに関するクラス
/// </summary>
public class Title : MonoBehaviour
{
	[SerializeField]
	GameObject LoadingGo;

	void Start()
	{
		LoadingGo.SetActive(false);

		this.UpdateAsObservable().Where(x => !!Input.anyKeyDown)
			.Subscribe(_ => {
				LoadingGo.SetActive(true);
				SceneManager.LoadScene(Common.Main);
			}).
			AddTo(this);
	}
}
