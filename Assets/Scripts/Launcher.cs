using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{


	void Start ()
	{
		
	}

	public void launch(GameObject bullet, Vector3 targetPos, int layerNo, Transform parent, Vector3 vec)
	{
		var obj = Instantiate(bullet, transform.position, Quaternion.identity);
		var rb = obj.GetComponent<Rigidbody>();
		obj.transform.SetParent(parent);
		if (layerNo == 13) {
			rb.velocity = vec;
		} else {
			// 速さベクトルのままAddForce()を渡してはいけないぞ。力(速さ×重さ)に変換するんだ
			var force = vec * rb.mass;
			rb.AddForce(force, ForceMode.Impulse);
		}
		obj.layer = layerNo;
	}
}

