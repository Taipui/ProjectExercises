//
//Player.cs
//プレイヤーに関するスクリプト
//2017/10/4 Taipui
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

public class Player : Character
{
	/// <summary>
	/// 発射する弾
	/// </summary>
	[SerializeField]
	GameObject Bullet;

	/// <summary>
	/// 弾の発射位置
	/// </summary>
	[SerializeField]
	Transform LaunchTfm;

	#region PlayerCollider関連
	/// <summary>
	/// PlayerColliderへの参照
	/// </summary>
	[SerializeField]
	PlayerCollider Pc;
	/// <summary>
	/// PlayerColliderのRigidbody2D
	/// </summary>
	[SerializeField]
	Rigidbody2D rb;
	/// <summary>
	/// 上半身のCollider
	/// </summary>
	[SerializeField]
	BoxCollider2D BodyCollider;
	/// <summary>
	/// 下半身のCollider
	/// </summary>
	[SerializeField]
	CircleCollider2D LegCollider;
	/// <summary>
	/// PlayerCollidersのTransform
	/// </summary>
	[SerializeField]
	Transform PlayerCollidersTfm;
	#endregion

	/// <summary>
	/// プレイヤーの移動速度
	/// </summary>
	const float Move_Speed = 2.0f;

	/// <summary>
	/// プレイヤーが回転する時のTween
	/// </summary>
	Tweener rotateTween;

	/// <summary>
	/// プレイヤーの向き
	/// </summary>
	string dir;
	/// <summary>
	/// プレイヤーの前回までの向き
	/// </summary>
	string dirOld;

	/// <summary>
	/// プレイヤーが回転中かどうか
	/// </summary>
	bool isRotating;


	/// <summary>
	/// プレイヤーのジャンプ力
	/// </summary>
	const float Jump_Power = 5.0f;

	/// <summary>
	/// アニメーションのデフォルトの再生速度
	/// </summary>
	private float defaultSpeed;
	/// <summary>
	/// 着地判定を調べる回数
	/// </summary>
	private readonly int landingCheckLimit = 100;
	/// <summary>
	/// 着地判定チェックを行う時間間隔
	/// </summary>
	private readonly float waitTime = 0.05F;
	/// <summary>
	/// 着地モーションへの移項を許可する距離
	/// </summary>
	private readonly float landingDistance = 0.3f;

	/// <summary>
	/// プレイヤーのアニメーター
	/// </summary>
	Animator anim;

	/// <summary>
	/// ジャンプ時にプレイヤーのColliderを上方向にずらす量
	/// </summary>
	const float Is_Jumping_Collider_Height_Offset = 0.7f;

	protected override void Start ()
	{
		base.Start();

		dir = "D";
		dirOld = dir;
		isRotating = false;

		anim = GetComponent<Animator>();

		isJumping = false;

		var currentSpeed = 0.0f;

		this.FixedUpdateAsObservable().Subscribe(_ => {
			anim.SetFloat("Speed", Mathf.Abs(currentSpeed));
			if (!isJumping) {
				anim.speed = 1.5f;
			}

			var velocity = new Vector3(currentSpeed, 0);

			velocity *= Move_Speed;

			CharacterColliderTfm.position += velocity * Time.fixedDeltaTime;

			//if (!isJumping) {
			//	CharacterColliderTfm.localPosition += velocity * Time.fixedDeltaTime;
			//} else {
			//	transform.localPosition += velocity * Time.fixedDeltaTime;
			//}
		})
		.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.A))
			.Subscribe(_ => {
				anim.SetBool("IsIdle", false);
				dir ="A";
				rotate();
				currentSpeed = -1.0f;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.D))
			.Subscribe(_ => {
				anim.SetBool("IsIdle", false);
				dir = "D";
				rotate();
				currentSpeed = 1.0f;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyUp(KeyCode.A) || !!Input.GetKeyUp(KeyCode.D))
			.Subscribe(_ => {
				anim.SetBool("IsIdle", true);
				currentSpeed = 0.0f;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.W))
			.Subscribe(_ => {
				StartCoroutine(jump(anim));
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetMouseButtonDown(0))
			.Subscribe(_ => {
				launch();
			})
			.AddTo(this);

		this.FixedUpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.Space))
			.Subscribe(_ => {
				StartCoroutine(jump(anim));
			})
			.AddTo(this);

		this.UpdateAsObservable().Subscribe(_ => {
//			Debug.Log(isJumping);
		})
		.AddTo(this);
	}

	/// <summary>
	/// プレイヤーの向きを変える
	/// </summary>
	void rotate()
	{
		if (dir == dirOld) {
			return;
		}
		if (!!isRotating) {
			return;
		}
		isRotating = true;
		var rotateVal = dir == "D" ? 90.0f : -90.0f;
		if (rotateTween != null) {
			rotateTween.Kill();
		}
		rotateTween = transform.DORotate(
			new Vector3(0.0f, rotateVal),
			0.1f
			).OnComplete(() => {
				dirOld = dir;
				isRotating = false;
		});
	}

	/// <summary>
	/// 弾を発射する
	/// </summary>
	void launch()
	{
		var obj = Instantiate(Bullet, LaunchTfm.position, Quaternion.identity);
		var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		var direction = mousePos - LaunchTfm.position;
		obj.GetComponent<Rigidbody2D>().velocity = direction.normalized * 40.0f;
		Pc.eraseGroundChip();
	}

	/// <summary>
	/// ジャンプする
	/// </summary>
	IEnumerator jump(Animator anim)
	{
		if (!!isJumping) {
			yield return null;
		}
		isJumping = true;
		anim.SetTrigger("Jump");

		//yield return new WaitForSeconds(0.3f);

		//var jumpAnimCoroutine = StartCoroutine(waitJumpAnimEnd(anim));
		//yield return jumpAnimCoroutine;
		//isJumping = false;
	}

	/// <summary>
	/// ジャンプのアニメーション終了通知
	/// </summary>
	/// <param name="anim"></param>
	/// <returns></returns>
	IEnumerator waitJumpAnimEnd(Animator anim)
	{
		while (true) {
			var nowState = anim.GetCurrentAnimatorStateInfo(0);
			if (nowState.IsName("Jump") && nowState.normalizedTime >= 0.9f) {
				yield break;
			} else {
				yield return new WaitForSeconds(0.1f);
			}
		}
	}

	/// <summary>
	/// ジャンプモーションで、足が離れる瞬間に呼び出されるメソッド
	/// </summary>
	void OnJumpStart()
	{
		defaultSpeed = anim.speed;
		// キャラクターをジャンプさせる
		rb.AddForce(Vector3.up * Jump_Power, ForceMode2D.Impulse);
//		LegCollider.enabled = false;
//		BodyCollider.offset = new Vector2(BodyCollider.offset.x, BodyCollider.offset.y + Is_Jumping_Collider_Height_Offset);
//		LegCollider.offset = new Vector2(LegCollider.offset.x, LegCollider.offset.y + Is_Jumping_Collider_Height_Offset);
	}

	/// <summary>
	/// ジャンプモーションで、頂点のフレームで呼び出されるメソッド
	/// </summary>
	void OnJumpTopPoint()
	{
		// アニメーションを停止して、着地判定のチェックを行う
		anim.speed = 0;
		StartCoroutine(CheckLanding());
	}

	/// <summary>
	/// ジャンプモーションで、足が地上に着いたときに呼ばれるメソッド
	/// </summary>
	void OnJumpEnd()
	{
//		BodyCollider.offset = new Vector2(BodyCollider.offset.x, BodyCollider.offset.y - Is_Jumping_Collider_Height_Offset);
		isJumping = false;
	}

	/// <summary>
	/// 足下との距離を計算して、一定距離まで近づいたらアニメーションを再開させる
	/// </summary>
	/// <returns>The landing.</returns>
	IEnumerator CheckLanding()
	{
		// 規定回数チェックして成功しない場合も着地モーションに移行する
		for (int count = 0; count < landingCheckLimit; count++) {
			var raycast = Physics2D.Raycast(transform.position, Vector2.down);
			// レイを飛ばして、成功且つ一定距離内であった場合、着地モーションへ移行させる
			if (!!raycast && raycast.distance < landingDistance) {
				break;
			}
			yield return new WaitForSeconds(waitTime);
		}
		anim.speed = defaultSpeed;
		//yield return new WaitForSecondsRealtime(0.005f);
		//LegCollider.offset = new Vector2(LegCollider.offset.x, LegCollider.offset.y - Is_Jumping_Collider_Height_Offset);
		//LegCollider.enabled = true;

	}
}
