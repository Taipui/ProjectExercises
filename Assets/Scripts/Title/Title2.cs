using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// Title2に関するクラス
/// </summary>

public class Title2 : TitleBase
{
	#region 風関連
	/// <summary>
	/// 風のGameObject
	/// </summary>
	[SerializeField]
	GameObject WindObj;
	#endregion

	/// <summary>
	/// マウスオーバーしたGameObjectの情報
	/// </summary>
	RaycastHit hitInfo;

	protected override void Start()
	{
		base.Start();
		GameObject instanceWindObj = null;

		this.UpdateAsObservable().Where(x => !!isShift() && instanceWindObj == null && !!canInput)
			.Subscribe(_ => {
				var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				instanceWindObj = Instantiate(WindObj, new Vector3(mousePos.x, mousePos.y), Quaternion.identity);
				Destroy(instanceWindObj, 10.0f);
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isTxtMouseOver() && !!canInput)
			.Subscribe(_ => {
				Debug.Log("hoge");
				currentSelect.Value = hitInfo.collider.gameObject.transform.parent.transform.parent.GetComponent<Txt>().Index_;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isLClk() && isTxtMouseOver() && !!canInput)
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
		return !!Input.GetKeyDown(KeyCode.W);
	}

	/// <summary>
	/// 前へボタンが押されたかどうか
	/// </summary>
	/// <returns>押された瞬間true</returns>
	protected override bool isPrev()
	{
		return !!Input.GetKeyDown(KeyCode.S);
	}

	/// <summary>
	/// 左のShiftキーを押したかどうか
	/// </summary>
	/// <returns>押した瞬間true</returns>
	bool isShift()
	{
		return !!Input.GetKeyDown(KeyCode.LeftShift);
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

	/// <summary>
	/// マウスオーバーしたものがテキストかどうか
	/// </summary>
	/// <returns>テキストならtrue</returns>
	bool isTxtMouseOver()
	{
		return !!checkMouseOverGo() && hitInfo.collider.tag == "Text";
	}

	/// <summary>
	/// オプションボタンを押されると呼ばれる
	/// </summary>
	protected override void onClickOptionBtn()
	{
		base.onClickOptionBtn();
		Time.timeScale = 0.0f;
	}

	/// <summary>
	/// オプションを閉じると呼ばれる
	/// </summary>
	public override void endOption()
	{
		base.endOption();
		Time.timeScale = 1.0f;
	}
}
