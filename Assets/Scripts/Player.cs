using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using TMPro;

// 必要なコンポーネントの列記
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]

/// <summary>
/// プレイヤー関連のクラス
/// </summary>
public class Player : Character
{
	#region アニメーション関連
	/// <summary>
	/// アニメーションの再生速度
	/// </summary>
	const float Anim_Speed = 1.5f;
	/// <summary>
	/// Mecanimでカーブ調整を使うかどうか
	/// </summary>
	const bool Use_Curve = true;
	/// <summary>
	/// カーブ補正の有効高さ(地面をすり抜けやすい時には大きくする)
	/// </summary>
	const float Use_Curve_Height = 0.25f;
	/// <summary>
	/// アニメーター
	/// </summary>
	Animator anim;
	#endregion

	#region 各ステートの参照
	/// <summary>
	/// Idleステートの参照
	/// </summary>
	static int idleState = Animator.StringToHash("Base Layer.Idle");
	/// <summary>
	/// Locomotionステートの参照
	/// </summary>
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");
	/// <summary>
	/// jumpステートの参照
	/// </summary>
	static int jumpState = Animator.StringToHash("Base Layer.Jump");
	#endregion

	#region プレイヤーのアクション関連
	/// <summary>
	/// 前進速度
	/// </summary>
	const float Forward_Speed = 7.0f;
	/// <summary>
	/// ジャンプ力
	/// </summary>
	const float Jump_Power = 4.0f;
	#endregion

	/// <summary>
	/// キャラクターのコライダ
	/// </summary>
	CapsuleCollider col;
	/// <summary>
	/// プレイヤーのRidigbody
	/// </summary>
	Rigidbody rb;
	/// <summary>
	/// CapsuleColliderで設定されているコライダのHeightの初期値を収める変数(Normal)
	/// </summary>
	float orgColHightNormal;
	/// <summary>
	/// CapsuleColliderで設定されているコライダのCenterの初期値を収める変数(Normal)
	/// </summary>
	Vector3 orgVectColCenterNormal;
	/// <summary>
	/// CapsuleColliderで設定されているコライダのHeightの初期値を収める変数(Normal)
	/// </summary>
	float orgColHightSD;
	/// <summary>
	/// CapsuleColliderで設定されているコライダのCenterの初期値を収める変数(Normal)
	/// </summary>
	Vector3 orgVectColCenterSD;
	/// <summary>
	/// 横の移動量
	/// </summary>
	readonly ReactiveProperty<float> h = new ReactiveProperty<float>(0.0f);

	/// <summary>
	/// プレイヤーの向いている方向
	/// </summary>
	readonly ReactiveProperty<string> dir = new ReactiveProperty<string>("D");

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
	int currentAvatar = 0;
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

	/// <summary>
	/// タイトル用かどうか
	/// </summary>
	[SerializeField]
	bool IsTitle;

	#region アイテム関連
	/// <summary>
	/// UIのImage
	/// </summary>
	[SerializeField]
	Image ItemImg;
	/// <summary>
	/// UIに設定するアイテムのSprite
	/// </summary>
	[SerializeField]
	Sprite[] ItemSprites;
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
		Item1
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
				ItemEffectRemainTxt.text = "";
			}
		}
	}
	/// <summary>
	/// ショットガンの上下方向のベクトルのカーブ
	/// </summary>
	[SerializeField]
	AnimationCurve ShotgunYVecCurve;
	/// <summary>
	/// ショットガンで一度に飛ばす弾の数
	/// </summary>
	const int Max_Shotgun_Num = 3;
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
	[SerializeField]
	CameraMover CamMover;

	/// <summary>
	/// 雪弾を取りに行くアニメーションをするためのhiyoko
	/// </summary>
	//[SerializeField]
	//GameObject EffectHiyoko;
	/// <summary>
	/// hiyokoのGameObject
	/// </summary>
	//[SerializeField]
	//GameObject HiyokoGo;

	/// <summary>
	/// 変身可能かどうかのフラグをセット
	/// </summary>
	/// <param name="val">セットする値</param>
	public void setEnableChange(bool val)
	{
		EnableChange = val;
	}

	protected override void Start()
	{
		base.Start();
		init();
		var currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
		var velocity = Vector3.zero;
		var prevStock = 0;
		var stockBullets = new List<GameObject>();
		GameObject instanceWindGo = null;
		Tutorial contactSignboard = null;
		var tfm = transform;
		var cachedLocalPosition = tfm.localPosition;
		var prevMousePos = Input.mousePosition;
		var prevPlayerXPos = transform.localPosition.x;

		this.FixedUpdateAsObservable().Subscribe(_ => {
			h.Value = Input.GetAxis("Horizontal");
			anim.SetFloat("Speed", Mathf.Abs(h.Value));
			anim.speed = Anim_Speed;
			currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
			rb.useGravity = true;

			// 以下、キャラクターの移動処理
			velocity = new Vector3(h.Value, 0, 0);

			//以下のvの閾値は、Mecanim側のトランジションと一緒に調整する
			if (Mathf.Abs(h.Value) > 0.1f) {
				velocity.x *= Forward_Speed;
				velocity.y *= Forward_Speed;
				velocity.z *= Forward_Speed;
			}

			tfm.localPosition += velocity * Time.fixedDeltaTime;
			// 以下のようにするとおかしくなる
			//cachedLocalPosition.x += velocity.x * Time.fixedDeltaTime;
			//cachedLocalPosition.y += velocity.y * Time.fixedDeltaTime;
			//cachedLocalPosition.z += velocity.z * Time.fixedDeltaTime;
			//tfm.localPosition = cachedLocalPosition;

			// 以下、Animatorの各ステート中での処理
			// Locomotion中
			// 現在のベースレイヤーがlocoStateの時
			if (currentBaseState.fullPathHash == locoState) {
				//カーブでコライダ調整をしている時は、念のためにリセットする
				if (!!Use_Curve) {
					resetCollider();
				}
			}
			// JUMP中の処理
			// 現在のベースレイヤーがjumpStateの時
			else if (currentBaseState.fullPathHash == jumpState) {
				// ステートがトランジション中でない場合
				if (!anim.IsInTransition(0)) {

					// 以下、カーブ調整をする場合の処理
					if (!!Use_Curve) {
						// 以下JUMP00アニメーションについているカーブJumpHeightとGravityControl
						// JumpHeight:JUMP00でのジャンプの高さ（0〜1）
						// GravityControl:1⇒ジャンプ中（重力無効）、0⇒重力有効
						var jumpHeight = anim.GetFloat("JumpHeight");
						var gravityControl = anim.GetFloat("GravityControl");
						if (gravityControl > 0.0f) {
							rb.useGravity = false;  //ジャンプ中の重力の影響を切る
						}

						// レイキャストをキャラクターのセンターから落とす
						var ray = new Ray(transform.position + Vector3.up, -Vector3.up);
						var hitInfo = new RaycastHit();
						// 高さが useCurvesHeight 以上ある時のみ、コライダーの高さと中心をJUMP00アニメーションについているカーブで調整する
						if (!!Physics.Raycast(ray, out hitInfo)) {
							if (hitInfo.distance > Use_Curve_Height) {
								var orgColHight = currentAvatar == 0 ? orgColHightNormal : orgColHightSD;
								var orgColCenter = currentAvatar == 0 ? orgVectColCenterNormal : orgVectColCenterSD;
								col.height = orgColHight - jumpHeight;          // 調整されたコライダーの高さ
								var adjCenterY = orgColCenter.y + jumpHeight;
								col.center = new Vector3(0.0f, adjCenterY, 0.0f); // 調整されたコライダーのセンター
							} else {
								// 閾値よりも低い時には初期値に戻す（念のため）
								resetCollider();
							}
						}
					}
					// Jump bool値をリセットする（ループしないようにする）				
					anim.SetBool("Jump", false);
				}
			}
			// IDLE中の処理
			// 現在のベースレイヤーがidleStateの時
			else if (currentBaseState.fullPathHash == idleState) {
				//カーブでコライダ調整をしている時は、念のためにリセットする
				if (!!Use_Curve) {
					resetCollider();
				}
			}
		})
		.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isSpJump())
			.Subscribe(_ => {
				isSp.Value = true;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => (!!isJump() || !!isSpJump()) && !!enableJump(currentBaseState))
			.Subscribe(_ => {
				jump();
			})
			.AddTo(this);

		h.AsObservable().Where(val => val < 0)
			.Subscribe(_ => {
				dir.Value = "A";
			})
			.AddTo(this);

		h.AsObservable().Where(val => val > 0)
			.Subscribe(_ => {
				dir.Value = "D";
			})
			.AddTo(this);

		dir.SkipLatestValueOnSubscribe().AsObservable()
			.Where(dir_ => dir_ == "D" || dir_ == "A")
			.Subscribe(dir_ => {
				transform.Rotate(0, 180.0f, 0);
				Mes.transform.localScale = new Vector3(-Mes.transform.localScale.x, Mes.transform.localScale.y, Mes.transform.localScale.z);
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay() && !!isRClk() && !isMaxStock() && !!enableJump(currentBaseState) && !IsTitle)
			.Subscribe(_ => {
				Gce.checkGroundChip();
				//EffectHiyoko.SetActive(true);
				//HiyokoGo.SetActive(false);
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay() && !!isLClk() && !isEmpty() && currentItemState == ItemState.NoItem)
			.Subscribe(_ => {
				--stock.Value;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay() && !!isLClk() && currentItemState == ItemState.Item1)
			.Subscribe(_ => {
				shotgunLaunch();
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

		this.UpdateAsObservable().Where(x => !!isLClk() && !!IsTitle)
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

		this.UpdateAsObservable().Where(x => !!isSp.Value && !!isShift() && instanceWindGo == null)
			.Subscribe(_ => {
				var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				instanceWindGo = Instantiate(WindGo, new Vector3(mousePos.x, mousePos.y), Quaternion.identity);
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

		this.UpdateAsObservable().Where(x => !!isChange() && !!EnableChange)
			.Subscribe(_ => {
				changeAvatar();
				CamMover.shake();
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

		col.OnTriggerEnterAsObservable().Where(colGo => !!isSignboard(colGo))
			.Subscribe(colGo => {
				contactSignboard = colGo.GetComponent<Tutorial>();
			})
			.AddTo(this);

		col.OnTriggerExitAsObservable().Where(colGo => !!isSignboard(colGo))
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
		anim = GetComponent<Animator>();
		col = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
		orgColHightNormal = col.height;
		orgVectColCenterNormal = col.center;
		orgColHightSD = col.height * 0.7f;
		orgVectColCenterSD = new Vector3(col.center.x, orgVectColCenterNormal.y - orgColHightSD / 4, col.center.z);

		MyBulletLayer = Common.PlayerBulletLayer;

		setHp(Default_Hp);
		//setHp(1000000);

		enableTeleportation = true;
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

		windLifespanSlider = WindLifespanSliderGo.GetComponent<Slider>();
		WindLifespanSliderGo.SetActive(false);

//		EffectHiyoko.SetActive(false);
		//HiyokoGo.SetActive(true);
	}

	/// <summary>
	/// ジャンプキーを押したかどうか
	/// </summary>
	/// <returns>押したらtrue</returns>
	bool isJump()
	{
		return !!Input.GetKeyDown(KeyCode.W);
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
	/// ジャンプ可能かどうか
	/// </summary>
	/// <returns>可能ならtrue</returns>
	bool enableJump(AnimatorStateInfo animStateInfo)
	{
		return animStateInfo.fullPathHash != jumpState;
	}

	/// <summary>
	/// ジャンプ処理
	/// </summary>
	void jump()
	{
		if (!!anim.IsInTransition(0)) {
			return;
		}
		rb.AddForce(Vector3.up * Jump_Power, ForceMode.VelocityChange);
		anim.SetBool("Jump", true);
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
		return !!Input.GetKeyDown(KeyCode.S);
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
	/// プレイヤーのコライダーサイズをリセットする
	/// </summary>
	void resetCollider()
	{
		// コンポーネントのHeight、Centerの初期値を戻す
		col.height = currentAvatar == 0 ? orgColHightNormal : orgColHightSD;
		col.center = currentAvatar == 0 ? orgVectColCenterNormal : orgVectColCenterSD;
	}

	/// <summary>
	/// ジャンプアニメーションのイベント(地面から足が離れる瞬間に呼ばれる)
	/// </summary>
	void OnJumpStart()
	{

	}

	/// <summary>
	/// ジャンプアニメーションのイベント(ジャンプの最高点に到達した瞬間に呼ばれる)
	/// </summary>
	void OnJumpTopPoint()
	{

	}

	/// <summary>
	/// ジャンプアニメーションのイベント(着地時に呼ばれる)
	/// </summary>
	void OnJumpEnd()
	{
		isSp.Value = false;
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
		foreach (Transform tfm in ParticleParents[currentAvatar]) {
			tfm.gameObject.SetActive(true);
		}
		StartCoroutine("teleportation");
		yield return new WaitForSeconds(0.5f);
		enableTeleportation = true;
		foreach (Transform tfm in ParticleParents[currentAvatar]) {
			tfm.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// 瞬間移動する
	/// </summary>
	/// <returns></returns>
	IEnumerator teleportation()
	{
		var power = 200.0f;
		if (dir.Value == "D") {
			rb.AddForce(Vector3.right * power, ForceMode.Impulse);
		} else if (dir.Value == "A") {
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
		return stock.Value <= 0;
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	void launch()
	{
		var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		if (!!IsTitle) {
			launchVec = calcLaunchVec();
		}
		// アシストの数だけ一度に撃つ
		foreach (Transform child in LauncherParent) {
			child.GetComponent<Launcher>().launch(Bullet, mousePos, 13, BulletParentTfm, launchVec);
		}
	}

	/// <summary>
	/// LocusDrawColliderを初期座標に戻し、発射する
	/// </summary>
	public void launchLocusDrawCol()
	{
		StopCoroutine("recordLocus");
		locusPoses.Clear();
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
		if (!!IsTitle) {
			return;
		}
		Lr.positionCount = locusPoses.Count;
		for (var i = 0; i < locusPoses.Count; ++i) {
			Lr.SetPosition(i, locusPoses[i]);
		}
	}

	/// <summary>
	/// ショットガン
	/// </summary>
	void shotgunLaunch()
	{
		var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		var direction = Vector3.zero;
		direction.x = mousePos.x - LaunchTfm.position.x;
		direction.y = mousePos.y - LaunchTfm.position.y;
		direction.z = mousePos.z - LaunchTfm.position.z;
		Vector3[] vecs = new Vector3[3];
		vecs[0] = Vector3.up;
		vecs[1] = Vector3.right;
		vecs[2] = Vector3.down;
		var launchPos = new Vector3(LaunchTfm.position.x + 0.5f, LaunchTfm.position.y - 0.5f, LaunchTfm.position.z);
		for (var i = 0; i < Max_Shotgun_Num; ++i) {
			launchPos.y += 0.2f;
			var go = Instantiate(Bullet, launchPos, Quaternion.identity);
			var correctMagnitude = Mathf.Max(direction.magnitude, 15.0f);
			var magnitude = ShotgunYVecCurve.Evaluate(Mathf.InverseLerp(10.0f, 20.0f, correctMagnitude)) * 10.0f;
			direction = direction.normalized * correctMagnitude;
			var vec = vecs[i] * magnitude + direction;
			go.GetComponent<Rigidbody>().velocity = vec;
			go.layer = Common.PlayerBulletLayer;
		}
	}

	/// <summary>
	/// 地面のチップが消されたら呼ばれる
	/// </summary>
	public void onErased()
	{
		++stock.Value;
//		EffectHiyoko.SetActive(false);
		//HiyokoGo.SetActive(true);
	}

	/// <summary>
	/// ダメージ処理
	/// </summary>
	protected override void damage()
	{
		if (currentItemState != ItemState.NoItem) {
			--ItemDurability;
		} else {
			if (hp.Value <= Default_Hp) {
				base.damage();
				HPGos[hp.Value].SetActive(false);
			}
		}
	}

	/// <summary>
	/// キャラクターの点滅を始める
	/// </summary>
	protected override void startFlick()
	{
		StartCoroutine("flick");
	}

	/// <summary>
	/// キャラクターを点滅させる
	/// </summary>
	/// <returns></returns>
	protected override IEnumerator flick()
	{
		while (true) {
			Models[currentAvatar].SetActive(!Models[currentAvatar].activeSelf);
			yield return new WaitForSeconds(Flick_Interval);
		}
	}

	/// <summary>
	/// 点滅を止める
	/// </summary>
	protected override void stopFlick()
	{
		StopCoroutine("flick");
		Models[currentAvatar].SetActive(true);
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		Gm.gameOver();
		Destroy(gameObject);
	}

	/// <summary>
	/// Avatarを変更する
	/// </summary>
	void changeAvatar()
	{
		currentAvatar = (currentAvatar + 1) % Avatars.Length;
		foreach (GameObject go in Models) {
			go.SetActive(false);
		}
		Models[currentAvatar].SetActive(true);
		anim.avatar = Avatars[currentAvatar];
		if (currentAvatar == 0) {
			col.height = orgColHightNormal;
			col.center = orgVectColCenterNormal;
		} else {
			col.height /= 2;
			col.center = new Vector3(col.center.x, col.center.y - col.height / 2, col.center.z);
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

	protected override void OnCollisionEnter(Collision col)
	{
		base.OnCollisionEnter(col);
		var tag = col.gameObject.tag;
		if (tag.IndexOf("Item") < 0) {
			return;
		}
		var index = System.Convert.ToInt32(tag.Substring(tag.Length - 1, 1));
		ItemImg.sprite = ItemSprites[index - 1];
		Destroy(col.gameObject);
		ItemEffectRemainTxt.text = "残り" + itemDurability.ToString() + "回";
		currentItemState = ItemState.Item1;
		ItemDurability = Default_Item_Durability;
	}
}
