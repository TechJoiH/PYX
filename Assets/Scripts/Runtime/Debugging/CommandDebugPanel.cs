using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ShadowRhythm.Command;
using ShadowRhythm.Input;
using ShadowRhythm.Rhythm;

namespace ShadowRhythm.Debugging
{
    /// <summary>
    /// 命令调试面板 - 可视化显示命令解析结果
    /// </summary>
    public sealed class CommandDebugPanel : MonoBehaviour
    {
        [Header("依赖")]
        [SerializeField] private CommandResolver commandResolver;
        [SerializeField] private GameplayInputRouter inputRouter;
        [SerializeField] private BeatClockSystem beatClockSystem;

        [Header("UI 元素（可选）")]
        [SerializeField] private Text commandNameText;
        [SerializeField] private Text commandDetailText;
        [SerializeField] private Image commandFlashImage;

        [Header("颜色设置")]
        [SerializeField] private Color singleColor = new Color(0.5f, 0.8f, 1f);
        [SerializeField] private Color comboColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color sequenceColor = new Color(1f, 0.4f, 0.8f);
        [SerializeField] private Color perfectColor = new Color(1f, 0.9f, 0.3f);

        [Header("显示设置")]
        [SerializeField] private float commandDisplayDuration = 0.8f;
        [SerializeField] private int maxHistoryDisplay = 8;

        // 命令历史
        private readonly List<CommandExecutionRequest> _commandHistory = new List<CommandExecutionRequest>();
        private CommandExecutionRequest? _lastCommand;
        private float _displayTimer;

        // 统计
        private int _singleCount;
        private int _comboCount;
        private int _sequenceCount;
        private int _perfectCount;

        private void Start()
        {
            // 自动查找组件
            if (commandResolver == null)
                commandResolver = FindObjectOfType<CommandResolver>();
            if (inputRouter == null)
                inputRouter = FindObjectOfType<GameplayInputRouter>();
            if (beatClockSystem == null)
                beatClockSystem = FindObjectOfType<BeatClockSystem>();

            // 订阅命令事件
            if (commandResolver != null)
            {
                commandResolver.OnCommandResolved += HandleCommandResolved;
            }

            Debug.Log("[CommandDebugPanel] 初始化完成");
        }

        private void OnDestroy()
        {
            if (commandResolver != null)
            {
                commandResolver.OnCommandResolved -= HandleCommandResolved;
            }
        }

        private void Update()
        {
            if (_displayTimer > 0)
            {
                _displayTimer -= Time.deltaTime;
            }

            UpdateUI();
        }

        private void HandleCommandResolved(CommandExecutionRequest request)
        {
            _lastCommand = request;
            _displayTimer = commandDisplayDuration;

            // 添加到历史
            _commandHistory.Add(request);
            if (_commandHistory.Count > maxHistoryDisplay)
            {
                _commandHistory.RemoveAt(0);
            }

            // 更新统计
            UpdateStats(request);

            // 更新视觉反馈
            if (commandFlashImage != null)
            {
                commandFlashImage.color = GetCommandColor(request);
            }
        }

        private void UpdateStats(CommandExecutionRequest request)
        {
            if (request.commandType.IsSequenceCommand())
            {
                _sequenceCount++;
            }
            else if (request.commandType.IsComboCommand())
            {
                _comboCount++;
            }
            else
            {
                _singleCount++;
            }

            if (request.isPerfectTiming)
            {
                _perfectCount++;
            }
        }

        private void UpdateUI()
        {
            if (_lastCommand.HasValue && _displayTimer > 0)
            {
                var cmd = _lastCommand.Value;

                if (commandNameText != null)
                {
                    commandNameText.text = cmd.commandType.GetDisplayName();
                    commandNameText.color = GetCommandColor(cmd);
                }

                if (commandDetailText != null)
                {
                    string perfect = cmd.isPerfectTiming ? "★" : "";
                    string inputs = cmd.triggerInputs.Length > 0
                        ? string.Join("+", cmd.triggerInputs)
                        : "?";
                    commandDetailText.text = $"{perfect} Beat {cmd.sourceBeatIndex} ({inputs})";
                }
            }
        }

        private Color GetCommandColor(CommandExecutionRequest request)
        {
            if (request.isPerfectTiming)
                return perfectColor;

            if (request.commandType.IsSequenceCommand())
                return sequenceColor;

            if (request.commandType.IsComboCommand())
                return comboColor;

            return singleColor;
        }

        private void OnGUI()
        {
            // 左侧 - 最近输入
            DrawInputPanel();

            // 右侧 - 命令历史
            DrawCommandPanel();

            // 底部 - 统计
            DrawStatsPanel();

            // 命令说明
            DrawCommandReference();
        }

        private void DrawInputPanel()
        {
            GUILayout.BeginArea(new Rect(10, 10, 280, 300));
            GUILayout.BeginVertical("box");

            GUILayout.Label("═══ Recent Inputs ═══");

            if (inputRouter != null)
            {
                var samples = inputRouter.InputBuffer.GetAllSamples();
                int startIndex = Mathf.Max(0, samples.Count - 8);

                for (int i = samples.Count - 1; i >= startIndex; i--)
                {
                    var sample = samples[i];
                    GUI.color = sample.isConsumed ? Color.gray : GetInputColor(sample.judgeResult);

                    string consumed = sample.isConsumed ? "[✓]" : "[ ]";
                    string timing = sample.deltaMs >= 0 ? $"+{sample.deltaMs:F0}" : $"{sample.deltaMs:F0}";

                    GUILayout.Label($"{consumed} [{sample.quantizedBeatIndex}] {sample.inputType} {timing}ms {sample.judgeResult}");
                }

                GUI.color = Color.white;
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawCommandPanel()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 310, 350));
            GUILayout.BeginVertical("box");

            GUILayout.Label("═══ Command History ═══");

            // 当前命令（大字显示）
            if (_lastCommand.HasValue && _displayTimer > 0)
            {
                var cmd = _lastCommand.Value;
                GUI.color = GetCommandColor(cmd);

                GUIStyle bigStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 24,
                    fontStyle = FontStyle.Bold
                };

                string perfect = cmd.isPerfectTiming ? "★ " : "";
                GUILayout.Label($"{perfect}{cmd.commandType.GetDisplayName()}", bigStyle);

                GUI.color = Color.white;
                GUILayout.Label($"Beat {cmd.sourceBeatIndex} | {string.Join("+", cmd.triggerInputs)}");
                GUILayout.Space(10);
            }

            GUILayout.Label("─── History ───");

            // 历史记录
            for (int i = _commandHistory.Count - 1; i >= 0; i--)
            {
                var cmd = _commandHistory[i];
                GUI.color = GetCommandColor(cmd);

                string perfect = cmd.isPerfectTiming ? "★" : " ";
                string type = GetCommandTypeLabel(cmd.commandType);
                GUILayout.Label($"{perfect} [{cmd.sourceBeatIndex}] {cmd.commandType.GetDisplayName()} {type}");
            }

            GUI.color = Color.white;

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawStatsPanel()
        {
            int total = _singleCount + _comboCount + _sequenceCount;

            GUILayout.BeginArea(new Rect(Screen.width - 220, Screen.height - 140, 210, 130));
            GUILayout.BeginVertical("box");

            GUILayout.Label("═══ Command Stats ═══");

            GUI.color = singleColor;
            GUILayout.Label($"Single: {_singleCount}");
            GUI.color = comboColor;
            GUILayout.Label($"Combo: {_comboCount}");
            GUI.color = sequenceColor;
            GUILayout.Label($"Sequence: {_sequenceCount}");
            GUI.color = perfectColor;
            GUILayout.Label($"Perfect: {_perfectCount}");
            GUI.color = Color.white;
            GUILayout.Label($"Total: {total}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawCommandReference()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 220, 280, 210));
            GUILayout.BeginVertical("box");

            GUILayout.Label("═══ Command Reference ═══");
            GUILayout.Label("── Single ──");
            GUILayout.Label("W/↑ = Lift (提势)");
            GUILayout.Label("J   = Flick (轻斩)");
            GUILayout.Label("K   = Shake (短刺)");
            GUILayout.Label("L   = Flash (闪避)");

            GUILayout.Label("── Combo (同拍) ──");
            GUILayout.Label("J+L = DashSlash (冲刺斩)");
            GUILayout.Label("J+W = RisingStrike (上挑)");
            GUILayout.Label("W+L = ParryGuard (弹反)");

            GUILayout.Label("── Sequence (连续拍) ──");
            GUILayout.Label("J→K = ComboStab (连刺)");
            GUILayout.Label("K→J = CounterSlash (反击)");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private Color GetInputColor(RhythmJudgeResult result)
        {
            return result switch
            {
                RhythmJudgeResult.Perfect => perfectColor,
                RhythmJudgeResult.Good => Color.green,
                RhythmJudgeResult.Miss => Color.red,
                _ => Color.white
            };
        }

        private string GetCommandTypeLabel(CommandType type)
        {
            if (type.IsSequenceCommand()) return "[SEQ]";
            if (type.IsComboCommand()) return "[CMB]";
            return "[SGL]";
        }

        /// <summary>
        /// 重置统计
        /// </summary>
        public void ResetStats()
        {
            _singleCount = 0;
            _comboCount = 0;
            _sequenceCount = 0;
            _perfectCount = 0;
            _commandHistory.Clear();
        }
    }
}