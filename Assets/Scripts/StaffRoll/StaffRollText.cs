using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;


public class StaffRollText : MonoBehaviour
{
	const float Move_Speed = 1.0f;

	#region アイテム関連
	
	/// <summary>
	/// アイテムをドロップする力
	/// </summary>
	const float Item_Launch_Power = 2.0f;
	/// <summary>
	/// アイテムをドロップする角度の範囲
	/// </summary>
	const float Item_Launch_Angle_Range = 45.0f;

	#endregion

	StaffRoll staffRoll;

	/// <summary>
	/// StaffRollをセット
	/// </summary>
	/// <param name="staffRoll_">セットするStaffRoll</param>
	public void setStaffRoll(StaffRoll staffRoll_)
	{
		staffRoll = staffRoll_;
	}

	void Start ()
	{
		tag = "Text";
		var col = gameObject.AddComponent<MeshCollider>();
		col.convex = true;

		this.UpdateAsObservable().Subscribe(_ => {
			transform.Translate(new Vector2(-Move_Speed * Time.deltaTime, 0.0f));
		})
		.AddTo(this);

		col.OnCollisionEnterAsObservable().Subscribe(colGo => {
			staffRoll.playSE(StaffRoll.SE.Kill, null);
			Destroy(colGo.gameObject);
			Destroy(gameObject);
			var particleGo = Instantiate(Resources.Load("Prefabs/PopStar2"), transform.position, Quaternion.identity);
			Destroy(particleGo, 1.0f);

			var r = Random.Range(0, GameManager.Instance.ItemSprites_.Length + 1);
			if (r <= 0) {
				return;
			}
			var itemGo = Instantiate(Resources.Load("Prefabs/Item") as GameObject, transform.position, Quaternion.identity);
			var vec = Vector3.up * Item_Launch_Power;
			vec = Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.Range(-Item_Launch_Angle_Range, Item_Launch_Angle_Range))) * vec;
			itemGo.GetComponent<Rigidbody>().AddForce(vec, ForceMode.Impulse);
			itemGo.tag = "Item" + r.ToString();
			var sr = itemGo.GetComponent<SpriteRenderer>();
			var index = r - 1;
			sr.sprite = GameManager.Instance.ItemSprites_[index];
			sr.material = GameManager.Instance.ItemMatsSprite_[index];
		})
		.AddTo(this);
	}
}
