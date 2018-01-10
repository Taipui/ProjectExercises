using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffRoll : MonoBehaviour
{
	const float Create_Word_Interval = 2.0f;

	void Start ()
	{
	}

	public void startStaffRoll()
	{
		//var go = FlyingText.GetObject("あいうえお");
		//go.AddComponent<StaffRollText>();
		StartCoroutine(createWords("あいうえお"));
	}

	IEnumerator createWords(string str)
	{
		for (var i = 0; i < str.Length; ++i) {
			var go = FlyingText.GetObject(str[i].ToString());
			go.AddComponent<StaffRollText>();
			yield return new WaitForSeconds(Create_Word_Interval);
		}
	}
}
