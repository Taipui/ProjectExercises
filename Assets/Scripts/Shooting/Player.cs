using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

public class Player : MonoBehaviour
{
	[SerializeField]
	Transform PlayerColliderTfm;

	[SerializeField]
	GameObject Bullet;

	[SerializeField]
	Transform LaunchTfm;

	readonly ReactiveProperty<float> dx = new ReactiveProperty<float>(0.0f);
	readonly ReactiveProperty<float> dy = new ReactiveProperty<float>(0.0f);

	readonly ReactiveProperty<float> rotateVal = new ReactiveProperty<float>(0.0f);

	const float Rotate_Speed = 1.0f;

	const float Move_Speed = 1.5f;

	Tweener rotateTween;

	string dir;
	string dirOld;

	bool isRotating;

	void Start ()
	{
		dir = "D";
		dirOld = dir;
		isRotating = false;

		var anim = GetComponent<Animator>();

		this.UpdateAsObservable().Subscribe(_ => {
			dx.Value = Input.GetAxis("Horizontal");
			dy.Value = Input.GetAxis("Vertical");

			anim.SetFloat("Speed", Mathf.Abs(dx.Value));
			anim.speed = 1.5f;

			PlayerColliderTfm.Translate(new Vector3(dx.Value * Time.deltaTime * Move_Speed, 0.0f));

			transform.position = PlayerColliderTfm.position;

			Debug.Log("dir:" + dir + ", dirOld:" + dirOld);
//			Debug.Log(dx);
		})
		.AddTo(this);

		dx.AsObservable().Where(val => val != 0.0f)
			.Subscribe(val => {
				if (val < 0) {
					dir = "A";
					rotate();
				} else if (val > 0) {
					dir = "D";
					rotate();
				}
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetMouseButtonDown(0))
			.Subscribe(_ => {
				var obj = Instantiate(Bullet, LaunchTfm.position, Quaternion.identity);
				var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				var direction = mousePos - LaunchTfm.position;
				obj.GetComponent<Rigidbody2D>().velocity = direction.normalized * 50.0f;
			})
			.AddTo(this);
	}

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
}

