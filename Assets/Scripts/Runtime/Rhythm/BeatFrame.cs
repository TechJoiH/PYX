namespace ShadowRhythm.Rhythm
{
    /// <summary>
    /// 某个时刻对应的节拍快照
    /// </summary>
    public readonly struct BeatFrame
    {
        /// <summary>当前拍序号（从0开始）</summary>
        public readonly int beatIndex;

        /// <summary>小节序号（从0开始）</summary>
        public readonly int barIndex;

        /// <summary>小节内的拍号（0 ~ beatsPerBar-1）</summary>
        public readonly int beatInBar;

        /// <summary>当前拍内进度（0.0 ~ 1.0）</summary>
        public readonly float beatProgress;

        /// <summary>最近拍点时间（秒）</summary>
        public readonly float nearestBeatTime;

        /// <summary>当前歌曲时间（秒）</summary>
        public readonly float songTime;

        /// <summary>距离最近拍点的偏差（毫秒，负数表示提前）</summary>
        public readonly float deltaMs;

        public BeatFrame(
            int beatIndex,
            int barIndex,
            int beatInBar,
            float beatProgress,
            float nearestBeatTime,
            float songTime,
            float deltaMs)
        {
            this.beatIndex = beatIndex;
            this.barIndex = barIndex;
            this.beatInBar = beatInBar;
            this.beatProgress = beatProgress;
            this.nearestBeatTime = nearestBeatTime;
            this.songTime = songTime;
            this.deltaMs = deltaMs;
        }

        public override string ToString()
        {
            return $"[Beat {beatIndex}] Bar:{barIndex} BeatInBar:{beatInBar + 1}/{beatInBar + 1} Progress:{beatProgress:F2} Delta:{deltaMs:F1}ms";
        }
    }
}