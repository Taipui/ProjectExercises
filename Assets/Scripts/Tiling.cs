using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マテリアルのタイリング
/// </summary>
public class Tiling : MonoBehaviour
{
	void Start ()
	{
		GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, 0.5f);
	}
}
