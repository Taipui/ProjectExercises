using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

// 必要なコンポーネントの列記
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]

/// <summary>
/// プレイヤーの移動に関するクラス
/// </summary>
public class PlayerMove : Character
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
	/// コライダの変更を可能にするかどうか
	/// </summary>
	bool canModifyCol;

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

	#region プレイヤーの左右の移動関連

	/// <summary>
	/// 前進速度
	/// </summary>
	const float Forward_Speed = 7.0f;
	/// <summary>
	/// 横の移動量
	/// </summary>
	readonly ReactiveProperty<float> h = new ReactiveProperty<float>(0.0f);

	/// <summary>
	/// プレイヤーの向いている方向
	/// </summary>
	readonly ReactiveProperty<string> dir = new ReactiveProperty<string>("D");
	public string Dir {
		get
		{
			return dir.Value;
		}
	}

	#endregion

	#region プレイヤーのジャンプ関連

	/// <summary>
	/// ジャンプ力
	/// </summary>
	const float Jump_Power = 5.0f;
	/// <summary>
	/// アニメーションのデフォルトの再生速度
	/// </summary>
	float defaultSpeed;
	/// <summary>
	/// 着地判定を調べる回数
	/// </summary>
	const int Landing_Check_Limit = 100;
	/// <summary>
	/// 着地モーションへの移項を許可する距離
	/// </summary>
	const float Landing_Dist = 1.05F;
	//const float Landing_Dist = 2.5f;

	#endregion

	#region コライダ関連

	/// <summary>
	/// キャラクターのコライダ
	/// </summary>
	CapsuleCollider col;
	public CapsuleCollider Col {
		get
		{
			return col;
		}
		set
		{
			col = value;
		}
	}
	/// <summary>
	/// CapsuleColliderで設定されているコライダのHeightの初期値を収める変数(Normal)
	/// </summary>
	public float OrgColHeightNormal { private set; get; }
	/// <summary>
	/// CapsuleColliderで設定されているコライダのCenterの初期値を収める変数(Normal)
	/// </summary>
	public Vector3 OrgVectColCenterNormal { private set; get; }
	/// <summary>
	/// CapsuleColliderで設定されているコライダのHeightの初期値を収める変数(Normal)
	/// </summary>
	float orgColHeightSD;
	/// <summary>
	/// CapsuleColliderで設定されているコライダのCenterの初期値を収める変数(Normal)
	/// </summary>
	Vector3 orgVectColCenterSD;

	#endregion

	/// <summary>
	/// PlayerAct
	/// </summary>
	PlayerAct playerAct;

	/// <summary>
	/// タイトル用かどうか
	/// </summary>
	[SerializeField]
	public bool IsTitle { private set; get; }

	/// <summary>
	/// 現在のステート
	/// </summary>
	AnimatorStateInfo currentBaseState;

	/// <summary>
	/// 着地時のパーティクル
	/// </summary>
	[SerializeField]
	ParticleSystem LandParticle;

	/// <summary>
	/// canInputに値をセットする
	/// </summary>
	/// <param name="val">セットする値</param>
	public void setCanInput(bool val)
	{
		canInput = val;
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		Col = GetComponent<CapsuleCollider>();
		OrgColHeightNormal = Col.height;
		OrgVectColCenterNormal = Col.center;
		orgColHeightSD = Col.height * 0.7f;
		orgVectColCenterSD = new Vector3(Col.center.x, OrgVectColCenterNormal.y - orgColHeightSD / 4, Col.center.z);
		canModifyCol = false;
		canInput = true;
		playerAct = GetComponent<PlayerAct>();
	}

	protected override void Start ()
	{
		base.Start();
		init();
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
		var velocity = Vector3.zero;
		var tfm = transform;

		this.FixedUpdateAsObservable().Subscribe(_ => {
			h.Value = !!canInput ? Input.GetAxis("Horizontal") : 0.0f;
			anim.SetFloat("Speed", Mathf.Abs(h.Value));
			//anim.speed = Anim_Speed;
			currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
			rb.useGravity = true;

			// 以下、キャラクターの移動処理
			velocity = new Vector3(h.Value, 0, 0);
			anim.SetBool("IsIdle", h.Value == 0);

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
					//resetCollider();
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
							//rb.useGravity = false;  //ジャンプ中の重力の影響を切る
						}

						// レイキャストをキャラクターのセンターから落とす
						var ray = new Ray(transform.position + Vector3.up, -Vector3.up);
						var hitInfo = new RaycastHit();
						// 高さが useCurvesHeight 以上ある時のみ、コライダーの高さと中心をJUMP00アニメーションについているカーブで調整する
						if (!!Physics.Raycast(ray, out hitInfo) && !!canModifyCol) {
							if (hitInfo.distance > Use_Curve_Height) {
								var orgColHight = playerAct.CurrentAvatar == 0 ? OrgColHeightNormal : orgColHeightSD;
								var orgColCenter = playerAct.CurrentAvatar == 0 ? OrgVectColCenterNormal : orgVectColCenterSD;
								Col.height = orgColHight - jumpHeight;          // 調整されたコライダーの高さ
								var adjCenterY = orgColCenter.y + jumpHeight;
								var colCenterY = Col.center.y;
								Col.center = new Vector3(0.0f, adjCenterY, 0.0f); // 調整されたコライダーのセンター
							} else {
								// 閾値よりも低い時には初期値に戻す（念のため）
								//resetCollider();
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
					//resetCollider();
				}
			}

			// 段差で浮かないようジャンプ時以外は下方向に力を加える
			if (currentBaseState.fullPathHash != jumpState) {
				rb.AddForce(new Vector3(0.0f, -200.0f));
			}
		})
		.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isJump() && !!enableJump() && !IsTitle)
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
				playerAct.flipMes();
			})
			.AddTo(this);
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
	/// ジャンプ可能かどうか
	/// </summary>
	/// <returns>可能ならtrue</returns>
	public bool enableJump()
	{
		return currentBaseState.fullPathHash != jumpState && !anim.IsInTransition(0);
	}

	/// <summary>
	/// ジャンプ処理
	/// </summary>
	public void jump()
	{
		anim.SetBool("Jump", true);
		StartCoroutine("disableInput");
	}

	/// <summary>
	/// プレイヤーのコライダーサイズをリセットする
	/// </summary>
	void resetCollider()
	{
		// コンポーネントのHeight、Centerの初期値を戻す
		var changeTime = 0.5f;
		var orgColHeight = playerAct.CurrentAvatar == 0 ? OrgColHeightNormal : orgColHeightSD;
		DOTween.To(
			() => Col.height,
			val => Col.height = val,
			orgColHeight,
			changeTime
		);
		//col.height = currentAvatar == 0 ? orgColHeightNormal : orgColHeightSD;
		var colCenterY = playerAct.CurrentAvatar == 0 ? OrgVectColCenterNormal.y : orgVectColCenterSD.y;
		var currentColCenterY = Col.center.y;
		DOTween.To(
			() => currentColCenterY,
			val => currentColCenterY = val,
			colCenterY,
			changeTime
		).OnUpdate(() => {
			Col.center = new Vector3(Col.center.x, currentColCenterY, Col.center.z);
		});
		//col.center = currentAvatar == 0 ? orgVectColCenterNormal : orgVectColCenterSD;
	}

	/// <summary>
	/// ジャンプアニメーションのイベント(地面から足が離れる瞬間に呼ばれる)
	/// </summary>
	void OnJumpStart()
	{
		defaultSpeed = anim.speed;
		rb.AddForce(Vector3.up * Jump_Power, ForceMode.VelocityChange);
	}

	/// <summary>
	/// 脚の曲げ始め
	/// </summary>
	void OnStartLegCurve()
	{
		canModifyCol = true;
	}

	/// <summary>
	/// ジャンプアニメーションのイベント(ジャンプの最高点に到達した瞬間に呼ばれる)
	/// </summary>
	void OnJumpTopPoint()
	{
		// アニメーションを停止して、着地判定のチェックを行う
		anim.speed = 0;
		StartCoroutine(CheckLanding());
		//Debug.Break();
	}

	/// <summary>
	/// 脚の曲げ終わり
	/// </summary>
	void OnEndLegCurve()
	{
		resetCollider();
		canModifyCol = false;
	}

	/// <summary>
	/// ジャンプアニメーションのイベント(着地時に呼ばれる)
	/// </summary>
	void OnJumpEnd()
	{
		playerAct.IsSp = false;
		StartCoroutine("disableInput");
		//Debug.Break();
		LandParticle.Play();
	}

	/// <summary>
	/// 足下との距離を計算して、一定距離まで近づいたらアニメーションを再会させる
	/// </summary>
	/// <returns>The landing.</returns>
	IEnumerator CheckLanding()
	{
		// 規定回数チェックして成功しない場合も着地モーションに移行する
		for (int count = 0; count < Landing_Check_Limit; count++) {
			var raycastL = new RaycastHit();
			var raycastR = new RaycastHit();
			var rayInterval = 0.125f;
			//var rayOffset = 1.7f;
			var rayOffset = 0.0f;
			var rayLen = 1.0f;
			var raycastSuccessL = Physics.Raycast(transform.localPosition + new Vector3(-rayInterval, rayOffset), Vector3.down * rayLen, out raycastL);
			var raycastSuccessR = Physics.Raycast(transform.localPosition + new Vector3(rayInterval, rayOffset), Vector3.down * rayLen, out raycastR);
			//Debug.DrawRay(transform.localPosition + new Vector3(-rayInterval, rayOffset), Vector3.down * rayLen, Color.red);
			//Debug.DrawRay(transform.localPosition + new Vector3(rayInterval, rayOffset), Vector3.down * rayLen, Color.red);
			//Debug.Log(raycastL.distance);
			//Debug.Log(anim.speed);
			//Debug.Break();
			// レイを飛ばして、成功且つ一定距離内であった場合、着地モーションへ移項させる
			if ((!!raycastSuccessL || !!raycastSuccessR) && (raycastL.distance < Landing_Dist || raycastR.distance < Landing_Dist)) {
				//Debug.Break();
				break;
			}
			//yield return new WaitForSeconds(waitTime);
			yield return new WaitForEndOfFrame();
		}
		anim.speed = defaultSpeed;
		//Debug.Log("break");
		//Debug.Break();
	}
}
