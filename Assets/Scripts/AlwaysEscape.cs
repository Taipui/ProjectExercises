using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// いつでもEscでゲームが終われるようにするためのクラス
/// </summary>
public class AlwaysEscape : MonoBehaviour
{
	void Start ()
	{
		DontDestroyOnLoad(gameObject);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.Escape))
			.Subscribe(_ => {
				Application.Quit();
			})
			.AddTo(this);
	}
}
