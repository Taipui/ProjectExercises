using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

/// <summary>
/// プレイヤーに関するクラス
/// </summary>
public class Player : Character
{
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
	Rigidbody2D Rb;
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

	#region 方向転換関係
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
	#endregion

	/// <summary>
	/// プレイヤーのアニメーター
	/// </summary>
	Animator anim;

	#region ジャンプ関連
	/// <summary>
	/// プレイヤーのジャンプ力
	/// </summary>
	const float Jump_Power = 5.0f;
	/// <summary>
	/// 着地判定を調べる回数
	/// </summary>
	private readonly int landingCheckLimit = 1000;
	/// <summary>
	/// 着地判定チェックを行う時間間隔
	/// </summary>
	private readonly float waitTime = 0.01f;
	/// <summary>
	/// 着地モーションへの移項を許可する距離
	/// </summary>
	private readonly float landingDistance = 0.62f;
	/// <summary>
	/// ジャンプ時にプレイヤーのColliderを上方向にずらす量
	/// </summary>
	const float Is_Jumping_Collider_Height_Offset = 0.7f;
	#endregion

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

	protected override void Start ()
	{
		base.Start();

		dir = "D";
		dirOld = dir;
		isRotating = false;

		anim = GetComponent<Animator>();

		isJumping = false;

		var currentSpeed = 0.0f;

		prevStock = 0;

		List<GameObject> StockBullets = new List<GameObject>();

		this.FixedUpdateAsObservable().Subscribe(_ => {
			anim.SetFloat("Speed", Mathf.Abs(currentSpeed));
			if (!isJumping) {
				anim.speed = 1.5f;
			}

			var velocity = new Vector3(currentSpeed, 0);

			velocity *= Move_Speed;

			CharacterColliderTfm.position += velocity * Time.fixedDeltaTime;
		})
		.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.A))
			.Subscribe(_ => {
				currentSpeed = changeDir("A");
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.D))
			.Subscribe(_ => {
				currentSpeed = changeDir("D");
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
			.Subscribe(_ => {
				anim.SetBool("IsIdle", true);
				currentSpeed = 0.0f;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyUp(KeyCode.A) && !!Input.GetKey(KeyCode.D))
			.Subscribe(_ => {
				currentSpeed = changeDir("D");
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyUp(KeyCode.D) && !!Input.GetKey(KeyCode.A))
			.Subscribe(_ => {
				currentSpeed = changeDir("A");
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.W))
			.Subscribe(_ => {
				StartCoroutine(jump(anim));
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetMouseButtonDown(0) && stock.Value > 0)
			.Subscribe(_ => {
				--stock.Value;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetMouseButtonDown(1) && stock.Value < Max_Stock && !isJumping)
			.Subscribe(_ => {
				Gce.checkGroundChip();
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.Space))
			.Subscribe(_ => {
				StartCoroutine(jump(anim));
			})
			.AddTo(this);

		this.UpdateAsObservable().Subscribe(_ => {
		})
		.AddTo(this);

		this.UpdateAsObservable().Where(x => !!isJumping)
			.Subscribe(_ => {

			})
			.AddTo(this);

		stock.AsObservable().Where(val => val > prevStock)
			.Subscribe(val => {
				var obj = Instantiate(StockBullet, new Vector3(StockTfm.localPosition.x, StockTfm.localPosition.y + 0.03f * val), Quaternion.Euler(0.0f, 90.0f, 0.0f));
				obj.transform.SetParent(StockTfm, false);
				StockBullets.Add(obj);
				prevStock = val;
			})
			.AddTo(this);

		stock.AsObservable().Where(val => val < prevStock)
			.Subscribe(val => {
				var obj = StockBullets[StockBullets.Count - 1];
				StockBullets.RemoveAt(StockBullets.Count - 1);
				Destroy(obj);
				launch();
				prevStock = val;
			})
			.AddTo(this);
	}
	
	/// <summary>
	/// 方向転換の処理
	/// </summary>
	/// <param name="dir_">向く方向</param>
	/// <returns>移動速度</returns>
	float changeDir(string dir_)
	{
		anim.SetBool("IsIdle", false);
		dir = dir_;
		rotate();
		return dir_ == "A" ? -1.0f : 1.0f;
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
	protected override void launch()
	{
		var obj = Instantiate(Bullet, LaunchTfm.position, Quaternion.identity);
		var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		var direction = mousePos - LaunchTfm.position;
		obj.GetComponent<Rigidbody2D>().velocity = direction.normalized * 40.0f;
		obj.transform.SetParent(BulletParentTfm);
	}

	/// <summary>
	/// ジャンプする
	/// </summary>
	IEnumerator jump(Animator anim)
	{
		if (!!isJumping) {
			yield break;
		}
		isJumping = true;
		anim.SetTrigger("Jump");
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
		// キャラクターをジャンプさせる
		Rb.AddForce(Vector3.up * Jump_Power, ForceMode2D.Impulse);
	}

	/// <summary>
	/// ジャンプモーションで、頂点のフレームで呼び出されるメソッド
	/// </summary>
	void OnJumpTopPoint()
	{
		// アニメーションを停止して、着地判定のチェックを行う
		anim.speed = 0.0f;
		StartCoroutine(checkLanding());
	}

	/// <summary>
	/// ジャンプモーションで、足が地上に着いたときに呼ばれるメソッド
	/// </summary>
	void OnJumpEnd()
	{
		isJumping = false;
	}

	/// <summary>
	/// 足下との距離を計算して、一定距離まで近づいたらアニメーションを再開させる
	/// </summary>
	/// <returns>The landing.</returns>
	IEnumerator checkLanding()
	{
		// 規定回数チェックして成功しない場合も着地モーションに移行する
		for (var cnt = 0; cnt < landingCheckLimit; ++cnt) {
			var offsetPos = new Vector2(transform.position.x, transform.position.y - 0.1f);
			var raycast = Physics2D.Raycast(offsetPos, Vector2.down);
			// レイを飛ばして、成功且つ一定距離内であった場合、着地モーションへ移行させる
			if (!!raycast && raycast.distance < landingDistance) {
				break;
			}
			yield return null;
		}
		anim.speed = 1.0f;
	}

	/// <summary>
	/// 地面のチップが消されたら呼ばれる
	/// </summary>
	public override void onErased()
	{
		++stock.Value;
	}
}
