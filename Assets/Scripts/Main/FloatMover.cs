using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// アシストをふわふわさせるクラス
/// </summary>
public class FloatMover : MonoBehaviour
{
	void Start ()
	{
		transform.Translate(new Vector3(0.0f, -0.2f, 0.0f));

		this.UpdateAsObservable().Subscribe(_ => {
			transform.Translate(new Vector3(0.0f, Mathf.Sin(Time.time) / 150, 0.0f));
		})
		.AddTo(this);
	}
}
