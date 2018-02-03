using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// WARNINGのテキストを動かすクラス
/// </summary>
public class WarningTextMover : MonoBehaviour
{
	/// <summary>
	/// テキストの出現前/出現後のX座標を、中心からどれだけ離れた位置にするか
	/// </summary>
	const float Default_Offset = 1000.0f;

	/// <summary>
	/// テキストが動く速度
	/// </summary>
	const float Move_Speed = 0.3f;

	/// <summary>
	/// テキストが画面の中央で止まる時間
	/// </summary>
	const float Show_Time = Move_Speed + 0.1f;

	void Start ()
	{
		var tfm = transform;

		tfm.Translate(new Vector3(Default_Offset, 0));
	}

	/// <summary>
	/// テキストのアニメーションを再生する
	/// </summary>
	public void play()
	{
		transform.DOMoveX(
			Screen.width / 2,
			Move_Speed
		).OnComplete(() => {
			StartCoroutine("end");
		});
	}

	/// <summary>
	/// テキストをスライドアウトｓる
	/// </summary>
	/// <returns></returns>
	IEnumerator end()
	{
		yield return new WaitForSeconds(Show_Time);

		transform.DOMoveX(
			-Default_Offset,
			Move_Speed
		).OnComplete(() => {
			Destroy(gameObject);
		});
	}
}
