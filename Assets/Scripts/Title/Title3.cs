using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// Title3に関するクラス
/// </summary>
public class Title3 : TitleBase
{
	/// <summary>
	/// マウスオーバーしたGameObjectの情報
	/// </summary>
	RaycastHit hitInfo;

	/// <summary>
	/// UnityちゃんのTransform
	/// </summary>
	[SerializeField]
	Transform UnityChanTfm;
	/// <summary>
	/// Unityちゃんのアニメーター
	/// </summary>
	[SerializeField]
	Animator UnityChanAnim;
	/// <summary>
	/// Avatar
	/// </summary>
	[SerializeField]
	Avatar[] Avatars;
	/// <summary>
	/// モデル
	/// </summary>
	[SerializeField]
	GameObject[] Models;

	/// <summary>
	/// 通常のUnityちゃんのY座標
	/// </summary>
	const float Normal_Y_Pos = -1.45f;
	/// <summary>
	/// SDUnityちゃんのY座標
	/// </summary>
	const float SD_Y_Pos = -0.87f;

	protected override void Start ()
	{
		base.Start();

		for (var i = 0; i < Models.Length; ++i) {
			Models[i].SetActive(false);
		}
		var r = Random.Range(0, 100);
		//r = 0;
		if (r == 0) {
			Models[1].SetActive(true);
			UnityChanAnim.avatar = Avatars[1];
			UnityChanTfm.position = new Vector3(UnityChanTfm.position.x, SD_Y_Pos, UnityChanTfm.position.z);
		} else {
			Models[0].SetActive(true);
			UnityChanAnim.avatar = Avatars[0];
			UnityChanTfm.position = new Vector3(UnityChanTfm.position.x, Normal_Y_Pos, UnityChanTfm.position.z);
		}

		this.UpdateAsObservable().Where(x => !!checkMouseOverGo() && hitInfo.collider.tag == "Text")
			.Subscribe(_ => {
				currentSelect.Value = hitInfo.collider.gameObject.transform.parent.transform.parent.GetComponent<Txt>().Index_;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isLClk())
			.Subscribe(_ => {
				decide();
			})
			.AddTo(this);
	}

	/// <summary>
	/// 次へボタンが押されたかどうか
	/// </summary>
	/// <returns>押された瞬間true</returns>
	protected override bool isNext()
	{
		return !!Input.GetKeyDown(KeyCode.A);
	}

	/// <summary>
	/// 前へボタンが押されたかどうか
	/// </summary>
	/// <returns>押された瞬間true</returns>
	protected override bool isPrev()
	{
		return !!Input.GetKeyDown(KeyCode.D);
	}

	/// <summary>
	/// 左クリックしたかどうか
	/// </summary>
	/// <returns>クリックしたらtrue</returns>
	bool isLClk()
	{
		return !!Input.GetMouseButtonDown(0);
	}

	/// <summary>
	/// マウスオーバーされているGameObjectをチェックする
	/// </summary>
	/// <returns>何かに当たればtrue</returns>
	bool checkMouseOverGo()
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		var isHit = !!Physics.Raycast(ray, out hitInfo);
		return isHit;
	}
}
