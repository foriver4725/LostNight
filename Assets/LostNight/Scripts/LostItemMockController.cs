using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LostNight
{
    // R3 drives mock state changes; UniTask sequences short feedback effects.
    public sealed class LostItemMockController : MonoBehaviour
    {
        [SerializeField] private Transform itemRoot;
        [SerializeField] private Text clockText;
        [SerializeField] private Text caseText;
        [SerializeField] private Text memoText;
        [SerializeField] private Text messageText;
        [SerializeField] private Button recordButton;
        [SerializeField] private Button observeButton;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button storeButton;

        private readonly ReactiveProperty<int> foundClues = new(0);
        private readonly CompositeDisposable disposables = new();
        private CancellationTokenSource feedbackCancellation;
        private Vector3 lastPointerPosition;
        private bool dragging;

        private static readonly string[] Clues =
        {
            "濡れていない",
            "柄に小さな歯形",
            "傘の内側に夜空が見える"
        };

        public void Initialize(Transform item, Text clock, Text caseLabel, Text memo, Text message,
            Button record, Button observe, Button returnAction, Button store)
        {
            itemRoot = item; clockText = clock; caseText = caseLabel; memoText = memo; messageText = message;
            recordButton = record; observeButton = observe; returnButton = returnAction; storeButton = store;
        }

        private void Start()
        {
            recordButton.onClick.AddListener(RecordClue);
            observeButton.onClick.AddListener(Observe);
            returnButton.onClick.AddListener(() => ShowVerdictAsync(true).Forget());
            storeButton.onClick.AddListener(() => ShowVerdictAsync(false).Forget());

            foundClues.Subscribe(UpdateMemo).AddTo(disposables);
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(_ => PulseClock())
                .AddTo(disposables);
            UpdateMemo(0);
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
            {
                dragging = true;
                lastPointerPosition = mouse.position.ReadValue();
            }

            if (mouse.leftButton.wasReleasedThisFrame) dragging = false;
            if (dragging && itemRoot != null)
            {
                var pointerPosition = mouse.position.ReadValue();
                var delta = (Vector3)pointerPosition - lastPointerPosition;
                itemRoot.Rotate(Vector3.up, -delta.x * 0.35f, Space.World);
                itemRoot.Rotate(Vector3.right, delta.y * 0.2f, Space.World);
                lastPointerPosition = pointerPosition;
            }

            if (itemRoot != null)
            {
                var zoom = mouse.scroll.ReadValue().y / 120f;
                itemRoot.localScale = Vector3.one * Mathf.Clamp(itemRoot.localScale.x + zoom * 0.08f, 0.75f, 1.35f);
            }
        }

        private void RecordClue()
        {
            if (foundClues.Value < Clues.Length) foundClues.Value++;
            messageText.text = foundClues.Value == Clues.Length ? "特徴をすべて記録しました" : "気になる特徴を記録しました";
        }

        private void Observe()
        {
            itemRoot.Rotate(0f, 35f, 0f, Space.World);
            messageText.text = "ドラッグで回転 / ホイールで拡大";
        }

        private void UpdateMemo(int count)
        {
            memoText.text = "調査メモ\n\n";
            for (var i = 0; i < Clues.Length; i++)
                memoText.text += $"{(i < count ? "■" : "□")} {Clues[i]}\n";
            caseText.text = $"案件 01 / 残り 1　　記録 {count}/3";
        }

        private async UniTaskVoid ShowVerdictAsync(bool returned)
        {
            feedbackCancellation?.Cancel();
            feedbackCancellation = new CancellationTokenSource();
            var token = feedbackCancellation.Token;
            if (returned && foundClues.Value >= 2)
            {
                messageText.text = "返却成功 — 子どもの影が、歯形を知っていた。";
                clockText.text = "0:12";
                await UniTask.Delay(TimeSpan.FromSeconds(2.5), cancellationToken: token);
                messageText.text = "駅の時計が、1分だけ戻った。";
            }
            else if (returned)
            {
                messageText.text = "証拠が足りない。もう少し観察しよう。";
            }
            else
            {
                messageText.text = "保管しました — 夜空はまだ傘の中にある。";
            }
        }

        private void PulseClock()
        {
            if (clockText != null) clockText.color = clockText.color == Color.white ? new Color(1f, .42f, .16f) : Color.white;
        }

        private void OnDestroy()
        {
            feedbackCancellation?.Cancel();
            feedbackCancellation?.Dispose();
            disposables.Dispose();
            foundClues.Dispose();
        }
    }
}
