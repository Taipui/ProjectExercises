using UnityEngine;

/// <summary>
/// Title1に関するクラス
/// </summary>
public class Title1 : TitleBase
{
	#region オプション関連

	/// <summary>
	/// プレイヤーのスクリプト
	/// </summary>
	[SerializeField]
	Player Player;

	#endregion

	protected override void Start ()
	{
		base.Start();
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
	/// オプションボタンを押されると呼ばれる
	/// </summary>
	protected override void onClickOptionBtn()
	{
		base.onClickOptionBtn();
		Player.setCanInput(false);
	}

	/// <summary>
	/// オプションを閉じると呼ばれる
	/// </summary>
	public override void endOption()
	{
		base.endOption();
		Player.setCanInput(true);
	}
}
