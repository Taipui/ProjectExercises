﻿//
// Mecanimのアニメーションデータが、原点で移動しない場合の Rigidbody付きコントローラ
// サンプル
// 2014/03/13 N.Kobyasahi
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;

namespace UnityChan
{
	// 必要なコンポーネントの列記
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Rigidbody))]

	public class Player : Character
	{

		public float animSpeed = 1.5f;              // アニメーション再生速度設定
		public float lookSmoother = 3.0f;           // a smoothing setting for camera motion
		public bool useCurves = true;               // Mecanimでカーブ調整を使うか設定する
													// このスイッチが入っていないとカーブは使われない
		public float useCurvesHeight = 0.5f;        // カーブ補正の有効高さ（地面をすり抜けやすい時には大きくする）

		// 以下キャラクターコントローラ用パラメタ
		// 前進速度
		public float forwardSpeed = 7.0f;
		// 後退速度
		public float backwardSpeed = 2.0f;
		// 旋回速度
		public float rotateSpeed = 2.0f;
		// ジャンプ威力
		public float jumpPower = 3.0f;
		// キャラクターコントローラ（カプセルコライダ）の参照
		private CapsuleCollider col;
		private Rigidbody rb;
		// キャラクターコントローラ（カプセルコライダ）の移動量
		private Vector3 velocity;
		// CapsuleColliderで設定されているコライダのHeiht、Centerの初期値を収める変数
		private float orgColHight;
		private Vector3 orgVectColCenter;
		private Animator anim;                          // キャラにアタッチされるアニメーターへの参照
		private AnimatorStateInfo currentBaseState;         // base layerで使われる、アニメーターの現在の状態の参照

		private GameObject cameraObject;    // メインカメラへの参照

		// アニメーター各ステートへの参照
		static int idleState = Animator.StringToHash("Base Layer.Idle");
		static int locoState = Animator.StringToHash("Base Layer.Locomotion");
		static int jumpState = Animator.StringToHash("Base Layer.Jump");
		static int restState = Animator.StringToHash("Base Layer.Rest");

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

		/// <summary>
		/// プレイヤーのCollider
		/// </summary>
		CapsuleCollider playerCollider;

		// 初期化
		protected override void Start()
		{
			base.Start();

			// Animatorコンポーネントを取得する
			anim = GetComponent<Animator>();
			// CapsuleColliderコンポーネントを取得する（カプセル型コリジョン）
			col = GetComponent<CapsuleCollider>();
			rb = GetComponent<Rigidbody>();
			//メインカメラを取得する
			cameraObject = GameObject.FindWithTag("MainCamera");
			// CapsuleColliderコンポーネントのHeight、Centerの初期値を保存する
			orgColHight = col.height;
			orgVectColCenter = col.center;

			prevStock = 0;
			var stockBullets = new List<GameObject>();

			MyBulletTag = "PlayerBullet";

			setHp(Default_Hp);

			this.FixedUpdateAsObservable().Subscribe(_ => {
				float h = Input.GetAxis("Horizontal");              // 入力デバイスの水平軸をhで定義
				float v = Input.GetAxis("Vertical");                // 入力デバイスの垂直軸をvで定義
				anim.SetFloat("Speed", Mathf.Abs(h));                          // Animator側で設定している"Speed"パラメタにvを渡す
//				anim.SetFloat("Direction", h);                      // Animator側で設定している"Direction"パラメタにhを渡す
				anim.speed = animSpeed;                             // Animatorのモーション再生速度に animSpeedを設定する
				currentBaseState = anim.GetCurrentAnimatorStateInfo(0); // 参照用のステート変数にBase Layer (0)の現在のステートを設定する
				rb.useGravity = true;//ジャンプ中に重力を切るので、それ以外は重力の影響を受けるようにする



				// 以下、キャラクターの移動処理
				velocity = new Vector3(h, 0, 0);        // 上下のキー入力からZ軸方向の移動量を取得

				//以下のvの閾値は、Mecanim側のトランジションと一緒に調整する
				if (Mathf.Abs(h) > 0.1) {
					velocity *= forwardSpeed;       // 移動速度を掛ける
				}

				if (Input.GetButtonDown("Jump")) {  // スペースキーを入力したら
					isSp.Value = true;
					if (currentBaseState.nameHash != jumpState) {
						//ステート遷移中でなかったらジャンプできる
						if (!anim.IsInTransition(0)) {
							rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
							anim.SetBool("Jump", true);     // Animatorにジャンプに切り替えるフラグを送る
						}
					}

				}

				if (Input.GetKeyDown(KeyCode.W)) {
					if (currentBaseState.nameHash != jumpState) {
						//ステート遷移中でなかったらジャンプできる
						if (!anim.IsInTransition(0)) {
							rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
							anim.SetBool("Jump", true);     // Animatorにジャンプに切り替えるフラグを送る
						}
					}

				}

				transform.localPosition += velocity * Time.fixedDeltaTime;

				if (h > 0) {
					dir.Value = "D";
				} else if (h < 0) {
					dir.Value = "A";
				}
				
				// 以下、Animatorの各ステート中での処理
				// Locomotion中
				// 現在のベースレイヤーがlocoStateの時
				if (currentBaseState.nameHash == locoState) {
					//カーブでコライダ調整をしている時は、念のためにリセットする
					if (useCurves) {
						resetCollider();
					}
				}
			// JUMP中の処理
			// 現在のベースレイヤーがjumpStateの時
			else if (currentBaseState.nameHash == jumpState) {
																			// ステートがトランジション中でない場合
					if (!anim.IsInTransition(0)) {

						// 以下、カーブ調整をする場合の処理
						if (useCurves) {
							// 以下JUMP00アニメーションについているカーブJumpHeightとGravityControl
							// JumpHeight:JUMP00でのジャンプの高さ（0〜1）
							// GravityControl:1⇒ジャンプ中（重力無効）、0⇒重力有効
							float jumpHeight = anim.GetFloat("JumpHeight");
							float gravityControl = anim.GetFloat("GravityControl");
							if (gravityControl > 0)
								rb.useGravity = false;  //ジャンプ中の重力の影響を切る

							// レイキャストをキャラクターのセンターから落とす
							Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
							RaycastHit hitInfo = new RaycastHit();
							// 高さが useCurvesHeight 以上ある時のみ、コライダーの高さと中心をJUMP00アニメーションについているカーブで調整する
							if (Physics.Raycast(ray, out hitInfo)) {
								if (hitInfo.distance > useCurvesHeight) {
									col.height = orgColHight - jumpHeight;          // 調整されたコライダーの高さ
									float adjCenterY = orgVectColCenter.y + jumpHeight;
									col.center = new Vector3(0, adjCenterY, 0); // 調整されたコライダーのセンター
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
			else if (currentBaseState.nameHash == idleState) {
					//カーブでコライダ調整をしている時は、念のためにリセットする
					if (useCurves) {
						resetCollider();
					}
				}
			// REST中の処理
			// 現在のベースレイヤーがrestStateの時
			else if (currentBaseState.nameHash == restState) {
					//cameraObject.SendMessage("setCameraPositionFrontView");		// カメラを正面に切り替える
					// ステートが遷移中でない場合、Rest bool値をリセットする（ループしないようにする）
					if (!anim.IsInTransition(0)) {
						anim.SetBool("Rest", false);
					}
				}
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

			this.UpdateAsObservable().Where(x => Gm.CurrentGameState == GameManager.GameState.Play && !!Input.GetMouseButtonDown(1) && stock.Value < Max_Stock && currentBaseState.nameHash != jumpState)
				.Subscribe(_ => {
					Gce.checkGroundChip();
				})
				.AddTo(this);

			this.UpdateAsObservable().Where(x => Gm.CurrentGameState == GameManager.GameState.Play && !!Input.GetMouseButtonDown(0) && stock.Value > 0)
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


		// 以下、メイン処理.リジッドボディと絡めるので、FixedUpdate内で処理を行う.

		// キャラクターのコライダーサイズのリセット関数
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
		/// 弾を発射する
		/// </summary>
		protected override void launch()
		{
			var obj = Instantiate(Bullet, LaunchTfm.position, Quaternion.identity);
			var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			var direction = mousePos - LaunchTfm.position;
			obj.GetComponent<Rigidbody>().velocity = direction.normalized * 40.0f;
			obj.transform.SetParent(BulletParentTfm);
			obj.tag = MyBulletTag;
		}

		/// <summary>
		/// 地面のチップが消されたら呼ばれる
		/// </summary>
		public override void onErased()
		{
			++stock.Value;
		}

		void OnCollisionEnter(Collision collision)
		{

			chechBullet(collision.gameObject);

		}

		/// <summary>
		/// 死亡処理
		/// </summary>
		protected override void dead()
		{
			Gm.gameOver();
		}
	}
}