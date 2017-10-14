using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;

// 必要なコンポーネントの列記
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]

public class Player : Character
{
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
	const float Use_Curve_Height = 0.5f;

	// 以下キャラクターコントローラ用パラメタ
	/// <summary>
	/// 前進速度
	/// </summary>
	const float Forward_Speed = 7.0f;
	/// <summary>
	/// ジャンプ力
	/// </summary>
	const float Jump_Power = 3.0f;
	/// <summary>
	/// キャラクターのコライダ
	/// </summary>
	CapsuleCollider col;
	/// <summary>
	/// プレイヤーのRidigbody
	/// </summary>
	Rigidbody rb;
	/// <summary>
	/// プレイヤーの移動量
	/// </summary>
	Vector3 velocity;
	/// <summary>
	/// CapsuleColliderで設定されているコライダのHeightの初期値を収める変数
	/// </summary>
	float orgColHight;
	/// <summary>
	/// CapsuleColliderで設定されているコライダのCenterの初期値を収める変数
	/// </summary>
	Vector3 orgVectColCenter;
	/// <summary>
	/// アニメーター
	/// </summary>
	Animator anim;
	/// <summary>
	/// base layerで使われる、アニメーターの現在の状態の参照
	/// </summary>
	AnimatorStateInfo currentBaseState;
	/// <summary>
	/// 横の移動量
	/// </summary>
	readonly ReactiveProperty<float> h = new ReactiveProperty<float>(0.0f);

	// アニメーター各ステートへの参照
	static int idleState = Animator.StringToHash("Base Layer.Idle");
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");
	static int jumpState = Animator.StringToHash("Base Layer.Jump");

	/// <summary>
	/// プレイヤーの向いている方向
	/// </summary>
	readonly ReactiveProperty<string> dir = new ReactiveProperty<string>("D");

	#region 弾関連
	/// <summary>
	/// 弾のストック数
	/// </summary>
	readonly ReactiveProperty<int> stock = new ReactiveProperty<int>(0);
	/// <summary>
	/// 前回のストック数
	/// </summary>
	int prevStock;
	/// <summary>
	/// 弾の最大ストック数
	/// </summary>
	const int Max_Stock = 10;
	/// <summary>
	/// ストック用の弾
	/// </summary>
	[SerializeField]
	GameObject StockBullet;
	/// <summary>
	/// ストック用の弾の親オブジェクトのTransform
	/// </summary>
	[SerializeField]
	Transform StockTfm;
	/// <summary>
	/// 弾の発射位置
	/// </summary>
	[SerializeField]
	Transform LaunchTfm;
	#endregion

	/// <summary>
	/// 特殊能力を発動したかどうか
	/// </summary>
	readonly ReactiveProperty<bool> isSp = new ReactiveProperty<bool>(false);

	/// <summary>
	/// デフォルトの体力
	/// </summary>
	const int Default_Hp = 3;

	/// <summary>
	/// GroundChipErase
	/// </summary>
	[SerializeField]
	GroundChipEraser Gce;
	
	protected override void Start()
	{
		base.Start();
		init();
		var stockBullets = new List<GameObject>();

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
				velocity *= Forward_Speed;
			}

			transform.localPosition += velocity * Time.fixedDeltaTime;
				
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
								col.height = orgColHight - jumpHeight;          // 調整されたコライダーの高さ
								var adjCenterY = orgVectColCenter.y + jumpHeight;
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

		this.UpdateAsObservable().Where(x => (!!isJump() || !!isSpJump()) && !!enableJump())
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

		dir.AsObservable().Where(dir_ => dir_ == "A")
			.Subscribe(dir_ => {
				transform.Rotate(0, 180.0f, 0);
			})
			.AddTo(this);

		dir.SkipLatestValueOnSubscribe().AsObservable()
			.Where(dir_ => dir_ == "D")
			.Subscribe(dir_ => {
				transform.Rotate(0, 180.0f, 0);
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay() && !!isRClk() && !isMaxStock() && !!enableJump())
			.Subscribe(_ => {
				Gce.checkGroundChip();
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay() && !!isLClk() && !isEmpty())
			.Subscribe(_ => {
				--stock.Value;
			})
			.AddTo(this);

		stock.AsObservable().Where(val => val > prevStock)
			.Subscribe(val => {
				var obj = Instantiate(StockBullet, new Vector3(StockTfm.localPosition.x, StockTfm.localPosition.y + 0.03f * val), Quaternion.Euler(0.0f, 90.0f, 0.0f));
				obj.transform.SetParent(StockTfm, false);
				stockBullets.Add(obj);
				prevStock = val;
			})
			.AddTo(this);

		stock.AsObservable().Where(val => val < prevStock)
			.Subscribe(val => {
				var obj = stockBullets[stockBullets.Count - 1];
				stockBullets.RemoveAt(stockBullets.Count - 1);
				Destroy(obj);
				launch();
				prevStock = val;
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
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		anim = GetComponent<Animator>();
		col = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
		orgColHight = col.height;
		orgVectColCenter = col.center;

		prevStock = 0;

		MyBulletLayer = "PlayerBullet";

		setHp(Default_Hp);
	}

	/// <summary>
	/// ジャンプキーを押したかどうか
	/// </summary>
	/// <returns>押したらtrue</returns>
	bool isJump()
	{
		return Input.GetKeyDown(KeyCode.W);
	}

	/// <summary>
	/// 特殊ジャンプキーを押したかどうか
	/// </summary>
	/// <returns>押したらtrue</returns>
	bool isSpJump()
	{
		return Input.GetButtonDown("Jump");
	}

	/// <summary>
	/// ジャンプ可能かどうか
	/// </summary>
	/// <returns>可能ならtrue</returns>
	bool enableJump()
	{
		return currentBaseState.fullPathHash != jumpState;
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
	/// プレイヤーのコライダーサイズをリセットする
	/// </summary>
	void resetCollider()
	{
		// コンポーネントのHeight、Centerの初期値を戻す
		col.height = orgColHight;
		col.center = orgVectColCenter;
	}

	void OnJumpStart()
	{

	}

	void OnJumpTopPoint()
	{

	}

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
		return Input.GetMouseButtonDown(0);
	}

	/// <summary>
	/// 右クリックしたかどうか
	/// </summary>
	/// <returns>クリックしたらtrue</returns>
	bool isRClk()
	{
		return Input.GetMouseButtonDown(1);
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
	protected override void launch()
	{
		var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		var direction = mousePos - LaunchTfm.position;
		foreach (Transform child in LauncherParent) {
			child.GetComponent<Launcher>().launch(Bullet, mousePos, 13, BulletParentTfm, direction.normalized * 40.0f);
		}
	}

	/// <summary>
	/// 地面のチップが消されたら呼ばれる
	/// </summary>
	public void onErased()
	{
		++stock.Value;
	}

	/// <summary>
	/// 死亡処理
	/// </summary>
	protected override void dead()
	{
		Gm.gameOver();
	}
}
