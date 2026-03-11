using ShadowRhythm.Data.Models;
using UnityEngine;

namespace ShadowRhythm.Rhythm
{
    /// <summary>
    /// 歌曲运行时数据，负责节拍计算
    /// </summary>
    public sealed class SongRuntime
    {
        public string SongId { get; private set; }
        public string DisplayName { get; private set; }
        public string MusicFileName { get; private set; }
        public float Bpm { get; private set; }
        public float OffsetSeconds { get; private set; }
        public int BeatsPerBar { get; private set; }
        public float SecondsPerBeat { get; private set; }

        public SongRuntime(SongMetaModel meta)
        {
            SongId = meta.songId;
            DisplayName = meta.displayName;
            MusicFileName = meta.musicFileName;
            Bpm = meta.bpm;
            OffsetSeconds = meta.offsetSeconds;
            BeatsPerBar = meta.beatsPerBar;
            SecondsPerBeat = 60f / Bpm;
        }

        /// <summary>
        /// 获取指定拍序号对应的时间点
        /// </summary>
        public float GetBeatTime(int beatIndex)
        {
            return OffsetSeconds + beatIndex * SecondsPerBeat;
        }

        /// <summary>
        /// 获取指定时间对应的拍序号（向下取整）
        /// </summary>
        public int GetBeatIndexAtTime(float songTime)
        {
            float adjustedTime = songTime - OffsetSeconds;
            if (adjustedTime < 0) return -1;
            return Mathf.FloorToInt(adjustedTime / SecondsPerBeat);
        }

        /// <summary>
        /// 获取指定时间的完整节拍帧信息
        /// </summary>
        public BeatFrame GetBeatFrameAtTime(float songTime)
        {
            float adjustedTime = songTime - OffsetSeconds;

            // 计算当前拍序号
            int beatIndex = adjustedTime >= 0 ? Mathf.FloorToInt(adjustedTime / SecondsPerBeat) : -1;

            // 计算小节序号和小节内拍号
            int barIndex = beatIndex >= 0 ? beatIndex / BeatsPerBar : -1;
            int beatInBar = beatIndex >= 0 ? beatIndex % BeatsPerBar : 0;

            // 计算拍内进度 (0.0 ~ 1.0)
            float beatProgress = 0f;
            if (adjustedTime >= 0)
            {
                float timeInCurrentBeat = adjustedTime - (beatIndex * SecondsPerBeat);
                beatProgress = timeInCurrentBeat / SecondsPerBeat;
            }

            // 计算最近拍点时间
            float nearestBeatTime = GetBeatTime(beatIndex >= 0 ? beatIndex : 0);

            // 计算距离最近拍点的偏差（考虑前后两个拍点）
            float currentBeatTime = GetBeatTime(beatIndex);
            float nextBeatTime = GetBeatTime(beatIndex + 1);

            float deltaToCurrentBeat = songTime - currentBeatTime;
            float deltaToNextBeat = songTime - nextBeatTime;

            // 选择更近的拍点
            float deltaMs;
            if (Mathf.Abs(deltaToCurrentBeat) <= Mathf.Abs(deltaToNextBeat))
            {
                nearestBeatTime = currentBeatTime;
                deltaMs = deltaToCurrentBeat * 1000f;
            }
            else
            {
                nearestBeatTime = nextBeatTime;
                deltaMs = deltaToNextBeat * 1000f;
            }

            return new BeatFrame(
                beatIndex,
                barIndex,
                beatInBar,
                beatProgress,
                nearestBeatTime,
                songTime,
                deltaMs
            );
        }
    }
}