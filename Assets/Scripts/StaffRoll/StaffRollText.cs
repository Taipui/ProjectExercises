using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;


public class StaffRollText : MonoBehaviour
{
	/// <summary>
	/// 文字の移動速度
	/// </summary>
	const float Move_Speed = 2.0f;

	/// <summary>
	/// StaffRoll
	/// </summary>
	StaffRoll staffRoll;

	/// <summary>
	/// セットするタグのテキスト
	/// </summary>
	string setTag;

	/// <summary>
	/// 値をセット
	/// </summary>
	/// <param name="staffRoll_">セットするStaffRoll</param>
	/// <param name="setTag_">セットするタグのテキスト</param>
	public void setVal(StaffRoll staffRoll_, string setTag_)
	{
		staffRoll = staffRoll_;
		setTag = setTag_;
	}

	void Start ()
	{
		tag = setTag;

		this.UpdateAsObservable().Subscribe(_ => {
			transform.Translate(new Vector2(-Move_Speed * Time.deltaTime, 0.0f));
		})
		.AddTo(this);
	}

	void OnDestroy()
	{
		if (tag != "EndText") {
			return;
		}
		staffRoll.cntEndTxt();
	}
}
