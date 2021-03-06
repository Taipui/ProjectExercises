﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using TMPro;
using DG.Tweening;


/// <summary>
/// プレイヤー移動以外のアクションに関するクラス
/// </summary>
public class PlayerAct : Character
{
	#region 雪弾関連
	/// <summary>
	/// 雪弾のストック数
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
	GameObject StockBullet;
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
	/// 特殊能力を発動したかどうか
	/// </summary>
	readonly ReactiveProperty<bool> isSp = new ReactiveProperty<bool>(false);
	public bool IsSp {
		get
		{
			return isSp.Value;
		}
		set
		{
			isSp.Value = value;
		}
	}

	#region HP関連
	/// <summary>
	/// デフォルトのHP
	/// </summary>
	const int Default_Hp = 3;
	/// <summary>
	/// HPを表すGameObject
	/// </summary>
	[SerializeField]
	GameObject[] HPGos;
	#endregion

	/// <summary>
	/// GroundChipErase
	/// </summary>
	[SerializeField]
	GroundChipEraser Gce;

	/// <summary>
	/// 看板の近くにいる時に表示されるメッセージのGameObject
	/// </summary>
	[SerializeField]
	GameObject Mes;

	#region 風関連
	/// <summary>
	/// 風のGameObject
	/// </summary>
	[SerializeField]
	GameObject WindGo;
	/// <summary>
	/// 風の寿命
	/// </summary>
	const float Wind_Lifespan = 10.0f;
	/// <summary>
	/// 風の寿命のタイマー
	/// </summary>
	readonly ReactiveProperty<float> windLifespanTimer = new ReactiveProperty<float>(Wind_Lifespan);
	public float WindLifespanTimer {
		get
		{
			return windLifespanTimer.Value;
		}
		set
		{
			windLifespanTimer.Value = value;
			windLifespanSlider.value = windLifespanTimer.Value;
		}
	}
	/// <summary>
	/// 風の寿命を表示するスライダーのGameObject(スライダーの表示/非表示に使用)
	/// </summary>
	[SerializeField]
	GameObject WindLifespanSliderGo;
	/// <summary>
	/// 風の寿命を表示するスライダー
	/// </summary>
	Slider windLifespanSlider;
	#endregion

	#region 変身関連
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
	/// 現在のAvatar
	/// </summary>
	public int CurrentAvatar { private set; get; }
	/// <summary>
	/// 変身可能かどうか
	/// </summary>
	public bool EnableChange { private set; get; }
	#endregion

	#region 瞬間移動関連
	/// <summary>
	/// 残像のパーティクルの親
	/// </summary>
	[SerializeField]
	Transform[] ParticleParents;
	/// <summary>
	/// 瞬間移動可能かどうか
	/// </summary>
	bool enableTeleportation;
	#endregion

	/// <summary>
	/// 煙のPrefab
	/// </summary>
	[SerializeField]
	GameObject Smoke;

	/// <summary>
	/// DestroyCollider
	/// </summary>
	[SerializeField]
	DestroyCollider Dc;

	#region アイテム関連
	/// <summary>
	/// UIのImage
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

	#region 軌跡関連
	/// <summary>
	/// 軌跡を描くために発射するコライダのTransform
	/// </summary>
	[SerializeField]
	Transform LocusDrawColTfm;
	/// <summary>
	/// 軌跡を点として記録する間隔
	/// </summary>
	const float Sampling_Interval = 0.01f;
	/// <summary>
	/// 軌跡の点のリスト
	/// </summary>
	List<Vector3> locusPoses;
	/// <summary>
	/// LineRenderer
	/// </summary>
	[SerializeField]
	LineRenderer Lr;
	/// <summary>
	/// 軌跡の線の幅
	/// </summary>
	const float Locus_Width = 0.1f;
	#endregion

	/// <summary>
	/// CameraMover(カメラを揺らすため)
	/// </summary>
	CameraMover camMover;

	/// <summary>
	/// Title
	/// </summary>
	[SerializeField]
	TitleBase Title;

	#region SE関連
	/// <summary>
	/// ダメージを受けた時に再生するSEの配列
	/// 9
	/// 1030
	/// 1038
	/// 1091
	/// 1092
	/// 1093
	/// 1094
	/// 1095
	/// 1258
	/// 1259
	/// </summary>
	[SerializeField]
	AudioClip[] DmgSEs;
	/// <summary>
	/// ゲームオーバーになった時に再生するSEの配列
	/// 4
	/// 0010
	/// 0011
	/// 1024
	/// 1025
	/// </summary>
	[SerializeField]
	AudioClip[] GameOverSEs;
	/// <summary>
	/// アイテム取得時のSEの配列
	/// 1
	/// 1070
	/// </summary>
	[SerializeField]
	AudioClip[] ItemSEs;
	/// <summary>
	/// 雪弾を投げる時のSEの配列
	/// 1
	/// 1179
	/// </summary>
	[SerializeField]
	AudioClip[] LaunchSEs;
	/// <summary>
	/// ショットガンを撃つ時のSEの配列
	/// 1
	/// 1222
	/// </summary>
	[SerializeField]
	AudioClip[] ShotgunLaunchSEs;
	/// <summary>
	/// マシンガンを撃つ時のSEの配列
	/// 2
	/// 1223
	/// 1227
	/// </summary>
	[SerializeField]
	AudioClip[] MachinegunLaunchSEs;
	/// <summary>
	/// 雪弾のストックがない状態で発射しようとした時のSEの配列
	/// 1
	/// 1229
	/// </summary>
	[SerializeField]
	AudioClip[] EmptySEs;
	/// <summary>
	/// 変身時のSEの配列
	/// 1
	/// 1087
	/// </summary>
	[SerializeField]
	AudioClip[] TransformSEs;
	#endregion

	/// <summary>
	/// 変身時のパーティクルの配列
	/// </summary>
	[SerializeField]
	ParticleSystem ChangeParticle;
	
	/// <summary>
	/// PlayerMove
	/// </summary>
	PlayerMove playerMove;

	/// <summary>
	/// canInputに値をセットする
	/// </summary>
	/// <param name="val">セットする値</param>
	public void setCanInput(bool val)
	{
		canInput = val;
		playerMove.setCanInput(val);
	}

	/// <summary>
	/// 変身可能かどうかのフラグをセット
	/// </summary>
	/// <param name="val">セットする値</param>
	public void setEnableChange(bool val)
	{
		EnableChange = val;
	}

	/// <summary>
	/// 風を置ける場所か
	/// </summary>
	/// <returns>置けるならtrue</returns>
	bool enableSpawnWindArea()
	{
		var tapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		var col = Physics2D.OverlapPoint(tapPos);
		if (!!col) {
			var hit = Physics2D.Raycast(tapPos, -Vector2.up);
			if (!!hit && hit.collider.tag == "WindBanArea") {
				StopCoroutine(Main.banWind());
				StartCoroutine(Main.banWind());
				return false;
			}
		}
		return true;
	}

	protected override void Start()
	{
		base.Start();
		init();
		var prevStock = 0;
		var stockBullets = new List<GameObject>();
		GameObject instanceWindGo = null;
		Tutorial contactSignboard = null;
		var prevMousePos = Input.mousePosition;
		var prevPlayerXPos = transform.localPosition.x;
		var tfm = transform;

		this.UpdateAsObservable().Where(x => !!isSpJump() && !!playerMove.enableJump() && !playerMove.IsTitle_)
			.Subscribe(_ => {
				playerMove.jump();
				isSp.Value = true;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay() && !!isRClk() && !isMaxStock() && !!playerMove.enableJump() && !playerMove.IsTitle_)
			.Subscribe(_ => {
				Gce.checkGroundChip();
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay() && !!isLClk() && !!permitLaunchItemState() && !playerMove.IsTitle_ && !!canInput && !isEmpty())
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
				var go = Instantiate(StockBullet, new Vector3(StockTfm.localPosition.x, StockTfm.localPosition.y + 0.03f * val), Quaternion.Euler(0.0f, 90.0f, 0.0f));
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

		this.UpdateAsObservable().Where(x => !!isLClk() && !!playerMove.IsTitle_ && !!canInput)
			.Subscribe(_ => {
				launch();
			})
			.AddTo(this);

		isSp.AsObservable().Where(val => !!val)
			.Subscribe(_ => {
				Time.timeScale = 0.5f;
			})
			.AddTo(this);

		isSp.AsObservable().Where(val => !val)
			.Subscribe(_ => {
				Time.timeScale = 1.0f;
			})
			.AddTo(this);

		tfm.UpdateAsObservable().Where(x => tfm.position.y <= -10.0f)
			.Subscribe(_ => {
				dead();
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isSp.Value && !!isShift() && instanceWindGo == null && !!enableSpawnWindArea())
			.Subscribe(_ => {
				var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				instanceWindGo = Instantiate(WindGo, new Vector3(mousePos.x, mousePos.y), Quaternion.identity);
				if (Main != null) {
					instanceWindGo.GetComponent<Wind>().setMain(Main);
				} else if (Title != null) {
					instanceWindGo.GetComponent<Wind>().setTitle(Title);
				}
				WindLifespanSliderGo.SetActive(true);
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => instanceWindGo != null)
			.Subscribe(_ => {
				WindLifespanTimer -= Time.deltaTime;
			})
			.AddTo(this);

		windLifespanTimer.AsObservable().Where(val => val <= 0.0f)
			.Subscribe(_ => {
				Destroy(instanceWindGo);
				WindLifespanTimer = Wind_Lifespan;
				WindLifespanSliderGo.SetActive(false);
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isChange() && !!EnableChange && !!canInput)
			.Subscribe(_ => {
				changeAvatar();
				camMover.shake();
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isTeleportation())
			.Subscribe(_ => {
				StartCoroutine("playAfterimage");
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isRead() && contactSignboard != null)
			.Subscribe(_ => {
				contactSignboard.execPop();
			})
			.AddTo(this);

		playerMove.Col.OnTriggerEnterAsObservable().Where(colGo => !!isSignboard(colGo))
			.Subscribe(colGo => {
				contactSignboard = colGo.GetComponent<Tutorial>();
			})
			.AddTo(this);

		playerMove.Col.OnTriggerExitAsObservable().Where(colGo => !!isSignboard(colGo))
			.Subscribe(_ => {
				contactSignboard = null;
			})
			.AddTo(this);

		itemDurability.AsObservable().Where(val => val <= 0)
			.Subscribe(val => {
				currentItemState = ItemState.NoItem;
				ItemEffectRemainTxt.text = "";
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay())
			.Subscribe(_ => {
				drawLocus();
			})
			.AddTo(this);

		//this.UpdateAsObservable().Where(x => !!isPlay() && prevMousePos != Input.mousePosition)
		//	.Subscribe(_ => {
		//		prevMousePos = Input.mousePosition;
		//		launchLocusDrawCol();
		//	})
		//	.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay() && Mathf.Abs(prevPlayerXPos - transform.localPosition.x) >= 0.1f)
			.Subscribe(_ => {
				prevPlayerXPos = transform.localPosition.x;
				launchLocusDrawCol();
			})
			.AddTo(this);
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		MyBulletLayer = Common.PlayerBulletLayer;

		hp = Default_Hp;

		enableTeleportation = true;

		windLifespanSlider = WindLifespanSliderGo.GetComponent<Slider>();
		WindLifespanSliderGo.SetActive(false);

		CurrentAvatar = 0;
		playerMove = GetComponent<PlayerMove>();
		canInput = true;

		foreach (Transform parentTfm in ParticleParents) {
			if (parentTfm == null) {
				return;
			}
			foreach (Transform childTfm in parentTfm) {
				childTfm.gameObject.SetActive(false);
			}
		}

		ItemImg.sprite = null;
		ItemEffectRemainTxt.text = "";
		currentItemState = ItemState.NoItem;

		Lr.startWidth = Locus_Width;
		Lr.endWidth = Locus_Width;
		locusPoses = new List<Vector3>();
		launchLocusDrawCol();

		camMover = Camera.main.GetComponent<CameraMover>();
	}

	/// <summary>
	/// 看板の近くにいる時に表示されるメッセージを反転させる
	/// </summary>
	public void flipMes()
	{
		Mes.transform.localScale = new Vector3(-Mes.transform.localScale.x, Mes.transform.localScale.y, Mes.transform.localScale.z);
	}

	/// <summary>
	/// 特殊ジャンプキーを押したかどうか
	/// </summary>
	/// <returns>押したらtrue</returns>
	bool isSpJump()
	{
		return !!Input.GetButtonDown("Jump");
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
	/// Sキーを押したかどうか
	/// </summary>
	/// <returns>押した瞬間true</returns>
	bool isChange()
	{
		return !!Input.GetKeyDown(KeyCode.Return);
	}

	/// <summary>
	/// Enterキーを押したかどうか
	/// </summary>
	/// <returns>押した瞬間true</returns>
	bool isRead()
	{
		return !!Input.GetKeyDown(KeyCode.F);
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
	/// 瞬間移動のキーを押したかどうか
	/// </summary>
	/// <returns>押した瞬間true</returns>
	bool isTeleportation()
	{
		return !!Input.GetMouseButtonDown(2);
	}

	/// <summary>
	/// 残像を再生する
	/// </summary>
	/// <returns></returns>
	IEnumerator playAfterimage()
	{
		if (!enableTeleportation) {
			yield break;
		}
		enableTeleportation = false;
		foreach (Transform tfm in ParticleParents[CurrentAvatar]) {
			tfm.gameObject.SetActive(true);
		}
		StartCoroutine("teleportation");
		yield return new WaitForSeconds(0.5f);
		enableTeleportation = true;
		foreach (Transform tfm in ParticleParents[CurrentAvatar]) {
			tfm.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// 瞬間移動する
	/// </summary>
	/// <returns></returns>
	IEnumerator teleportation()
	{
		Main.playSE(Main.SE.Teleportation, null);

		var power = 200.0f;
		if (playerMove.Dir == "D") {
			rb.AddForce(Vector3.right * power, ForceMode.Impulse);
		} else if (playerMove.Dir == "A") {
			rb.AddForce(Vector3.left * power, ForceMode.Impulse);
		}
		yield return new WaitForSeconds(0.1f);
		rb.velocity = Vector3.zero;
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
		if (!!playerMove.IsTitle_) {
			return false;
		}
		if (stock.Value <= 0) {
			audioSource.Stop();
			audioSource.PlayOneShot(EmptySEs[Random.Range(0, EmptySEs.Length)]);
			return true;
		}
		return false;
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	void launch()
	{
		var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		if (!!playerMove.IsTitle_) {
			launchVec = calcLaunchVec();
		}
		var scale = 1.0f;
		if (currentItemState == ItemState.Item4) {
			scale = Big_Scale;
			--ItemDurability;
		}
		// アシストの数だけ一度に撃つ
		foreach (Transform child in LauncherParent) {
			child.GetComponent<Launcher>().createLaunch(Bullet, mousePos, 13, BulletParentTfm, launchVec, scale);
		}

		if (Main != null) {
			Main.playSE(Main.SE.Launch, audioSource);
		}
		if (Title != null) {
			Title.playSE(TitleBase.SE.Launch);
		}

		//audioSource.PlayOneShot(LaunchSEs[Random.Range(0, LaunchSEs.Length)]);
	}

	/// <summary>
	/// LocusDrawColliderを初期座標に戻し、発射する
	/// </summary>
	public void launchLocusDrawCol()
	{
		if (LocusDrawColTfm == null) {
			return;
		}
		StopCoroutine("recordLocus");
		if (locusPoses != null) {
			locusPoses.Clear();
		}
		LocusDrawColTfm.position = LaunchTfm.position;
		var rb = LocusDrawColTfm.GetComponent<Rigidbody>();
		rb.velocity = calcLaunchVec();
		StartCoroutine("recordLocus");
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
	/// 軌跡を点として記録する
	/// </summary>
	/// <returns></returns>
	IEnumerator recordLocus()
	{
		while (true) {
			locusPoses.Add(LocusDrawColTfm.position);
			yield return new WaitForSeconds(Sampling_Interval);
		}
	}

	/// <summary>
	/// 軌跡を描画する
	/// </summary>
	void drawLocus()
	{
		if (!!playerMove.IsTitle_) {
			return;
		}
		Lr.positionCount = locusPoses.Count;
		var startColor = Color.red;
		var endColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
		var val = 0.01f;
		for (var i = 0; i < locusPoses.Count; ++i) {
			Lr.startColor = startColor;
			Lr.endColor = endColor;
			startColor = new Color(Mathf.Max(0.0f, startColor.r - val), Mathf.Min(1.0f, startColor.g + val), startColor.b, startColor.a);
			endColor = new Color(Mathf.Max(0.0f, endColor.r - val), endColor.g, endColor.b, endColor.a);
			Lr.SetPosition(i, locusPoses[i]);
		}
	}

	/// <summary>
	/// ショットガン
	/// </summary>
	void shotgunLaunch()
	{
		Main.playSE(Main.SE.ShotgunLaunch, null);
		audioSource.PlayOneShot(ShotgunLaunchSEs[Random.Range(0, ShotgunLaunchSEs.Length)]);

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
			var go = Instantiate(Bullet, LaunchTfm.position, Quaternion.identity);
			if (i == 1) {
				go.GetComponent<Rigidbody>().velocity = calcLaunchVec();
			} else {
				var sideVec = vecs[i];
				var inverseLerp = Mathf.InverseLerp(10.0f, 20.0f, direction.magnitude);
				var eval = ShotgunYVecCurve.Evaluate(inverseLerp);
				sideVec *= eval;
				//Debug.Log("direction.magnitude" + direction.magnitude);
				//Debug.Log("InverseLerp:" + inverseLerp);
				//Debug.Log("Eval:" + eval);
				//Debug.Log("sideVec.magnitude:" + sideVec.magnitude);
				var vec = (sideVec + direction.normalized) * Launch_Power;
				go.GetComponent<Rigidbody>().velocity = vec;
			}
			go.layer = Common.PlayerBulletLayer;
		}
		//Debug.Break();
		--ItemDurability;
	}

	/// <summary>
	/// マシンガン
	/// </summary>
	/// <returns></returns>
	IEnumerator machinegunLaunch()
	{
		audioSource.PlayOneShot(MachinegunLaunchSEs[Random.Range(0, MachinegunLaunchSEs.Length)]);
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
	/// currentItemStateによって雪弾の発射を許可する
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
//		EffectHiyoko.SetActive(false);
		//HiyokoGo.SetActive(true);
	}

	/// <summary>
	/// ダメージ処理
	/// </summary>
	protected override IEnumerator dmg()
	{
		StartCoroutine(base.dmg());
		camMover.shake(0.7f);
		if (hp < 0 || hp > Default_Hp) {
			yield break;
		}
		HPGos[hp].SetActive(false);
		audioSource.PlayOneShot(DmgSEs[Random.Range(0, DmgSEs.Length)]);
		yield return null;
	}

	/// <summary>
	/// 点滅を始める
	/// </summary>
	protected override void startFlick()
	{
		flickCoroutine = StartCoroutine(flick(Models[CurrentAvatar]));
	}

	/// <summary>
	/// 点滅を止める
	/// </summary>
	protected override void stopFlick()
	{
		StopCoroutine(flickCoroutine);
		Models[CurrentAvatar].SetActive(true);
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		//base.dead();
		canInput = false;
		Main.gameOver();
		audioSource.Stop();
		audioSource.PlayOneShot(GameOverSEs[Random.Range(0, GameOverSEs.Length)]);
		//Destroy(gameObject);
	}

	/// <summary>
	/// Avatarを変更する
	/// </summary>
	void changeAvatar()
	{
		ChangeParticle.Play();
		audioSource.PlayOneShot(TransformSEs[Random.Range(0, TransformSEs.Length)]);
		Main.playSE(Main.SE.Transform, null);
		CurrentAvatar = (CurrentAvatar + 1) % Avatars.Length;
		foreach (GameObject go in Models) {
			go.SetActive(false);
		}
		Models[CurrentAvatar].SetActive(true);
		anim.avatar = Avatars[CurrentAvatar];
		if (CurrentAvatar == 0) {
			playerMove.Col.height = playerMove.OrgColHeightNormal;
			playerMove.Col.center = playerMove.OrgVectColCenterNormal;
		} else {
			playerMove.Col.height /= 2;
			playerMove.Col.center = new Vector3(playerMove.Col.center.x, playerMove.Col.center.y - playerMove.Col.height / 2, playerMove.Col.center.z);
		}
		var smoke = Instantiate(Smoke, new Vector3(transform.position.x, transform.position.y + 1.0f, -1.0f), Quaternion.identity);
		Destroy(smoke, 5.0f);

		Dc.destroy();
		EnableChange = false;
	}

	/// <summary>
	/// 当たったものが看板かどうか
	/// </summary>
	/// <param name="col">当たったもの</param>
	/// <returns>看板ならtrue</returns>
	bool isSignboard(Collider col)
	{
		return col.tag == "Signboard";
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
		base.OnCollisionEnter(col);
		var tag = col.gameObject.tag;
		if (tag.IndexOf("Item") < 0) {
			return;
		}
		Main.playSE(Main.SE.Item, null);
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
		audioSource.PlayOneShot(ItemSEs[Random.Range(0, ItemSEs.Length)]);
	}

	void OnDestroy()
	{
		StopAllCoroutines();
	}

	/// <summary>
	/// 無敵状態の切り替え
	/// <returns>無敵ならtrue</returns>
	/// </summary>
	public bool changeInvincible()
	{
		isInvincible = !isInvincible;
		return isInvincible;
	}
}
