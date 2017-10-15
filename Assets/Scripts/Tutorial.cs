using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// チュートリアルに関するクラス
/// </summary>
public class Tutorial : MonoBehaviour
{
	/// <summary>
	/// 自身のコライダ
	/// </summary>
	BoxCollider col;

	/// <summary>
	/// プレイヤーの頭上に表示されるメッセージのGameObject
	/// </summary>
	[SerializeField]
	GameObject PlayerMes;

	void Start ()
	{
		col = GetComponent<BoxCollider>();
		PlayerMes.SetActive(false);

		col.OnTriggerEnterAsObservable().Where(colObj => colObj.gameObject.tag == "Player")
			.Subscribe(_ => {
				PlayerMes.SetActive(true);
			})
			.AddTo(this);

		col.OnTriggerStayAsObservable().Where(colObj => colObj.gameObject.tag == "Player" && !!Input.GetKeyDown(KeyCode.Return))
			.Subscribe(_ => {
				Debug.Log("hoge");
			})
			.AddTo(this);

		col.OnTriggerExitAsObservable().Where(colObj => colObj.gameObject.tag == "Player")
			.Subscribe(_ => {
				PlayerMes.SetActive(false);
			})
			.AddTo(this);
	}
}

