using System;
using System.Collections.Generic;
using UnityEngine;
using ShadowRhythm.Input;
using ShadowRhythm.Rhythm;

namespace ShadowRhythm.Command
{
    /// <summary>
    /// 命令解析器 - 将输入转换为命令执行请求
    /// </summary>
    public sealed class CommandResolver : MonoBehaviour
    {
        [Header("依赖")]
        [SerializeField] private GameplayInputRouter inputRouter;
        [SerializeField] private BeatClockSystem beatClockSystem;

        [Header("设置")]
        [SerializeField] private bool enableDebugLog = true;

        /// <summary>命令模式匹配器</summary>
        public CommandPatternMatcher PatternMatcher { get; private set; }

        /// <summary>命令优先级表</summary>
        public CommandPriorityTable PriorityTable { get; private set; }

        /// <summary>命令上下文</summary>
        public CommandContext Context { get; private set; }

        /// <summary>是否启用</summary>
        public bool IsEnabled { get; private set; } = true;

        /// <summary>上一个解析出的命令</summary>
        public CommandExecutionRequest LastResolvedCommand { get; private set; }

        // 已处理的拍点记录（防止重复解析）
        private readonly HashSet<int> _processedBeats = new HashSet<int>();

        /// <summary>命令解析成功事件</summary>
        public event Action<CommandExecutionRequest> OnCommandResolved;

        private void Awake()
        {
            PatternMatcher = new CommandPatternMatcher();
            PriorityTable = new CommandPriorityTable();
            Context = new CommandContext();

            // 自动查找依赖
            if (inputRouter == null)
                inputRouter = FindObjectOfType<GameplayInputRouter>();
            if (beatClockSystem == null)
                beatClockSystem = FindObjectOfType<BeatClockSystem>();
        }

        private void OnEnable()
        {
            if (inputRouter != null)
            {
                inputRouter.OnInputReceived += HandleInputReceived;
            }
        }

        private void OnDisable()
        {
            if (inputRouter != null)
            {
                inputRouter.OnInputReceived -= HandleInputReceived;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(GameplayInputRouter router, BeatClockSystem clock)
        {
            inputRouter = router;
            beatClockSystem = clock;

            if (inputRouter != null)
            {
                inputRouter.OnInputReceived += HandleInputReceived;
            }

            Debug.Log("[CommandResolver] 初始化完成");
        }

        /// <summary>
        /// 启用解析
        /// </summary>
        public void Enable()
        {
            IsEnabled = true;
        }

        /// <summary>
        /// 禁用解析
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            Context.Reset();
            _processedBeats.Clear();
            LastResolvedCommand = CommandExecutionRequest.Empty;
        }

        /// <summary>
        /// 手动尝试解析命令
        /// </summary>
        public bool TryResolve(out CommandExecutionRequest request)
        {
            request = CommandExecutionRequest.Empty;

            if (!IsEnabled || inputRouter == null || beatClockSystem == null)
                return false;

            // 更新上下文
            UpdateContext();

            // 尝试匹配模式
            var matchResult = PatternMatcher.TryMatch(Context);

            if (!matchResult.IsValid)
                return false;

            // 创建命令请求
            request = new CommandExecutionRequest(
                matchResult.commandType,
                Context.CurrentBeatIndex,
                matchResult.isPerfect,
                matchResult.deltaMs,
                matchResult.consumedInputs
            );

            // 标记输入为已消耗
            MarkInputsConsumed(matchResult);

            // 更新状态
            LastResolvedCommand = request;
            Context.LastCommand = request.commandType;
            Context.LastCommandBeatIndex = request.sourceBeatIndex;

            return true;
        }

        private void HandleInputReceived(InputSample sample)
        {
            if (!IsEnabled) return;

            // 延迟一帧处理，确保同拍的所有输入都已收集
            // 使用协程或在 LateUpdate 中处理
            ProcessInputAtBeat(sample.quantizedBeatIndex);
        }

        private void ProcessInputAtBeat(int beatIndex)
        {
            // 检查是否已处理过这一拍
            // 注意：这里简化处理，实际可能需要等待一个短暂窗口收集同拍输入
            if (_processedBeats.Contains(beatIndex))
                return;

            // 更新上下文
            UpdateContext();

            // 尝试解析
            if (TryResolve(out var request))
            {
                _processedBeats.Add(beatIndex);

                // 清理旧记录
                CleanupProcessedBeats(beatIndex);

                // 触发事件
                OnCommandResolved?.Invoke(request);

                if (enableDebugLog)
                {
                    Debug.Log($"[CommandResolver] 解析成功: {request}");
                }
            }
        }

        private void UpdateContext()
        {
            if (beatClockSystem == null || inputRouter == null) return;

            var frame = beatClockSystem.CurrentBeatFrame;
            var buffer = inputRouter.InputBuffer;

            Context.CurrentBeatIndex = frame.beatIndex;
            Context.CurrentSongTime = frame.songTime;

            // 获取当前拍和上一拍的输入
            Context.CurrentBeatInputs = buffer.GetSamplesAtBeat(frame.beatIndex);
            Context.PreviousBeatInputs = buffer.GetSamplesAtBeat(frame.beatIndex - 1);

            // 获取最近的所有输入
            Context.RecentInputs = buffer.GetRecentSamples(frame.beatIndex, 4);
        }

        private void MarkInputsConsumed(PatternMatchResult result)
        {
            if (inputRouter == null) return;

            for (int i = 0; i < result.consumedInputs.Length && i < result.consumedBeatIndices.Length; i++)
            {
                inputRouter.InputBuffer.MarkConsumed(
                    result.consumedBeatIndices[i],
                    result.consumedInputs[i]
                );
            }
        }

        private void CleanupProcessedBeats(int currentBeat)
        {
            // 只保留最近8拍的记录
            _processedBeats.RemoveWhere(b => b < currentBeat - 8);
        }
    }
}