using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;
using System.Collections;
using System.Text;
using DG.Tweening;

/// <summary>
/// スタッフロールに関するクラス
/// </summary>
public class StaffRoll : MonoBehaviour
{
	#region スタッフロール関連

	/// <summary>
	/// 最初の文字のX座標
	/// </summary>
	const float First_Word_Pos_X = 30.0f;
	/// <summary>
	/// 文字の間隔
	/// </summary>
	const float Word_Interval = 0.1f;
	/// <summary>
	/// 文の間隔
	/// </summary>
	const float Txt_Interval = 10.0f;
	/// <summary>
	/// 2行目のインデント
	/// </summary>
	const float Indent = 2.0f;
	///// <summary>
	///// センターのY座標
	///// </summary>
	//const float Center_Pos_Y = 5.0f;
	///// <summary>
	///// 行間隔
	///// </summary>
	//const float Row_Interval = 2.0f;
	//const float 

	/// <summary>
	/// 1行目のY座標
	/// </summary>
	const float Line_1_Pos_Y = 7.0f;
	/// <summary>
	/// 2行目のY座標
	/// </summary>
	const float Line_2_Pos_Y = 5.0f;

	/// <summary>
	/// EndTextのカウント
	/// </summary>
	readonly ReactiveProperty<int> endTxtCnt = new ReactiveProperty<int>(0);

	/// <summary>
	/// 全角文字のチェックに使用
	/// </summary>
	Encoding enc = Encoding.GetEncoding("UTF-16");

	/// <summary>
	/// EndTextの文字数
	/// </summary>
	int endTxtLen;

	/// <summary>
	/// フェード用の画像
	/// </summary>
	[SerializeField]
	Image FadePanel;

	/// <summary>
	/// メインのCanvasを消してからも黒画面にするためのCanvasのGameObject
	/// </summary>
	[SerializeField]
	GameObject BlackCanvasGo;

	/// <summary>
	/// テキストの親のTransform
	/// </summary>
	[SerializeField]
	Transform TxtsParent;

	#endregion

	#region SE関連

	/// <summary>
	/// SEのAudioSource
	/// </summary>
	[SerializeField]
	AudioSource SEAudioSource;
	/// <summary>
	/// SE
	/// </summary>
	public enum SE
	{
		/// <summary>
		/// 雪弾がキャラクターに当たった時のSE
		/// </summary>
		Hit,
		/// <summary>
		/// アイテムを取得した時のSE
		/// </summary>
		Item,
		/// <summary>
		/// 敵を倒した時のSE
		/// </summary>
		Kill,
		/// <summary>
		/// 雪弾の発射時のSE
		/// </summary>
		Launch,
		/// <summary>
		/// ショットガンの発射時のSE
		/// </summary>
		ShotgunLaunch,
	}
	/// <summary>
	/// SEの配列
	/// </summary>
	[SerializeField]
	AudioClip[] SEs;

	#endregion

	#region オプション関連

	/// <summary>
	/// オプション画面
	/// </summary>
	[SerializeField]
	GameObject OptionCanvasGo;
	/// <summary>
	/// オプションを開く前のtimeScale;
	/// </summary>
	float prevTimeScale;

	#endregion

	/// <summary>
	/// プレイヤーのアクションに関連するスクリプト
	/// </summary>
	[SerializeField]
	StaffRollPlayerAct Player;

	/// <summary>
	/// 初期化
	/// </summary>
	void init()
	{
		var audioMixer = SEAudioSource.outputAudioMixerGroup.audioMixer;

		audioMixer.SetFloat("MasterVol", Mathf.Lerp(-80.0f, 0.0f, PlayerPrefs.GetFloat("Master", 100) / 100));
		audioMixer.SetFloat("BGMVol", Mathf.Lerp(-80.0f, 0.0f, PlayerPrefs.GetFloat("BGM", 100) / 100));
		audioMixer.SetFloat("SEVol", Mathf.Lerp(-80.0f, 0.0f, PlayerPrefs.GetFloat("SE", 100) / 100));

		OptionCanvasGo.SetActive(false);

		BlackCanvasGo.SetActive(false);

		endTxtLen = 1;

		//Player.transform.position = new Vector3(1000.0f, Player.transform.position.y, Player.transform.position.z);
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void Start()
	{
		init();
		StartCoroutine(createWords(createStrArray()));

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.O))
			.Subscribe(_ => {
				if (!OptionCanvasGo.activeSelf) {
					OptionCanvasGo.SetActive(true);
					prevTimeScale = Time.timeScale;
					Time.timeScale = 0.0f;
					Player.setCanInput(false);
				} else {
					OptionCanvasGo.SetActive(false);
					Time.timeScale = prevTimeScale;
					Player.setCanInput(true);
				}
			})
			.AddTo(this);

		endTxtCnt.AsObservable().Where(val => val >= endTxtLen)
			.Subscribe(_ => {
				end();
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.Q))
			.First()
			.Subscribe(_ => {
				end();
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyDown(KeyCode.Space))
			.Subscribe(_ => {
				Time.timeScale = 2.0f;
			})
			.AddTo(this);

		this.UpdateAsObservable().Where(x => !!Input.GetKeyUp(KeyCode.Space))
			.Subscribe(_ => {
				Time.timeScale = 1.0f;
			})
			.AddTo(this);
	}

	/// <summary>
	/// 文のリストを作成
	/// </summary>
	/// <returns>文をまとめた配列</returns>
	string[] createStrArray()
	{
		var txtArray = new string[] {
			"ディレクター",
			"速水大秀、畑舜矢",
			"ゲームプランナー",
			"速水大秀、畑舜矢",
			"ゲームプログラマー",
			"速水大秀",
			"ゲームデザイナー",
			"速水大秀、畑舜矢",
			"",
			"参考文献",
			"",
			"ツール",
			"SourceTree",
			"https://ja.atlassian.com/software/sourcetree",
			"Trello",
			"https://trello.com/",
			"",
			"アセット",
			"UniRx",
			"https://www.assetstore.unity3d.com/jp/#!/content/17276",
			"DOTween Pro",
			"https://www.assetstore.unity3d.com/jp/#!/content/32416",
			"\"Unity-chan!\" Model",
			"https://www.assetstore.unity3d.com/jp/#!/content/18705",
			"Package Uninstaller",
			"https://www.assetstore.unity3d.com/jp/#!/content/35439",
			"TextMesh Pro",
			"https://www.assetstore.unity3d.com/jp/#!/content/84126",
			"Amplify Shader Editor",
			"https://www.assetstore.unity3d.com/jp/#!/content/68570",
			"Skybox Series freebie",
			"https://www.assetstore.unity3d.com/jp/#!/content/103633",
			"",
			"プログラム",
			"",
			"ジャンプのアニメーション",
			"mecanimのアニメーションイベントで、ジャンプしてから着地するまでを自然に表現する",
			"https://qiita.com/adarapata/items/322b416022d536f8e2eb",
			"",
			"AI",
			"【Unity】Rigidbodyを使い、放物線を描き、目標の地点へ  - うら干物書き",
			"http://www.urablog.xyz/entry/2017/05/16/235548",
			"",
			"アシスト",
			"Unity とあるオブジェクトの周りを回転するスクリプト transform.Rotate transform.LookAt",
			"https://gist.github.com/hiroyukihonda/8552618",
			"オブジェクトを自動で円状に配置【Unity】 - (:3[kanのメモ帳]",
			"http://kan-kikuchi.hatenablog.com/entry/CircleDeployer",
			"",
			"デカール",
			"Projectorでデカールもどき - D.N.A.のおぼえがき",
			"http://dnasoftwares.hatenablog.com/entry/2016/02/21/185311",
			"",
			"最適化",
			"【Unity道場 2016】パフォーマンス最適化のポイント",
			"https://www.slideshare.net/UnityTechnologiesJapan/unity-2016-77897096",
			"【Unite 2017 Tokyo】Unity最適化講座 ～スペシャリストが教えるメモリとCPU使用率の負担最小化テクニック～",
			"https://www.slideshare.net/UnityTechnologiesJapan/unite-2017-tokyounity-cpu",
			"",
			"カーソル",
			"【Unity】画面外のターゲットを追跡するカーソル - テラシュールブログ",
			"http://tsubakit1.hateblo.jp/entry/2016/04/26/233000",
			"",
			"背景",
			"[Unity] 2D Spriteにシェーダーをかける",
			"http://h-sao.com/blog/2016/01/11/unity-shader-2dsprite/",
			"",
			"マテリアルのタイリング",
			"MaterialのTiling(タイリング)を操作する - Neareal",
			"http://www.neareal.net/index.php?ComputerGraphics%2FUnity%2FTips%2FScript%2FMaterial%2FEditMaterialsTiling",
			"",
			"文字列",
			"C# - 文字列から一文字を取り出す",
			"http://jeanne.wankuma.com/tips/csharp/string/chars.html",
			"Char.ToString メソッド (Char) (System)",
			"https://msdn.microsoft.com/ja-jp/library/3d315df2(v=vs.110).aspx",
			".NET TIPS 文字列の全角／半角をチェックするには？ - C# - ＠IT",
			"http://www.atmarkit.co.jp/fdotnet/dotnettips/014strcheck/strcheck.html",
			"",
			"汎用メソッド",
			"重み付きランダム - Qiita",
			"https://qiita.com/divideby_zero/items/a8e749e307013ab24a0b",
			"",
			"FlyingText3D",
			"Unityで文字の3Dモデルを作る（FlyingText3Dアセット編） - Qiita",
			"https://qiita.com/arumani/items/fc7211287678deb75277",
			"",
			"Grayちゃん",
			"GrayChan_0_5_0",
			"http://rarihoma.xvs.jp/products/graychan",
			"ライセンス",
			"http://www.gray-chan.com/agreement",
			"",
			"リソース",
			"",
			"画像",
			"フリーテクスチャ素材館／薄茶レンガブロック壁のパターン素材01(PHOTO)",
			"https://free-texture.net/seamless-pattern/brick01.html",
			"Eighth note icon 8 | アイコン素材ダウンロードサイト「icooon-mono」 | 商用利用可能なアイコン素材が無料(フリー)ダウンロードできるサイト",
			"http://icooon-mono.com/11221-eighth-note-icon-8/?lang=en",
			"mark_batsu.png",
			"http://1.bp.blogspot.com/-eJGNGE4u8LA/UsZuCAMuehI/AAAAAAAAc2c/QQ5eBSC2Ey0/s800/mark_batsu.png",
			"",
			"フォント",
			"スマートフォントUI",
			"https://www.flopdesign.com/freefont/smartfont.html",
			"Oswald",
			"https://www.fontsquirrel.com/fonts/oswald",
			"Ice Age Font",
			"https://www.dafont.com/ice-age-font.font",
			"",
			"その他",
			"",
			"モーション",
			"【Unity】Humanoid系のモーションを使いまわしてみた - スーパー空箱",
			"http://ponkotsu-box.hatenablog.com/entry/2015/05/08/000348",
			"gitignore",
			"https://qiita.com/nariya/items/97afba6b7b448920cdf0",
			"",
			"パーティクル",
			"その１ Unityのパーティクル「Shuriken」",
			"http://marupeke296.com/UNI_PT_No1_Shuriken.html",
			"【Unity】パーティクルシステムのSkinned Mesh Rendererについて | ゴマちゃんフロンティア",
			"http://gomafrontier.com/unity/798",
			"[Unity] ParticleSystemで連番テクスチャを扱う (TextureSheetAnimation) - Qiita",
			"https://qiita.com/lycoris102/items/befb1ba79df9bba19b9f",
			"Unity5.5から Particle Systemのパラメータをスクリプトから変更する方法が変わった話 - Qiita",
			"https://qiita.com/UnagiHuman/items/caaa1585f7ee201dca7b",
			"",
			"Curve全般",
			"Unity - マニュアル: 曲線の編集",
			"https://docs.unity3d.com/jp/540/Manual/EditingCurves.html",
			"",
			"AnimationCurve",
			"AnimationCurveをInspectorで設定し、スクリプトから使う【Unity】 - (:3[kanのメモ帳]",
			"http://kan-kikuchi.hatenablog.com/entry/AnimationCurve_nspector",
			"",
			"Mathf",
			"数学系の処理を扱うMathfの全変数と全関数【Unity】 - (:3[kanのメモ帳]",
			"http://kan-kikuchi.hatenablog.com/entry/Mathf#逆補完正規化InverseLerp",
			"",
			"Timeline",
			"Unity2017のTimelineをやってみた &#8211; てっくぼっと！",
			"http://blog.applibot.co.jp/blog/2017/06/16/unity2017timeline/",
			"そろそろUnity2017のTimelineの基礎を押さえておこう - 渋谷ほととぎす通信",
			"http://www.shibuya24.info/entry/timeline_basis",
			"Unity - Manual:  Playable Director component",
			"https://docs.unity3d.com/Manual/class-PlayableDirector.html",
			"",
			"虹色",
			"Rainbow PNG image",
			"http://pngimg.com/download/5580",
			"",
			"DOTween",
			"[Unity]DOTweenめーも｜杏z 学習帳",
			"https://anz-note.tumblr.com/post/145405933481/unitydotween%E3%82%81%E3%83%BC%E3%82%82",
			"【Unity】DOTweenでTime.timeScaleを無視する方法 - コガネブログ",
			"http://baba-s.hatenablog.com/entry/2016/11/17/100000",
			"",
			"Enum",
			"C# で enum と int &#12289; string を 相互 に 変換 する 方法 - galife",
			"https://garafu.blogspot.jp/2015/07/c-enum.html",
			"",
			"アサート",
			"Unityのアサート機能について &#8211; Unity Blog",
			"https://blogs.unity3d.com/jp/2015/08/25/the-unity-assertion-library/",
			"",
			"UniRx",
			"UniRxのシンプルなサンプル その9(TimerとInterval 一定時間後に実行) - Qiita",
			"https://qiita.com/Marimoiro/items/a72b60315c797c19a27c",
			"",
			"Rigidbody",
			"【Unity】物理演算で動かすオブジェクトが壁を貫通する問題と対策 - テラシュールブログ",
			"http://tsubakit1.hateblo.jp/entry/2016/07/09/235856",
			"【Unity】 Rigidbodyの回転方法 - エフアンダーバー",
			"http://www.f-sp.com/entry/2017/09/06/181854",
			"Unity で rigidbody の位置,回転の固定をスクリプトから変更する  |  Lonely Mobiler",
			"https://loumo.jp/wp/archive/20131210003026/",
			"",
			"TextMesh Pro",
			"Rich Text, TextMesh Pro Documentation",
			"http://digitalnativestudios.com/textmeshpro/docs/rich-text/",
			"TextMeshProの使い方 - Qiita",
			"https://qiita.com/hinagawa/items/b606c6a2fd56d559a375",
			"【Unity道場 博多スペシャル 2017】Textmesh proを使いこなす",
			"https://www.slideshare.net/UnityTechnologiesJapan/unity2017text-meshpro",
			"",
			"オブジェクトをクリックしたことを検知する",
			"[Unity初心者Tips]オブジェクトがクリックされたか検知する方法、よく見かける？あの方法と比較 - Qiita",
			"https://qiita.com/JunShimura/items/4547563fbb2691f40626",
			"Unity2d クリック(タップ) されたオブジェクトを特定するやり方 - Qiita",
			"https://qiita.com/yxuyxu/items/ffec547e9b93cfd2b99d",
			"",
			"GIMP",
			"GIMP: 選択: 境界をぼかす",
			"http://alphasis.info/2012/08/gimp-selection-feather/",
			"",
			"カーソルの表示/非表示",
			"Unity - スクリプトリファレンス: Cursor.lockState",
			"https://docs.unity3d.com/ja/540/ScriptReference/Cursor-lockState.html",
			"",
			"非同期ロード",
			"Resourceを非同期で読み込む - 霙の忘備録",
			"http://mizorememorandum.blog.fc2.com/blog-entry-3.html",
			"",
			"定数の配列",
			"Unityで定数を定義する - Qiita",
			"https://qiita.com/okuhiiro/items/254c360a0731dc640b56",
			"",
			"Lighting",
			"Unity5でRender Settingsが消えたらしい - のしメモ アプリ開発ブログ",
			"http://www.noshimemo.com/entry/2015/04/08/023033",
			"",
			"色の抽出",
			"イメージカラーピッカー | 画像から色(カラーコード)を抽出",
			"https://lab.syncer.jp/Tool/Image-Color-Picker/",
			"",
			"それと…",
			"",
			"遊んでくれたみなさん！",
			"",
			"ありがとうございました！！"
		};
		return txtArray;
	}

	/// <summary>
	/// 流れる文字を作る
	/// </summary>
	/// <param name="txtArray">文をまとめた配列</param>
	IEnumerator createWords(string[] txtArray)
	{
		var wordPosX = First_Word_Pos_X;
		var prevWordPosXBegin = 0.0f;
		var prevWordPosXEnd = 0.0f;

		for (var txtIndex = 0; txtIndex < txtArray.Length; ++txtIndex) {
			var isNewLine = txtIndex % 2 == 1;
			if (!isNewLine) {
				prevWordPosXBegin = wordPosX;
			}

			var isCenter = txtArray[Mathf.Max(0, txtIndex - 1)].Length == 0;

			var wordInterval = !!isZenkaku(txtArray[txtIndex]) ? Word_Interval : Word_Interval / 1.5f;

			var isEndTxt = txtIndex == txtArray.Length - 1;
			if (!!isEndTxt) {
				endTxtLen = txtArray[txtIndex].Length;
			}

			for (var wordIndex = 0; wordIndex < txtArray[txtIndex].Length; ++wordIndex) {

				var go = FlyingText.GetObject(txtArray[txtIndex][wordIndex].ToString());
				go.transform.SetParent(TxtsParent);

				var col = go.AddComponent<MeshCollider>();
				col.convex = true;

				var staffRollTxt = go.AddComponent<StaffRollText>();
				var setTag = !!isEndTxt ? "EndText" : "Text";
				staffRollTxt.setVal(this, setTag);

				var indent = 0.0f;
				var posY = Line_1_Pos_Y;
				if (!!isNewLine) {
					indent = Indent;
					posY = Line_2_Pos_Y;
				}
				if (!!isCenter) {
					posY = (Line_1_Pos_Y + Line_2_Pos_Y) / 2;
				}

				go.transform.localPosition = new Vector3(wordPosX + indent, posY);
				wordPosX += col.bounds.size.x + wordInterval;

				yield return 0;
			}
			if (!!isNewLine) {
				wordPosX = Mathf.Max(wordPosX, prevWordPosXEnd) + Txt_Interval;
			} else {
				prevWordPosXEnd = wordPosX;
				wordPosX = prevWordPosXBegin;
			}
		}
	}

	/// <summary>
	/// SEを再生する
	/// </summary>
	/// <param name="se">SEの種類</param>
	public void playSE(SE se, AudioSource source)
	{
		if (source == null) {
			source = SEAudioSource;
		}
		source.PlayOneShot(SEs[(int)se]);
	}

	/// <summary>
	/// オプションを閉じると呼ばれる
	/// </summary>
	public void endOption()
	{
		Time.timeScale = prevTimeScale;
		Player.setCanInput(true);
	}

	/// <summary>
	/// 文が全角かどうかをチェックする
	/// .NET TIPS 文字列の全角／半角をチェックするには？ - C# - ＠IT
	/// http://www.atmarkit.co.jp/fdotnet/dotnettips/014strcheck/strcheck.html
	/// </summary>
	/// <param name="str">文</param>
	/// <returns>全角ならtrue</returns>
	bool isZenkaku(string str)
	{
		var num = enc.GetByteCount(str);
		return num == str.Length * 2;
	}

	/// <summary>
	/// EndTextをカウントアップする
	/// </summary>
	public void cntEndTxt()
	{
		++endTxtCnt.Value;
	}

	/// <summary>
	/// 終了処理
	/// </summary>
	void end()
	{
		FadePanel.color = Color.clear;
		DOTween.ToAlpha(
			() => FadePanel.color,
			color => FadePanel.color = color,
			1.0f,
			3.0f
		).OnComplete(() => {
			SceneManager.LoadScene(Common.Title_Scene);
			// このCanvasは他のシーンへ引き継がれてしまうため、消す必要がある
			Destroy(gameObject);
			BlackCanvasGo.SetActive(true);
		});
	}
}
