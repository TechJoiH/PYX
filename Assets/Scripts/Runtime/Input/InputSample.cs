using UnityEngine;

namespace ShadowRhythm.Input
{
    /// <summary>
    /// 输入采样 - 记录一次玩家输入的完整信息
    /// </summary>
    public struct InputSample
    {
        /// <summary>输入类型</summary>
        public RhythmInputType inputType;

        /// <summary>按下时的歌曲时间（秒）</summary>
        public float pressedSongTime;

        /// <summary>量化到的拍点序号</summary>
        public int quantizedBeatIndex;

        /// <summary>距离最近拍点的偏差（毫秒，正数晚，负数早）</summary>
        public float deltaMs;

        /// <summary>节奏判定结果</summary>
        public RhythmJudgeResult judgeResult;

        /// <summary>输入的帧序号（用于去重）</summary>
        public int frameCount;

        /// <summary>是否已被消耗（用于命令解析）</summary>
        public bool isConsumed;

        public InputSample(
            RhythmInputType type,
            float songTime,
            int beatIndex,
            float delta,
            RhythmJudgeResult result,
            int frame = 0)
        {
            inputType = type;
            pressedSongTime = songTime;
            quantizedBeatIndex = beatIndex;
            deltaMs = delta;
            judgeResult = result;
            frameCount = frame;
            isConsumed = false;
        }

        /// <summary>
        /// 标记为已消耗
        /// </summary>
        public void MarkConsumed()
        {
            isConsumed = true;
        }

        /// <summary>
        /// 是否是有效输入（非 None 且有判定）
        /// </summary>
        public bool IsValid => inputType != RhythmInputType.None;

        /// <summary>
        /// 是否是精确输入
        /// </summary>
        public bool IsPerfect => judgeResult == RhythmJudgeResult.Perfect;

        public override string ToString()
        {
            string timing = deltaMs >= 0 ? $"+{deltaMs:F1}ms" : $"{deltaMs:F1}ms";
            return $"[{inputType}] Beat:{quantizedBeatIndex} {timing} -> {judgeResult}";
        }
    }
}