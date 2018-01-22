using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;


public class StaffRollText : MonoBehaviour
{
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
	}

	void OnDestroy()
	{
		if (tag != "EndText") {
			return;
		}
		staffRoll.cntEndTxt();
	}
}
