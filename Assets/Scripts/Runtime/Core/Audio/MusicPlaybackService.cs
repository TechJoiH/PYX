using System;
using UnityEngine;

namespace ShadowRhythm.Core.Audio
{
    /// <summary>
    /// 音乐播放服务，负责稳定的音频播放和 DSP 时间同步
    /// </summary>
    public sealed class MusicPlaybackService : MonoBehaviour
    {
        [Header("音频源")]
        [SerializeField] private AudioSource audioSource;

        /// <summary>歌曲开始播放的 DSP 时间</summary>
        public double SongStartDspTime { get; private set; }

        /// <summary>是否正在播放</summary>
        public bool IsPlaying => audioSource != null && audioSource.isPlaying;

        /// <summary>当前歌曲时间（秒），基于 DSP 时间计算</summary>
        public float CurrentSongTime
        {
            get
            {
                if (!IsPlaying && !_isPaused) return 0f;
                if (_isPaused) return _pausedSongTime;
                return (float)(AudioSettings.dspTime - SongStartDspTime);
            }
        }

        /// <summary>当前加载的音频剪辑</summary>
        public AudioClip CurrentClip => audioSource?.clip;

        /// <summary>音频剪辑时长</summary>
        public float ClipLength => audioSource?.clip != null ? audioSource.clip.length : 0f;

        private bool _isPaused;
        private float _pausedSongTime;
        private double _scheduledStartTime;

        /// <summary>播放开始事件</summary>
        public event Action OnPlayStarted;

        /// <summary>播放暂停事件</summary>
        public event Action OnPlayPaused;

        /// <summary>播放恢复事件</summary>
        public event Action OnPlayResumed;

        /// <summary>播放停止事件</summary>
        public event Action OnPlayStopped;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            // 确保音频源配置正确
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        /// <summary>
        /// 加载音频剪辑
        /// </summary>
        public void LoadClip(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogError("[MusicPlaybackService] 音频剪辑为空！");
                return;
            }

            Stop();
            audioSource.clip = clip;
            Debug.Log($"[MusicPlaybackService] 已加载音频: {clip.name} ({clip.length:F2}s)");
        }

        /// <summary>
        /// 播放音乐（使用 PlayScheduled 确保精确时间）
        /// </summary>
        /// <param name="delaySeconds">延迟播放秒数（用于倒计时）</param>
        public void Play(float delaySeconds = 0f)
        {
            if (audioSource.clip == null)
            {
                Debug.LogError("[MusicPlaybackService] 没有加载音频剪辑！");
                return;
            }

            _isPaused = false;
            _pausedSongTime = 0f;

            // 计算播放开始的 DSP 时间
            _scheduledStartTime = AudioSettings.dspTime + delaySeconds;
            SongStartDspTime = _scheduledStartTime;

            // 使用 PlayScheduled 进行精确播放
            audioSource.PlayScheduled(_scheduledStartTime);

            Debug.Log($"[MusicPlaybackService] 音乐将在 {delaySeconds:F3}s 后开始 (DSP: {_scheduledStartTime:F4})");

            OnPlayStarted?.Invoke();
        }

        /// <summary>
        /// 立即播放
        /// </summary>
        public void PlayImmediate()
        {
            Play(0f);
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void Pause()
        {
            if (!IsPlaying) return;

            _pausedSongTime = CurrentSongTime;
            _isPaused = true;
            audioSource.Pause();

            Debug.Log($"[MusicPlaybackService] 已暂停于 {_pausedSongTime:F3}s");

            OnPlayPaused?.Invoke();
        }

        /// <summary>
        /// 恢复播放
        /// </summary>
        public void Resume()
        {
            if (!_isPaused) return;

            // 重新计算 DSP 起始时间
            SongStartDspTime = AudioSettings.dspTime - _pausedSongTime;
            _isPaused = false;

            audioSource.UnPause();

            Debug.Log($"[MusicPlaybackService] 已恢复播放 (新 DSP 起点: {SongStartDspTime:F4})");

            OnPlayResumed?.Invoke();
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            audioSource.Stop();
            _isPaused = false;
            _pausedSongTime = 0f;
            SongStartDspTime = 0;

            Debug.Log("[MusicPlaybackService] 已停止播放");

            OnPlayStopped?.Invoke();
        }

        /// <summary>
        /// 跳转到指定时间
        /// </summary>
        public void SeekTo(float songTime)
        {
            if (audioSource.clip == null) return;

            songTime = Mathf.Clamp(songTime, 0f, ClipLength);

            bool wasPlaying = IsPlaying;
            audioSource.Stop();

            audioSource.time = songTime;
            SongStartDspTime = AudioSettings.dspTime - songTime;
            _isPaused = false;

            if (wasPlaying)
            {
                audioSource.Play();
            }

            Debug.Log($"[MusicPlaybackService] 跳转到 {songTime:F3}s");
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        public void SetVolume(float volume)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// 获取当前 DSP 时间（调试用）
        /// </summary>
        public double GetCurrentDspTime()
        {
            return AudioSettings.dspTime;
        }
    }
}