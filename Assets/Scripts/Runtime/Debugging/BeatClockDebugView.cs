using UnityEngine;
using UnityEngine.UI;
using ShadowRhythm.Rhythm;

namespace ShadowRhythm.Debugging
{
    /// <summary>
    /// 节拍时钟调试视图 - 可视化显示当前节拍状态
    /// </summary>
    public sealed class BeatClockDebugView : MonoBehaviour
    {
        [Header("依赖")]
        [SerializeField] private BeatClockSystem beatClockSystem;

        [Header("视觉反馈")]
        [SerializeField] private Image beatFlashImage;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.2f);
        [SerializeField] private float flashDuration = 0.1f;

        [Header("UI 文本（可选）")]
        [SerializeField] private Text beatIndexText;
        [SerializeField] private Text barInfoText;
        [SerializeField] private Text songTimeText;
        [SerializeField] private Text bpmText;

        [Header("调试设置")]
        [SerializeField] private bool logEveryBeat = true;

        private float _flashTimer;

        private void Start()
        {
            if (beatClockSystem == null)
            {
                beatClockSystem = FindObjectOfType<BeatClockSystem>();
            }

            if (beatClockSystem != null)
            {
                beatClockSystem.OnNewBeat += HandleNewBeat;
                beatClockSystem.OnNewBar += HandleNewBar;
            }
            else
            {
                Debug.LogWarning("[BeatClockDebugView] 未找到 BeatClockSystem！");
            }

            // 初始化 UI
            if (beatFlashImage != null)
            {
                beatFlashImage.color = normalColor;
            }
        }

        private void OnDestroy()
        {
            if (beatClockSystem != null)
            {
                beatClockSystem.OnNewBeat -= HandleNewBeat;
                beatClockSystem.OnNewBar -= HandleNewBar;
            }
        }

        private void Update()
        {
            UpdateFlash();
            UpdateUI();
        }

        private void HandleNewBeat(BeatFrame frame)
        {
            // 触发闪烁
            _flashTimer = flashDuration;

            if (beatFlashImage != null)
            {
                beatFlashImage.color = flashColor;
            }

            if (logEveryBeat)
            {
                string barPosition = $"{frame.beatInBar + 1}/4";
                Debug.Log($"♪ Beat {frame.beatIndex} | Bar {frame.barIndex} [{barPosition}] | Time: {frame.songTime:F3}s | Delta: {frame.deltaMs:F1}ms");
            }
        }

        private void HandleNewBar(BeatFrame frame)
        {
            Debug.Log($"═══════════ Bar {frame.barIndex} Start ═══════════");
        }

        private void UpdateFlash()
        {
            if (_flashTimer > 0)
            {
                _flashTimer -= Time.deltaTime;

                if (beatFlashImage != null)
                {
                    float t = _flashTimer / flashDuration;
                    beatFlashImage.color = Color.Lerp(normalColor, flashColor, t);
                }
            }
        }

        private void UpdateUI()
        {
            if (beatClockSystem == null || !beatClockSystem.IsRunning) return;

            var frame = beatClockSystem.CurrentBeatFrame;

            if (beatIndexText != null)
            {
                beatIndexText.text = $"Beat: {frame.beatIndex}";
            }

            if (barInfoText != null)
            {
                barInfoText.text = $"Bar {frame.barIndex} | {frame.beatInBar + 1}/4";
            }

            if (songTimeText != null)
            {
                songTimeText.text = $"Time: {frame.songTime:F2}s";
            }

            if (bpmText != null && beatClockSystem.SongRuntime != null)
            {
                bpmText.text = $"BPM: {beatClockSystem.SongRuntime.Bpm}";
            }
        }

        /// <summary>
        /// 在 Scene 视图中绘制调试信息
        /// </summary>
        private void OnGUI()
        {
            if (beatClockSystem == null || !beatClockSystem.IsRunning) return;

            var frame = beatClockSystem.CurrentBeatFrame;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"═══ Beat Clock Debug ═══");
            GUILayout.Label($"Song Time: {frame.songTime:F3}s");
            GUILayout.Label($"Beat Index: {frame.beatIndex}");
            GUILayout.Label($"Bar: {frame.barIndex} | Beat: {frame.beatInBar + 1}/4");
            GUILayout.Label($"Progress: {frame.beatProgress:F2}");
            GUILayout.Label($"Delta to Beat: {frame.deltaMs:F1}ms");

            if (beatClockSystem.SongRuntime != null)
            {
                GUILayout.Label($"BPM: {beatClockSystem.SongRuntime.Bpm}");
                GUILayout.Label($"Sec/Beat: {beatClockSystem.SongRuntime.SecondsPerBeat:F4}s");
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}