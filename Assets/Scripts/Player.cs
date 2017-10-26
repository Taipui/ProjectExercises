using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;

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

	#region 弾関連
	/// <summary>
	/// 弾のストック数
	/// </summary>
	readonly ReactiveProperty<int> stock = new ReactiveProperty<int>(0);
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
	GameObject WindObj;
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
		GameObject instanceWindObj = null;
		Tutorial contactSignboard = null;
		var tfm = transform;
		var cachedLocalPosition = tfm.localPosition;

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
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isPlay() && !!isLClk() && !isEmpty())
			.Subscribe(_ => {
				--stock.Value;
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

		this.UpdateAsObservable().Where(x => !!isSp.Value && !!isShift() && instanceWindObj == null)
			.Subscribe(_ => {
				var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				instanceWindObj = Instantiate(WindObj, new Vector3(mousePos.x, mousePos.y), Quaternion.identity);
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isChange())
			.Subscribe(_ => {
				changeAvatar();
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

		//setHp(1000000);
		setHp(Default_Hp);

		enableTeleportation = true;
		foreach (Transform parentTfm in ParticleParents) {
			if (parentTfm == null) {
				return;
			}
			foreach (Transform childTfm in parentTfm) {
				childTfm.gameObject.SetActive(false);
			}
		}
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
		var direction = Vector3.zero;
		direction.x = mousePos.x - LaunchTfm.position.x;
		direction.y = mousePos.y - LaunchTfm.position.y;
		direction.z = mousePos.z - LaunchTfm.position.z;
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
		Destroy(gameObject);
	}

	/// <summary>
	/// Avatarを変更する
	/// </summary>
	void changeAvatar()
	{
		if (!EnableChange) {
			return;
		}
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

	/// <summary>
	/// ダメージ処理
	/// </summary>
	protected override void damage()
	{
		base.damage();
		HPGos[hp.Value].SetActive(false);
	}
}
