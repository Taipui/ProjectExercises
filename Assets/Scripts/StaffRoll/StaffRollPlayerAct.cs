using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using TMPro;

/// <summary>
/// スタッフロール用のプレイヤーの行動に関するクラス
/// </summary>
public class StaffRollPlayerAct : Character
{
	/// <summary>
	/// StaffRoll
	/// </summary>
	[SerializeField]
	StaffRoll StaffRoll;

	#region 雪弾関連

	/// <summary>
	/// 雪弾の現在のストック数
	/// </summary>
	readonly ReactiveProperty<int> stock = new ReactiveProperty<int>(0);
	/// <summary>
	/// 雪弾の最大ストック数
	/// </summary>
	const int Max_Stock = 10;
	/// <summary>
	/// ストック用の雪弾
	/// </summary>
	[SerializeField]
	GameObject StockBulletPrefab;
	/// <summary>
	/// ストック用の雪弾の親オブジェクトのTransform
	/// </summary>
	[SerializeField]
	Transform StockTfm;
	/// <summary>
	/// 雪弾の発射位置
	/// </summary>
	[SerializeField]
	Transform LaunchTfm;
	/// <summary>
	/// 雪弾を発射する強さ
	/// </summary>
	const float Launch_Power = 40.0f;
	/// <summary>
	/// 雪弾を発射するベクトル
	/// </summary>
	Vector3 launchVec;

	#endregion

	/// <summary>
	/// GroundChipEraser
	/// </summary>
	[SerializeField]
	GroundChipEraser Gce;

	#region アイテム関連

	/// <summary>
	/// UIのアイテムのImage
	/// </summary>
	[SerializeField]
	Image ItemImg;
	/// <summary>
	/// 取得アイテムの状態
	/// </summary>
	enum ItemState
	{
		/// <summary>
		/// アイテムを未取得/効果切れ状態
		/// </summary>
		NoItem,
		/// <summary>
		/// ショットガン
		/// </summary>
		Item1,
		/// <summary>
		/// マシンガン
		/// </summary>
		Item2,
		/// <summary>
		/// 雪弾巨大化
		/// </summary>
		Item4,
		/// <summary>
		/// 雪弾生成数x3
		/// </summary>
		Item5
	}
	/// <summary>
	/// 現在の取得アイテムの状態
	/// </summary>
	ItemState currentItemState;
	/// <summary>
	/// アイテムの耐久度のテキスト
	/// </summary>
	[SerializeField]
	TextMeshProUGUI ItemEffectRemainTxt;
	/// <summary>
	/// アイテムの耐久度の初期値
	/// </summary>
	const int Default_Item_Durability = 10;
	/// <summary>
	/// アイテムの耐久度
	/// </summary>
	readonly ReactiveProperty<int> itemDurability = new ReactiveProperty<int>(Default_Item_Durability);
	public int ItemDurability {
		get
		{
			return itemDurability.Value;
		}
		set
		{
			itemDurability.Value = value;
			if (itemDurability.Value > 0) {
				ItemEffectRemainTxt.text = "残り" + itemDurability.ToString() + "回";
			} else {
				itemLost();
			}
		}
	}
	/// <summary>
	/// ショットガンの上下方向のベクトルのカーブ
	/// </summary>
	[SerializeField]
	AnimationCurve ShotgunYVecCurve;

	/// <summary>
	/// マシンガンで持ち弾を消費して発射する雪弾の数
	/// </summary>
	const int Machinegun_Launch_Num = 3;
	/// <summary>
	/// マシンガンで持ち弾を消費せずに発射する雪弾の数
	/// </summary>
	const int Machinegun_Plus_Alpha_Num = 1;
	/// <summary>
	/// マシンガンで雪弾を発射する間隔
	/// </summary>
	const float Machinegun_Launch_Interval = 0.1f;

	/// <summary>
	/// 雪弾巨大化状態時の雪弾のスケール(倍)
	/// </summary>
	const float Big_Scale = 2.0f;

	/// <summary>
	/// 生成数増加で増やす数
	/// </summary>
	const int Make_Num = 3;

	#endregion

	/// <summary>
	/// スタッフロール用のPlayerMove
	/// </summary>
	StaffRollPlayerMove playerMove;

	/// <summary>
	/// モデルのGameObject
	/// </summary>
	[SerializeField]
	GameObject[] Models;

	/// <summary>
	/// 現在のキャラクター
	/// </summary>
	public int CurrentChar { private set; get; }

	/// <summary>
	/// スタッフロール用のBulletManager
	/// </summary>
	[SerializeField]
	StaffRollBulletManager BulletManager;

	/// <summary>
	/// canInputに値をセットする
	/// </summary>
	/// <param name="val">セットする値</param>
	public void setCanInput(bool val)
	{
		canInput = val;
		playerMove.setCanInput(val);
	}

	void init()
	{
		ItemImg.sprite = null;
		ItemEffectRemainTxt.text = "";
		currentItemState = ItemState.NoItem;

		playerMove = GetComponent<StaffRollPlayerMove>();

		canInput = false;
	}

	protected override void Start ()
	{
		base.Start();

		init();
		var prevStock = 0;
		var stockBullets = new List<GameObject>();

		var prevMousePos = Input.mousePosition;

		var tfm = transform;

		this.UpdateAsObservable().Where(x => !!isPlay() && !!isRClk() && !isMaxStock() && !!canInput)
			.Subscribe(_ => {
				Gce.checkGroundChip();
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay() && !!isLClk() && !!permitLaunchItemState() && !!canInput && !isEmpty())
			.Subscribe(_ => {
				--stock.Value;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay() && !!isLClk() && currentItemState == ItemState.Item1 && !!canInput)
			.Subscribe(_ => {
				shotgunLaunch();
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay() && !!isLClk() && currentItemState == ItemState.Item2 && !!canInput)
			.Subscribe(_ => {
				StartCoroutine("machinegunLaunch");
			})
			.AddTo(this);

		stock.AsObservable().Where(val => val > prevStock)
			.Subscribe(val => {
				var go = Instantiate(StockBulletPrefab, new Vector3(StockTfm.localPosition.x, StockTfm.localPosition.y + 0.03f * val), Quaternion.Euler(0.0f, 90.0f, 0.0f));
				go.transform.SetParent(StockTfm, false);
				stockBullets.Add(go);
				prevStock = val;
			})
			.AddTo(this);

		stock.AsObservable().Where(val => val < prevStock)
			.Subscribe(val => {
				var go = stockBullets[stockBullets.Count - 1];
				stockBullets.RemoveAt(stockBullets.Count - 1);
				Destroy(go);
				launch();
				prevStock = val;
			})
			.AddTo(this);

		itemDurability.AsObservable().Where(val => val <= 0)
			.Subscribe(val => {
				currentItemState = ItemState.NoItem;
				ItemEffectRemainTxt.text = "";
			})
			.AddTo(this);
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
	/// 右クリックしたかどうか
	/// </summary>
	/// <returns>クリックしたらtrue</returns>
	bool isRClk()
	{
		return !!Input.GetMouseButtonDown(1);
	}

	/// <summary>
	/// ストックが一杯かどうか
	/// </summary>
	/// <returns>ストックが一杯ならtrue</returns>
	bool isMaxStock()
	{
		return stock.Value >= Max_Stock;
	}

	/// <summary>
	/// ストックが空かどうか
	/// </summary>
	/// <returns>空ならtrue</returns>
	bool isEmpty()
	{
		return stock.Value <= 0;
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	void launch()
	{
		var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		launchVec = calcLaunchVec();
		var scale = 1.0f;
		if (currentItemState == ItemState.Item4) {
			scale = Big_Scale;
			--ItemDurability;
		}
		// アシストの数だけ一度に撃つ
		foreach (Transform child in LauncherParent) {
			child.GetComponent<Launcher>().cycleLaunch(BulletManager, mousePos, launchVec, scale);
		}

		StaffRoll.playSE(StaffRoll.SE.Launch, audioSource);
	}

	/// <summary>
	/// 発射ベクトルを計算する
	/// </summary>
	/// <returns>発射ベクトル</returns>
	Vector3 calcLaunchVec()
	{
		var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		var direction = Vector3.zero;
		direction.x = mousePos.x - LaunchTfm.position.x;
		direction.y = mousePos.y - LaunchTfm.position.y;
		direction.z = mousePos.z - LaunchTfm.position.z;
		launchVec = direction.normalized * Launch_Power;
		return launchVec;
	}

	/// <summary>
	/// ショットガン
	/// </summary>
	void shotgunLaunch()
	{
		StaffRoll.playSE(StaffRoll.SE.ShotgunLaunch, null);

		var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		var direction = Vector3.zero;
		direction.x = mousePos.x - LaunchTfm.position.x;
		direction.y = mousePos.y - LaunchTfm.position.y;
		direction.z = mousePos.z - LaunchTfm.position.z;
		Vector3[] vecs = new Vector3[3];
		vecs[0] = Vector3.up;
		vecs[1] = Vector3.right;
		vecs[2] = Vector3.down;
		for (var i = 0; i < 3; ++i) {
			var bulletTfm = BulletManager.getBullet();
			var bulletRb = bulletTfm.GetComponent<Rigidbody>();
			if (i == 1) {
				bulletRb.velocity = calcLaunchVec();
			} else {
				var sideVec = vecs[i];
				var inverseLerp = Mathf.InverseLerp(10.0f, 20.0f, direction.magnitude);
				var eval = ShotgunYVecCurve.Evaluate(inverseLerp);
				sideVec *= eval;
				var vec = (sideVec + direction.normalized) * Launch_Power;
				bulletRb.velocity = vec;
			}
			bulletTfm.gameObject.layer = Common.PlayerBulletLayer;
			bulletTfm.position = LaunchTfm.position;
			bulletTfm.GetComponent<StaffRollBullet>().launch();
		}
		--ItemDurability;
	}

	/// <summary>
	/// マシンガン
	/// </summary>
	/// <returns></returns>
	IEnumerator machinegunLaunch()
	{
		--ItemDurability;
		for (var i = 0; i < Machinegun_Launch_Num; ++i) {
			if (stock.Value <= 0) {
				break;
			}
			--stock.Value;
			yield return new WaitForSeconds(Machinegun_Launch_Interval);
		}
		for (var i = 0; i < Machinegun_Plus_Alpha_Num; ++i) {
			launch();
		}
	}

	/// <summary>
	/// アイテムの効果を失った時の処理
	/// </summary>
	void itemLost()
	{
		ItemEffectRemainTxt.text = "";
		ItemImg.sprite = null;
		currentItemState = ItemState.NoItem;
	}

	/// <summary>
	/// currentItemStateによって通常の雪弾の発射を許可する
	/// </summary>
	/// <returns>trueで許可</returns>
	bool permitLaunchItemState()
	{
		switch (currentItemState) {
			default:
				Debug.Log("currentItemStateが不正です。");
				return true;
			case ItemState.Item1:
			case ItemState.Item2:
				return false;
			case ItemState.NoItem:
			case ItemState.Item4:
			case ItemState.Item5:
				return true;
		}
	}

	/// <summary>
	/// 地面のチップが消されたら呼ばれる
	/// </summary>
	public void onErased()
	{
		++stock.Value;
		if (currentItemState != ItemState.Item5) {
			return;
		}
		for (var i = 0; i < Make_Num - 1; ++i) {
			++stock.Value;
		}
		--ItemDurability;
	}

	/// <summary>
	/// アイテムをセットする
	/// </summary>
	/// <param name="index">セットするアイテムの種類</param>
	void setItem(int index)
	{
		ItemImg.sprite = GameManager.Instance.ItemSprites_[--index];
		ItemImg.material = GameManager.Instance.ItemMatsUI_[index];
		ItemEffectRemainTxt.text = "残り" + itemDurability.ToString() + "回";
		ItemDurability = Default_Item_Durability;
	}

	protected override void OnCollisionEnter(Collision col)
	{
		var tag = col.gameObject.tag;
		if (tag == "Piyo") {
			col.gameObject.GetComponent<Piyo>().dead();
			return;
		}
		if (tag.IndexOf("Item") < 0) {
			return;
		}
		StaffRoll.playSE(StaffRoll.SE.Item, null);
		var index = System.Convert.ToInt32(tag.Substring(tag.Length - 1, 1));
		Destroy(col.gameObject);
		switch (index) {
			case 1:
				currentItemState = ItemState.Item1;
				setItem(index);
				break;
			case 2:
				currentItemState = ItemState.Item2;
				setItem(index);
				break;
			case 3:
				for (var i = stock.Value; i < Max_Stock; ++i) {
					++stock.Value;
				}
				break;
			case 4:
				currentItemState = ItemState.Item4;
				setItem(index);
				break;
			case 5:
				currentItemState = ItemState.Item5;
				setItem(index);
				break;
		}
	}

	public delegate void onComplete(int currentChar);
	/// <summary>
	/// ランダムにキャラクターを変える
	/// <param name="callback">StaffRollPlayerMoveに返す</param>
	/// </summary>
	public void randomChangeAvatar(onComplete callback)
	{
		for (var i = 0; i < Models.Length; ++i) {
			Models[i].SetActive(false);
		}

		CurrentChar = Common.getRandomIndex(1000, 50, 1);
		//CurrentChar = 1;
		Models[CurrentChar].SetActive(true);

		callback(CurrentChar);
	}
}
