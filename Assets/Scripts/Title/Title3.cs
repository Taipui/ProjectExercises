using System.Collections;
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
	/// タイトル用のキャラクターのTransform
	/// </summary>
	[SerializeField]
	Transform TitleCharTfm;
	/// <summary>
	/// タイトル用のキャラクターのアニメーターの配列
	/// </summary>
	[SerializeField]
	Animator[] TitleCharAnimators;
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

	/// <summary>
	/// 待機時のモーションを行う最低間隔
	/// </summary>
	const float Idle_Animation_Interval_Base = 15.0f;
	/// <summary>
	/// 待機時のモーションを行うランダムな間隔の範囲(Idle_Animation_Interval_Base + Random.Range(0, Idle_Animation_Interval_Random))
	/// </summary>
	const float Idle_Animation_Interval_Random = 15.0f;

	/// <summary>
	/// 現在のキャラクター
	/// </summary>
	int currentChar;

	protected override void Start ()
	{
		base.Start();

		randomChangeAvatar();

		StartCoroutine("randomInterval");

		this.UpdateAsObservable().Where(x => !!isTxtMouseOver() && !!canInput)
			.Subscribe(_ => {
				currentSelect.Value = hitInfo.collider.gameObject.transform.parent.transform.parent.GetComponent<Txt>().Index_;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isLClk() && !!isTxtMouseOver() && !!canInput)
			.Subscribe(_ => {
				decide();
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => Input.GetKeyDown(KeyCode.Alpha1))
			.Subscribe(_ => {
				TitleCharAnimators[currentChar].SetTrigger("doJump");
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

	/// <summary>
	/// ランダムにキャラクターを変える
	/// </summary>
	void randomChangeAvatar()
	{
		for (var i = 0; i < Models.Length; ++i) {
			Models[i].SetActive(false);
		}

		currentChar = Common.getRandomIndex(1000, 50, 1);
		//currentChar = 1;
		Models[currentChar].SetActive(true);

		var posY = 0.0f;

		switch (currentChar) {
			case 0:
			case 2:
				posY = Normal_Y_Pos;
				break;
			case 1:
				posY = SD_Y_Pos;
				break;
		}

		TitleCharTfm.position = new Vector3(TitleCharTfm.position.x, posY, TitleCharTfm.position.z);
	}

	/// <summary>
	/// ランダムな間隔で実行する
	/// </summary>
	/// <returns></returns>
	IEnumerator randomInterval()
	{
		while (true) {
			yield return new WaitForSeconds(Idle_Animation_Interval_Base + Random.Range(0, Idle_Animation_Interval_Random));
			randomIdleMotion();
		}
	}

	/// <summary>
	/// ランダムに待機時のモーションを行う
	/// </summary>
	void randomIdleMotion()
	{
		switch (currentChar) {
			case 0:
			case 2:
				var animStr = "";

				var r = Random.Range(0, 3);
				//r = 2;
				switch (r) {
					case 0:
						animStr = "doWAIT01";
						break;
					case 1:
						animStr = "doWAIT02";
						break;
					case 2:
						animStr = "doWAIT04";
						break;
				}
				TitleCharAnimators[currentChar].SetTrigger(animStr);
				//Debug.Log(r);
				break;
			case 1:
				return;
		}
	}
}
