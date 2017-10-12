using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// アシストをふわふわさせるクラス
/// </summary>
public class FloatMover : MonoBehaviour
{
	/// <summary>
	/// 通し番号
	/// </summary>
	public int Index { private set; get; }

	/// <summary>
	/// 通し番号をセット
	/// </summary>
	/// <param name="index">通し番号</param>
	public void setIndex(int index)
	{
		Index = index;
	}

	void Start ()
	{
		transform.Translate(new Vector3(0.0f, -0.2f, 0.0f));

		this.UpdateAsObservable().Subscribe(_ => {
			transform.Translate(new Vector3(0.0f, Mathf.Sin(Time.time) / 150, 0.0f));
		})
		.AddTo(this);
	}
}

