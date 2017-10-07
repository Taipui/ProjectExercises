using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChip : MonoBehaviour
{
	/// <summary>
	/// 地面の階層
	/// </summary>
	public int Hierarchy { private set; get; }

	/// <summary>
	/// 地面の階層をセット
	/// </summary>
	/// <param name="val"></param>
	public void setHierarchy(int val)
	{
		Hierarchy = val;
	}

	void Start ()
	{
		
	}
}

