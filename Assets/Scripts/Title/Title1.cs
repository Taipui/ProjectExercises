using UnityEngine;

/// <summary>
/// Title1に関するクラス
/// </summary>
public class Title1 : TitleBase
{
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
}
