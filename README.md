# 終電忘れ物センター / LostNight

終電後の忘れ物窓口を舞台に、品物に残された特徴と申告者の証言を照合する、観察・推理・軽い怪異の短編ゲームです。

## 動作環境

- Unity `6000.3.18f1`
- Universal Render Pipeline（URP）
- Input System
- WebGL対応を想定

`Assets/Scenes/LostItemCenter.unity` を開いて再生してください。

## 遊び方

![終電忘れ物センターの遊び方：観察、記録、判断](Assets/Documentation/how-to-play.png)

1. 忘れ物をドラッグで回転し、マウスホイールで拡大します。
2. 光っている気になる箇所をクリックすると、発見した特徴が調査メモへ自動で記録されます。
3. 記録した特徴と申告者A・Bの証言を照合します。
4. 持ち主を特定できたら申告者を選んで「返却」、誰の物とも断定できなければ「保管」を選びます。
5. 判定理由を確認して次の案件へ進み、5件の正解を目指します。

各案件の制限時間は45秒です。少ない調査回数で正解すると迅速判定ボーナス、残り時間に応じて時間ボーナスを獲得できます。誤判断または時間切れが3件に達するとゲームオーバーです。

## ゲームフロー

`音声有効化 → タイトル → チュートリアル → 案件調査 → 判定結果 → 次の案件`

5件正解でクリアとなり、クリア／ゲームオーバー画面から再挑戦またはタイトルへ戻れます。WebGLではブラウザの自動再生制限に対応するため、最初のタップ後にBGMとSEを有効化します。タイトル・ゲーム中・結果画面から共通音量を調整できます。

## 開発について

本作は、制作者がゲーム内容と仕様を決定し、以下のツールによる制作支援を受けて開発しています。

- **OpenAI Codex** — 設計、実装、デバッグ、ドキュメント整備などの開発支援
- **Unity CLI** — Unity Editorのバッチ実行、シーンのベイク、構成検証
- **Unity Editor** — シーン編集、アセットのインポート、再生確認、WebGL向け制作
- **Git / GitHub** — バージョン管理と成果物の共有

CodexやUnity CLIは制作に使用したツールであり、それ自体をゲームへ同梱しているわけではありません。AIが出力した内容はそのまま採用せず、プロジェクト内で動作確認・修正を行う方針です。

Unity AI関連パッケージも開発環境に導入されていますが、パッケージが存在することと、各成果物の生成元であることは同義ではありません。

## 使用ライブラリ

- [UniTask](https://github.com/Cysharp/UniTask)
- [R3](https://github.com/Cysharp/R3)
- [VContainer](https://github.com/hadashiA/VContainer)
- Unity公式パッケージ（URP、Input System、uGUI、Timelineなど）

各ライブラリおよびUnity公式パッケージには、それぞれの配布元が定めるライセンスが適用されます。再配布や公開ビルドを行う場合は、使用バージョンに対応するライセンスおよび表示要件を各配布元で確認してください。

## アセットと権利について

### プロジェクト独自の素材

- ゲーム内の簡易3Dモデル、メッシュ、マテリアル、シェーダー表現は、本プロジェクト用にUnity上で作成・生成したものです。
- SEとBGMは、外部の音源ファイルを収録したものではなく、ゲーム内コードで波形を合成して再生しています。
- シナリオ、案件データ、UI文言、ゲームルールは本プロジェクト用に制作したものです。
- `Assets/LostNight/Generated` のファイルは、Editorのベイク処理から生成された本プロジェクトの構成物です。

### AI支援による画像

`Assets/Documentation/how-to-play.png` は、遊び方を説明するためにAI画像生成の支援を利用して制作した画像です。AI生成物の利用条件や著作権上の扱いは、公開地域、利用サービスの規約、配布形態によって異なる可能性があります。公開・販売時の最終的な権利確認は配布者が行ってください。

企画時に使用した参考画像や企画書は開発資料であり、現在のゲーム本体へそのまま収録されているとは限りません。READMEの記載は、原則としてこのリポジトリ内に含まれるファイルを対象としています。

### Noto Sans JP

日本語表示には **Noto Sans JP** を使用しています。

- Copyright: 2014–2021 Adobe（Reserved Font Name: `Source`）
- License: SIL Open Font License 1.1
- Font: `Assets/LostNight/ThirdParty/NotoSansJP/NotoSansJP.ttf`
- License text: `Assets/LostNight/ThirdParty/NotoSansJP/OFL.txt`

フォントを再配布・改変する場合は、同梱されているOFL本文を確認してください。

### 商標・サービス名

Unity、OpenAI、Codex、GitHub、その他本文中の製品名・サービス名は、各権利者の商標または登録商標である場合があります。本プロジェクトは、各サービス提供者による公式な保証・提携・承認を示すものではありません。

## コード構成

- `LostItemCaseDefinition` — 1案件分の忘れ物、特徴、申告者、判定理由
- `LostItemCaseCatalog` — 13件の案件一覧
- `CaseDeck` — セッション開始時に案件をシャッフルし、重複なしで出題
- `LostItemModelPresenter` — 案件ごとの専用3Dモデルと調査ポイントを切り替え
- `LostNightAudio` — WebGL対応のSEと、雨・低音・遠いベルによるループBGM
- `GameSession` — 正解数、ミス数、得点、クリア／ゲームオーバー条件
- `GameFlowState` — タイトルから終了までの画面状態
- `LostNightScreenView` — UI表示とボタン参照
- `LostItemMockController` — 入力を受け、セッションと画面遷移を仲介
- `LostItemSceneFactory` — Editorベイク時に使用するシーン、UI、モデル生成

## シーンのベイク

実行時にはシーンや3Dモデルを生成しません。構成を変更した場合は、次のいずれかで `LostItemCenter.unity` を再生成します。

- Unity Editor: `Lost Night > Bake Lost Item Center Scene`
- Unityを閉じてUnity CLIを実行:

```sh
unity run . -- -executeMethod LostNight.Editor.LostItemSceneBuilder.BuildFromCommandLine
```

生成されたMaterialとMeshは `Assets/LostNight/Generated` に保存されます。ベイク後はシーンを再生し、フォント、音声、クリック判定、各案件のモデル表示を確認してください。

## ライセンスに関する注意

このREADMEは、リポジトリ内の素材と依存関係を把握しやすくするための説明であり、法的助言ではありません。ゲームの公開、販売、素材の再配布を行う際は、配布者の責任で最新の利用規約とライセンスを確認してください。
