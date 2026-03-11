using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ShadowRhythm.Input;
using ShadowRhythm.Rhythm;

namespace ShadowRhythm.Debugging
{
    /// <summary>
    /// 输入调试面板 - 可视化显示输入判定结果
    /// </summary>
    public sealed class InputDebugPanel : MonoBehaviour
    {
        [Header("依赖")]
        [SerializeField] private GameplayInputRouter inputRouter;
        [SerializeField] private BeatClockSystem beatClockSystem;

        [Header("UI 元素（可选）")]
        [SerializeField] private Text inputTypeText;
        [SerializeField] private Text timingText;
        [SerializeField] private Text resultText;
        [SerializeField] private Text bufferStatsText;
        [SerializeField] private Image resultFlashImage;

        [Header("判定结果颜色")]
        [SerializeField] private Color perfectColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color goodColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color missColor = new Color(0.8f, 0.2f, 0.2f);

        [Header("显示设置")]
        [SerializeField] private float resultDisplayDuration = 0.5f;
        [SerializeField] private int maxHistoryDisplay = 8;

        // 输入历史（用于 OnGUI 显示）
        private readonly List<InputSample> _inputHistory = new List<InputSample>();
        private InputSample? _lastInput;
        private float _resultDisplayTimer;

        private void Start()
        {
            // 自动查找组件
            if (inputRouter == null)
                inputRouter = FindObjectOfType<GameplayInputRouter>();

            if (beatClockSystem == null)
                beatClockSystem = FindObjectOfType<BeatClockSystem>();

            // 订阅输入事件
            if (inputRouter != null)
            {
                inputRouter.OnInputReceived += HandleInputReceived;
            }

            Debug.Log("[InputDebugPanel] 初始化完成");
        }

        private void OnDestroy()
        {
            if (inputRouter != null)
            {
                inputRouter.OnInputReceived -= HandleInputReceived;
            }
        }

        private void Update()
        {
            // 更新结果显示计时器
            if (_resultDisplayTimer > 0)
            {
                _resultDisplayTimer -= Time.deltaTime;
            }

            UpdateUI();
        }

        private void HandleInputReceived(InputSample sample)
        {
            _lastInput = sample;
            _resultDisplayTimer = resultDisplayDuration;

            // 添加到历史
            _inputHistory.Add(sample);
            if (_inputHistory.Count > maxHistoryDisplay)
            {
                _inputHistory.RemoveAt(0);
            }

            // 更新 UI 颜色
            if (resultFlashImage != null)
            {
                resultFlashImage.color = GetResultColor(sample.judgeResult);
            }
        }

        private void UpdateUI()
        {
            if (_lastInput.HasValue && _resultDisplayTimer > 0)
            {
                var input = _lastInput.Value;

                if (inputTypeText != null)
                {
                    inputTypeText.text = $"{GameplayInputRouter.GetInputTypeName(input.inputType)} ({input.inputType})";
                }

                if (timingText != null)
                {
                    string timing = input.deltaMs >= 0 ? $"+{input.deltaMs:F1}ms" : $"{input.deltaMs:F1}ms";
                    timingText.text = timing;
                }

                if (resultText != null)
                {
                    resultText.text = InputWindowEvaluator.GetResultText(input.judgeResult);
                    resultText.color = GetResultColor(input.judgeResult);
                }
            }

            // 更新缓冲区统计
            if (bufferStatsText != null && inputRouter != null)
            {
                bufferStatsText.text = inputRouter.InputBuffer.GetStats();
            }
        }

        private Color GetResultColor(RhythmJudgeResult result)
        {
            return result switch
            {
                RhythmJudgeResult.Perfect => perfectColor,
                RhythmJudgeResult.Good => goodColor,
                RhythmJudgeResult.Miss => missColor,
                _ => Color.white
            };
        }

        /// <summary>
        /// 在屏幕上绘制调试信息
        /// </summary>
        private void OnGUI()
        {
            // 左侧面板 - 当前状态
            GUILayout.BeginArea(new Rect(10, 220, 300, 400));
            GUILayout.BeginVertical("box");

            GUILayout.Label("═══ Input Debug Panel ═══");

            // 当前拍点信息
            if (beatClockSystem != null && beatClockSystem.IsRunning)
            {
                var frame = beatClockSystem.CurrentBeatFrame;
                GUILayout.Label($"Current Beat: {frame.beatIndex}");
                GUILayout.Label($"Bar: {frame.barIndex} | Beat: {frame.beatInBar + 1}/4");
                GUILayout.Label($"Progress: {frame.beatProgress:F2}");
            }

            GUILayout.Space(10);

            // 最后一次输入
            if (_lastInput.HasValue && _resultDisplayTimer > 0)
            {
                var input = _lastInput.Value;
                GUI.color = GetResultColor(input.judgeResult);

                GUILayout.Label($"═══ Last Input ═══");
                GUILayout.Label($"Type: {GameplayInputRouter.GetInputTypeName(input.inputType)} ({input.inputType})");

                string timing = input.deltaMs >= 0 ? $"+{input.deltaMs:F1}ms" : $"{input.deltaMs:F1}ms";
                GUILayout.Label($"Timing: {timing}");
                GUILayout.Label($"Result: {InputWindowEvaluator.GetResultText(input.judgeResult)}");
                GUILayout.Label($"Beat: {input.quantizedBeatIndex}");

                GUI.color = Color.white;
            }

            GUILayout.Space(10);

            // 输入历史
            GUILayout.Label("═══ Input History ═══");
            for (int i = _inputHistory.Count - 1; i >= 0; i--)
            {
                var sample = _inputHistory[i];
                GUI.color = GetResultColor(sample.judgeResult);

                string timing = sample.deltaMs >= 0 ? $"+{sample.deltaMs:F0}" : $"{sample.deltaMs:F0}";
                GUILayout.Label($"[{sample.quantizedBeatIndex}] {sample.inputType} {timing}ms → {sample.judgeResult}");
            }
            GUI.color = Color.white;

            // 缓冲区统计
            if (inputRouter != null)
            {
                GUILayout.Space(10);
                GUILayout.Label(inputRouter.InputBuffer.GetStats());
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();

            // 操作提示
            GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 150));
            GUILayout.BeginVertical("box");
            GUILayout.Label("═══ Controls ═══");
            GUILayout.Label("上  = Lift (提)");
            GUILayout.Label("左     = Flick (拨)");
            GUILayout.Label("下     = Shake (抖)");
            GUILayout.Label("右     = Flash (闪)");
            GUILayout.Label("Space  = Pause/Resume");
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}