using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 球状の風を発生させるクラス
/// </summary>
public class Wind : MonoBehaviour
{
	/// <summary>
	/// 絵を回転させる速度
	/// </summary>
	const float Rotate_Speed = 200.0f;

	/// <summary>
	/// 吸引力
	/// </summary>
	const float Turbulence = 100.0f;

	/// <summary>
	/// Main
	/// </summary>
	Main main;

	/// <summary>
	/// Title
	/// </summary>
	TitleBase title;

	/// <summary>
	/// Mainをセット
	/// </summary>
	/// <param name="main_">セットするMain</param>
	public void setMain(Main main_)
	{
		main = main_;
	}

	/// <summary>
	/// Titleをセット
	/// </summary>
	/// <param name="title_">セットするTitle</param>
	public void setTitle(TitleBase title_)
	{
		title = title_;
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		if (main != null) {
			main.playSE(Main.SE.Wind, GetComponent<AudioSource>());
		} else if (title != null) {
			title.playSE(TitleBase.SE.Wind);
		}
	}

	void Start ()
	{
		init();

		var col = GetComponent<SphereCollider>();

		//this.UpdateAsObservable().Subscribe(_ => {
		//	transform.Rotate(new Vector3(0.0f, 0.0f, Rotate_Speed * Time.deltaTime));
		//})
		//.AddTo(this);

		col.OnTriggerStayAsObservable().Where(colGo => !!isBullet(colGo))
			.Subscribe(colGo => {
				colGo.gameObject.GetComponent<Rigidbody>().AddForce((transform.position - colGo.transform.position) * Turbulence);
				colGo.gameObject.layer = Common.BulletLayer;
			})
			.AddTo(this);
	}

	/// <summary>
	/// 当たったものが弾かどうか
	/// </summary>
	/// <param name="col">当たったもの</param>
	/// <returns>弾ならtrue</returns>
	bool isBullet(Collider col)
	{
		return col.tag == "Bullet";
	}
}
