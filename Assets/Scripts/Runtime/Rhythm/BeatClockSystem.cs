using System;
using UnityEngine;
using ShadowRhythm.Core.Audio;

namespace ShadowRhythm.Rhythm
{
    /// <summary>
    /// 节拍时钟系统 - 整个项目的时间真相
    /// 负责根据 DSP 时间计算当前节拍信息
    /// </summary>
    public sealed class BeatClockSystem : MonoBehaviour
    {
        [Header("依赖")]
        [SerializeField] private MusicPlaybackService musicPlaybackService;

        [Header("调试")]
        [SerializeField] private bool enableDebugLog = false;

        /// <summary>当前歌曲运行时</summary>
        public SongRuntime SongRuntime { get; private set; }

        /// <summary>是否正在运行</summary>
        public bool IsRunning { get; private set; }

        /// <summary>当前歌曲时间（秒）</summary>
        public float CurrentSongTime => musicPlaybackService != null ? musicPlaybackService.CurrentSongTime : 0f;

        /// <summary>当前拍序号</summary>
        public int CurrentBeatIndex { get; private set; } = -1;

        /// <summary>当前小节序号</summary>
        public int CurrentBarIndex { get; private set; } = -1;

        /// <summary>当前小节内拍号</summary>
        public int CurrentBeatInBar { get; private set; } = 0;

        /// <summary>当前拍内进度 (0.0 ~ 1.0)</summary>
        public float CurrentBeatProgress { get; private set; } = 0f;

        /// <summary>当前完整节拍帧</summary>
        public BeatFrame CurrentBeatFrame { get; private set; }

        // 上一帧的拍序号，用于检测新拍
        private int _lastBeatIndex = -1;

        /// <summary>新拍事件（每到一个新拍触发）</summary>
        public event Action<BeatFrame> OnNewBeat;

        /// <summary>新小节事件（每到一个新小节触发）</summary>
        public event Action<BeatFrame> OnNewBar;

        /// <summary>每帧更新事件</summary>
        public event Action<BeatFrame> OnBeatFrameUpdated;

        private void Awake()
        {
            if (musicPlaybackService == null)
            {
                musicPlaybackService = GetComponent<MusicPlaybackService>();
                if (musicPlaybackService == null)
                {
                    musicPlaybackService = FindObjectOfType<MusicPlaybackService>();
                }
            }
        }

        private void Update()
        {
            if (!IsRunning || SongRuntime == null) return;

            UpdateBeatFrame();
        }

        /// <summary>
        /// 初始化节拍时钟
        /// </summary>
        public void Initialize(SongRuntime songRuntime, MusicPlaybackService playbackService = null)
        {
            SongRuntime = songRuntime ?? throw new ArgumentNullException(nameof(songRuntime));

            if (playbackService != null)
            {
                musicPlaybackService = playbackService;
            }

            ResetState();

            Debug.Log($"[BeatClockSystem] 初始化完成 - BPM:{songRuntime.Bpm} 每拍:{songRuntime.SecondsPerBeat:F4}s");
        }

        /// <summary>
        /// 开始运行
        /// </summary>
        public void StartClock()
        {
            if (SongRuntime == null)
            {
                Debug.LogError("[BeatClockSystem] 未初始化，无法启动！");
                return;
            }

            IsRunning = true;
            ResetState();
            Debug.Log("[BeatClockSystem] 时钟已启动");
        }

        /// <summary>
        /// 停止运行
        /// </summary>
        public void StopClock()
        {
            IsRunning = false;
            Debug.Log("[BeatClockSystem] 时钟已停止");
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            IsRunning = false;
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            IsRunning = true;
        }

        /// <summary>
        /// 获取当前节拍帧（手动调用）
        /// </summary>
        public BeatFrame GetCurrentBeatFrame()
        {
            if (SongRuntime == null) return default;
            return SongRuntime.GetBeatFrameAtTime(CurrentSongTime);
        }

        /// <summary>
        /// 量化时间到最近拍点
        /// </summary>
        public float QuantizeToNearestBeat(float songTime)
        {
            if (SongRuntime == null) return songTime;

            var frame = SongRuntime.GetBeatFrameAtTime(songTime);
            return frame.nearestBeatTime;
        }

        /// <summary>
        /// 获取到指定拍点的时间差（毫秒）
        /// </summary>
        public float GetDeltaToNearestBeat(float songTime)
        {
            if (SongRuntime == null) return 0f;

            var frame = SongRuntime.GetBeatFrameAtTime(songTime);
            return frame.deltaMs;
        }

        private void UpdateBeatFrame()
        {
            float songTime = CurrentSongTime;
            CurrentBeatFrame = SongRuntime.GetBeatFrameAtTime(songTime);

            CurrentBeatIndex = CurrentBeatFrame.beatIndex;
            CurrentBarIndex = CurrentBeatFrame.barIndex;
            CurrentBeatInBar = CurrentBeatFrame.beatInBar;
            CurrentBeatProgress = CurrentBeatFrame.beatProgress;

            // 检测新拍
            if (CurrentBeatIndex > _lastBeatIndex && CurrentBeatIndex >= 0)
            {
                // 新拍事件
                OnNewBeat?.Invoke(CurrentBeatFrame);

                if (enableDebugLog)
                {
                    Debug.Log($"[BeatClock] Beat {CurrentBeatIndex} | Bar {CurrentBarIndex} | {CurrentBeatInBar + 1}/4 | Time: {songTime:F3}s");
                }

                // 检测新小节
                if (CurrentBeatInBar == 0)
                {
                    OnNewBar?.Invoke(CurrentBeatFrame);
                }
            }

            _lastBeatIndex = CurrentBeatIndex;

            // 每帧更新事件
            OnBeatFrameUpdated?.Invoke(CurrentBeatFrame);
        }

        private void ResetState()
        {
            CurrentBeatIndex = -1;
            CurrentBarIndex = -1;
            CurrentBeatInBar = 0;
            CurrentBeatProgress = 0f;
            _lastBeatIndex = -1;
        }
    }
}