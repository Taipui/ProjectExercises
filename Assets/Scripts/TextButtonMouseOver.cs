using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

/// <summary>
/// 文字のボタンのマウスオーバーに関するクラス
/// </summary>
public class TextButtonMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	/// <summary>
	/// ボタンのテキスト
	/// </summary>
	[SerializeField]
	TextMeshProUGUI BtnTxt;

	/// <summary>
	/// マウスオーバー時に変わる色
	/// </summary>
	[SerializeField]
	Color HighlightedCol;

	/// <summary>
	/// マウスオーバー時に色を変えるTween
	/// </summary>
	Tween changeColTween;

	/// <summary>
	/// 何秒で色を変えるか
	/// </summary>
	const float Change_Color_Time = 0.1f;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (changeColTween != null) {
			changeColTween.Kill();
		}
		changeColTween = DOTween.To(
			() => BtnTxt.color,
			col => BtnTxt.color = col,
			HighlightedCol,
			Change_Color_Time
		).SetUpdate(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (changeColTween != null) {
			changeColTween.Kill();
		}
		changeColTween = DOTween.To(
			() => BtnTxt.color,
			col => BtnTxt.color = col,
			Color.white,
			Change_Color_Time
		).SetUpdate(true);
	}
}
