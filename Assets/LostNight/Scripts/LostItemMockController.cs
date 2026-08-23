using System;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LostNight
{
    public sealed class LostItemMockController : MonoBehaviour
    {
        [Serializable]
        private sealed class CaseData
        {
            public string itemName;
            public string[] clues;
            public string[] claimantNames;
            public string[] claims;
            public int ownerIndex;
            public string successReason;
            public string failureReason;
            public string memory;
        }

        [SerializeField] private Transform itemRoot;
        [SerializeField] private Text clockText;
        [SerializeField] private Text caseText;
        [SerializeField] private Text memoText;
        [SerializeField] private Text messageText;
        [SerializeField] private Text itemText;
        [SerializeField] private Text claimantText;
        [SerializeField] private Text progressText;
        [SerializeField] private Button claimantAButton;
        [SerializeField] private Button claimantBButton;
        [SerializeField] private Transform[] clueHotspots;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button storeButton;
        [SerializeField] private Button nextButton;

        private readonly ReactiveProperty<int> foundClues = new(0);
        private readonly ReactiveProperty<int> selectedClaimant = new(-1);
        private readonly CompositeDisposable disposables = new();
        private Vector3 lastPointerPosition;
        private Vector3 pointerDownPosition;
        private bool dragging;
        private bool resolved;
        private int caseIndex;
        private int correctCount;
        private int mistakeCount;
        private readonly bool[] recordedClues = new bool[3];

        private readonly CaseData[] cases =
        {
            new()
            {
                itemName = "内側に星空が降る透明傘",
                clues = new[] { "濡れていない", "柄に小さな歯形", "内側に夜空が見える" },
                claimantNames = new[] { "A　会社員", "B　子どもの影" },
                claims = new[] { "『透明です。普通の傘でした』", "『持ち手に、かんだあとがある』" },
                ownerIndex = 1,
                successReason = "歯形を知っていたのは子どもの影だけだった。",
                failureReason = "会社員は色しか一致せず、歯形を説明できない。",
                memory = "駅の時計が、1分だけ戻った。"
            },
            new()
            {
                itemName = "片方だけ温かい右手袋",
                clues = new[] { "右手用", "乾いた砂が付着", "微かな拍手音がする" },
                claimantNames = new[] { "A　旅の楽団員", "B　清掃員" },
                claims = new[] { "『右手をなくした。砂浜で演奏した』", "『左手用だ。砂には触れていない』" },
                ownerIndex = 0,
                successReason = "右手・砂・拍手音のすべてが楽団員の証言と一致した。",
                failureReason = "清掃員の証言は左右と砂の両方で矛盾している。",
                memory = "誰もいないホームから、短い拍手が聞こえた。"
            },
            new()
            {
                itemName = "行先が消え続ける定期券",
                clues = new[] { "日付は明日", "顔写真が瞬く", "券面に駅員の名前" },
                claimantNames = new[] { "A　学生", "B　会社員" },
                claims = new[] { "『今日まで有効。写真は私です』", "『名前は私のものです』" },
                ownerIndex = -1,
                successReason = "申告者の誰とも一致しない。保管が正しい判断だった。",
                failureReason = "券面は申告者ではなく、窓口にいる駅員の名前を示している。",
                memory = "保管棚の奥で、明日の発車ベルが鳴った。"
            },
            new()
            {
                itemName = "雨音を閉じ込めた古い水筒",
                clues = new[] { "蓋に青い糸", "中から雨音", "底に山の刻印" },
                claimantNames = new[] { "A　登山客", "B　駅売店員" },
                claims = new[] { "『青い糸を目印にした。山で使った』", "『新品で、印は何もない』" },
                ownerIndex = 0,
                successReason = "青い糸と山の刻印が登山客の証言を裏付けた。",
                failureReason = "売店員の新品という証言は、刻印と雨音に矛盾している。",
                memory = "窓の外で、雨が一瞬だけ上へ降った。"
            },
            new()
            {
                itemName = "影だけが遅れて動く腕時計",
                clues = new[] { "針は0時13分", "裏蓋に猫の毛", "影が一秒遅れる" },
                claimantNames = new[] { "A　猫を抱いた女性", "B　制服の青年" },
                claims = new[] { "『猫の毛が裏に挟まっているはず』", "『正確な時計で、傷も汚れもない』" },
                ownerIndex = 0,
                successReason = "裏蓋の猫の毛を知っていた女性が持ち主だった。",
                failureReason = "青年の証言は止まった針と猫の毛の両方に合わない。",
                memory = "女性の影だけが、先に改札を抜けていった。"
            }
        };

        public void Initialize(Transform item, Text clock, Text caseLabel, Text memo, Text message,
            Text itemLabel, Text claimLabel, Text progressLabel, Button claimantA, Button claimantB,
            Transform[] hotspots, Button returnAction, Button store, Button next)
        {
            itemRoot = item; clockText = clock; caseText = caseLabel; memoText = memo; messageText = message;
            itemText = itemLabel; claimantText = claimLabel; progressText = progressLabel;
            claimantAButton = claimantA; claimantBButton = claimantB; clueHotspots = hotspots;
            returnButton = returnAction; storeButton = store; nextButton = next;
        }

        public void Initialize(Transform item, Text clock, Text caseLabel, Text memo, Text message,
            Button record, Button observe, Button returnAction, Button store)
        {
            Initialize(item, clock, caseLabel, memo, message, null, null, null, null, null, null,
                returnAction, store, null);
        }

        private void Start()
        {
            returnButton.onClick.AddListener(() => Resolve(true));
            storeButton.onClick.AddListener(() => Resolve(false));
            claimantAButton?.onClick.AddListener(() => SelectClaimant(0));
            claimantBButton?.onClick.AddListener(() => SelectClaimant(1));
            nextButton?.onClick.AddListener(NextCase);

            foundClues.Subscribe(UpdateMemo).AddTo(disposables);
            selectedClaimant.Subscribe(_ => UpdateDecisionState()).AddTo(disposables);
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => PulseClock()).AddTo(disposables);
            LoadCase();
        }

        private void Update()
        {
            if (resolved) return;
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (mouse.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
            {
                dragging = true;
                pointerDownPosition = mouse.position.ReadValue();
                lastPointerPosition = mouse.position.ReadValue();
            }
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                dragging = false;
                var pointerPosition = mouse.position.ReadValue();
                if (Vector2.Distance(pointerDownPosition, pointerPosition) < 12f) TryRecordHotspot(pointerPosition);
            }
            if (dragging && itemRoot != null)
            {
                var pointerPosition = mouse.position.ReadValue();
                var delta = (Vector3)pointerPosition - lastPointerPosition;
                itemRoot.Rotate(Vector3.up, -delta.x * .35f, Space.World);
                itemRoot.Rotate(Vector3.right, delta.y * .2f, Space.World);
                lastPointerPosition = pointerPosition;
            }
            if (itemRoot != null)
            {
                var zoom = mouse.scroll.ReadValue().y / 120f;
                itemRoot.localScale = Vector3.one * Mathf.Clamp(itemRoot.localScale.x + zoom * .08f, .75f, 1.35f);
            }
            if (clueHotspots != null)
            {
                var pulse = .18f + Mathf.Sin(Time.time * 4f) * .035f;
                foreach (var hotspot in clueHotspots)
                    if (hotspot != null && hotspot.gameObject.activeSelf) hotspot.localScale = Vector3.one * pulse;
            }
        }

        private void LoadCase()
        {
            resolved = false;
            foundClues.Value = 0;
            selectedClaimant.Value = -1;
            Array.Clear(recordedClues, 0, recordedClues.Length);
            var data = cases[caseIndex];
            if (itemText != null) itemText.text = $"本日の忘れ物\n{data.itemName}\n\n特徴を2つ以上記録して判断する。";
            if (claimantText != null)
                claimantText.text = $"{data.claimantNames[0]}\n{data.claims[0]}\n\n{data.claimantNames[1]}\n{data.claims[1]}";
            SetButtonLabel(claimantAButton, data.claimantNames[0]);
            SetButtonLabel(claimantBButton, data.claimantNames[1]);
            messageText.text = "忘れ物を回して光る箇所をクリック → 申告者を選択 → 返却 / 保管";
            if (nextButton != null) nextButton.gameObject.SetActive(false);
            SetActionsInteractable(true);
            UpdateHotspots();
            UpdateMemo(0);
            UpdateDecisionState();
        }

        private void TryRecordHotspot(Vector2 screenPosition)
        {
            var camera = Camera.main;
            if (camera == null || clueHotspots == null) return;
            var hits = Physics.RaycastAll(camera.ScreenPointToRay(screenPosition), 100f);
            for (var hitIndex = 0; hitIndex < hits.Length; hitIndex++)
            {
                for (var i = 0; i < clueHotspots.Length; i++)
                {
                    if (hits[hitIndex].transform != clueHotspots[i] || recordedClues[i]) continue;
                    recordedClues[i] = true;
                    foundClues.Value++;
                    UpdateHotspots();
                    messageText.text = foundClues.Value >= 2
                        ? $"『{cases[caseIndex].clues[i]}』を記録。判断可能です。"
                        : $"『{cases[caseIndex].clues[i]}』を記録。もう1箇所探してください。";
                    return;
                }
            }
        }

        private void SelectClaimant(int index)
        {
            if (resolved) return;
            selectedClaimant.Value = index;
            messageText.text = $"申告者 {cases[caseIndex].claimantNames[index]} を選択中。返却で確定します。";
            TintClaimantButtons();
        }

        private void Resolve(bool returned)
        {
            if (resolved || foundClues.Value < 2) return;
            if (returned && selectedClaimant.Value < 0)
            {
                messageText.text = "返却先の申告者を選択してください。";
                return;
            }

            var data = cases[caseIndex];
            var correct = returned ? selectedClaimant.Value == data.ownerIndex : data.ownerIndex < 0;
            resolved = true;
            if (correct) correctCount++; else mistakeCount++;
            messageText.text = $"{(correct ? "正しい判断" : "誤った判断")} — {(correct ? data.successReason : data.failureReason)}\n{data.memory}";
            clockText.text = $"0:{Mathf.Max(8, 13 - caseIndex - 1):00}";
            SetActionsInteractable(false);
            UpdateProgress();
            if (nextButton != null)
            {
                SetButtonLabel(nextButton, caseIndex == cases.Length - 1 ? "結果を見る" : "次の案件");
                nextButton.gameObject.SetActive(true);
            }
        }

        private void NextCase()
        {
            if (caseIndex < cases.Length - 1)
            {
                caseIndex++;
                LoadCase();
                return;
            }

            resolved = true;
            caseText.text = "一夜の業務終了";
            memoText.text = $"業務報告\n\n正しい判断　{correctCount} / {cases.Length}\n誤った判断　{mistakeCount}\n\n{(correctCount == cases.Length ? "結末コード：星傘-013" : "もう一度、証言をよく照合しよう。")}";
            messageText.text = correctCount == cases.Length
                ? "全案件を正しく処理しました。忘れ物は、記憶を少しだけ残していった。"
                : "一夜が終了しました。再挑戦して全案件の正解を目指せます。";
            SetButtonLabel(nextButton, "もう一度");
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(RestartRun);
        }

        private void RestartRun()
        {
            caseIndex = 0; correctCount = 0; mistakeCount = 0; clockText.text = "0:13";
            nextButton.onClick.RemoveAllListeners(); nextButton.onClick.AddListener(NextCase);
            LoadCase();
        }

        private void UpdateMemo(int count)
        {
            var data = cases[caseIndex];
            memoText.text = "調査メモ\n\n";
            for (var i = 0; i < data.clues.Length; i++)
                memoText.text += recordedClues[i] ? $"■ {data.clues[i]}\n" : "□ 未記録\n";
            caseText.text = $"案件 {caseIndex + 1:00} / {cases.Length:00}　記録 {count}/3";
            UpdateProgress();
            UpdateDecisionState();
        }

        private void UpdateDecisionState()
        {
            if (returnButton == null || storeButton == null) return;
            var enoughEvidence = foundClues.Value >= 2 && !resolved;
            returnButton.interactable = enoughEvidence && selectedClaimant.Value >= 0;
            storeButton.interactable = enoughEvidence;
            TintClaimantButtons();
        }

        private void UpdateProgress()
        {
            if (progressText != null) progressText.text = $"正解 {correctCount}　誤判断 {mistakeCount}　残り {cases.Length - caseIndex}";
        }

        private void SetActionsInteractable(bool value)
        {
            if (claimantAButton != null) claimantAButton.interactable = value;
            if (claimantBButton != null) claimantBButton.interactable = value;
            if (clueHotspots != null)
                foreach (var hotspot in clueHotspots)
                    if (hotspot != null) hotspot.gameObject.SetActive(value);
            if (!value) { returnButton.interactable = false; storeButton.interactable = false; }
        }

        private void UpdateHotspots()
        {
            if (clueHotspots == null) return;
            for (var i = 0; i < clueHotspots.Length; i++)
            {
                var hotspot = clueHotspots[i];
                if (hotspot == null) continue;
                hotspot.gameObject.SetActive(!recordedClues[i] && !resolved);
            }
        }

        private void TintClaimantButtons()
        {
            if (claimantAButton == null || claimantBButton == null) return;
            claimantAButton.image.color = selectedClaimant.Value == 0 ? new Color(.55f, .4f, .16f) : new Color(.14f, .22f, .24f);
            claimantBButton.image.color = selectedClaimant.Value == 1 ? new Color(.55f, .4f, .16f) : new Color(.14f, .22f, .24f);
        }

        private static void SetButtonLabel(Button button, string value)
        {
            if (button != null && button.GetComponentInChildren<Text>() is { } label) label.text = value;
        }

        private void PulseClock()
        {
            if (clockText != null) clockText.color = clockText.color == Color.white ? new Color(1f, .42f, .16f) : Color.white;
        }

        private void OnDestroy()
        {
            disposables.Dispose(); foundClues.Dispose(); selectedClaimant.Dispose();
        }
    }
}
