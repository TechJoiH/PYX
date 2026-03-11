using ShadowRhythm.Input;

namespace ShadowRhythm.Command
{
    /// <summary>
    /// 命令执行请求 - 包含要执行的命令及其上下文信息
    /// </summary>
    public struct CommandExecutionRequest
    {
        /// <summary>命令类型</summary>
        public CommandType commandType;

        /// <summary>来源拍点序号</summary>
        public int sourceBeatIndex;

        /// <summary>是否是精确时机</summary>
        public bool isPerfectTiming;

        /// <summary>命令优先级</summary>
        public int priority;

        /// <summary>触发此命令的输入类型（用于调试）</summary>
        public RhythmInputType[] triggerInputs;

        /// <summary>时间偏差（毫秒）</summary>
        public float deltaMs;

        /// <summary>是否有效</summary>
        public bool IsValid => commandType != CommandType.None;

        public CommandExecutionRequest(
            CommandType type,
            int beatIndex,
            bool perfect,
            float delta = 0f,
            params RhythmInputType[] inputs)
        {
            commandType = type;
            sourceBeatIndex = beatIndex;
            isPerfectTiming = perfect;
            priority = type.GetPriority();
            deltaMs = delta;
            triggerInputs = inputs ?? System.Array.Empty<RhythmInputType>();
        }

        /// <summary>
        /// 创建空请求
        /// </summary>
        public static CommandExecutionRequest Empty => new CommandExecutionRequest(CommandType.None, -1, false);

        public override string ToString()
        {
            if (!IsValid) return "[Empty Command]";

            string timing = isPerfectTiming ? "★Perfect" : "○Normal";
            string inputs = triggerInputs.Length > 0 ? string.Join("+", triggerInputs) : "?";
            return $"[{commandType.GetDisplayName()}] Beat:{sourceBeatIndex} {timing} ({inputs})";
        }
    }
}