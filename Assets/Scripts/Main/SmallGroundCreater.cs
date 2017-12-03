using UnityEngine;

/// <summary>
/// ちょっとした地面を生成するクラス
/// </summary>
public class SmallGroundCreater : MonoBehaviour
{
	/// <summary>
	/// 並べるGameObject
	/// </summary>
	[SerializeField]
	GameObject GroundChip;
	/// <summary>
	/// 縦に並べる個数
	/// </summary>
	[SerializeField]
	int HNum;
	/// <summary>
	/// 横に並べる個数
	/// </summary>
	[SerializeField]
	int WNum;

	/// <summary>
	/// 並べるGameObjectの幅と高さ
	/// </summary>
	Vector2 groundChipeScale;

	/// <summary>
	/// 生成オプション
	/// </summary>
	enum CreateOption
	{
		/// <summary>
		/// 通常(起伏なし)
		/// </summary>
		Normal,
		/// <summary>
		/// 坂
		/// </summary>
		Slope
	}

	[SerializeField]
	CreateOption CurrentCreateOption;

	void Awake()
	{
		groundChipeScale = GroundChip.transform.localScale;
		groundChipeScale /= 100;

		switch (CurrentCreateOption) {
			default:
				return;
			case CreateOption.Normal:
				createNormal();
				break;
			case CreateOption.Slope:
				createSlope();
				break;
		}
	}

	/// <summary>
	/// 平たい地面を作成する
	/// </summary>
	void createNormal()
	{
		var wPos = 0.0f;
		var hPos = 0.0f;
		for (var w = 0; w < WNum; ++w) {
			for (var h = 0; h < HNum; ++h) {
				var go = Instantiate(GroundChip, transform);
				go.transform.localPosition = new Vector3(wPos, hPos);
				hPos += groundChipeScale.y;
			}
			hPos = 0.0f;
			wPos += groundChipeScale.x;
		}
	}

	/// <summary>
	/// 坂を作成する
	/// </summary>
	void createSlope()
	{
		var wPos = 0.0f;
		var hPos = 0.0f;
		for (var w = 0; w < WNum; ++w) {
			for (var h = 0; h < HNum; ++h) {
				var go = Instantiate(GroundChip, transform);
				go.transform.localPosition = new Vector3(wPos, hPos);
				hPos += groundChipeScale.y;
			}
			hPos = groundChipeScale.y * w * 6;
			wPos += groundChipeScale.x;
		}
	}
}
