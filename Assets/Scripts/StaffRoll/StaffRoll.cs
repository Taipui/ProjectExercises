using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffRoll : MonoBehaviour
{

	void Start ()
	{
	}

	public void startStaffRoll()
	{
		var go = FlyingText.GetObject("あいうえお");
		go.AddComponent<StaffRollText>();
	}
}

