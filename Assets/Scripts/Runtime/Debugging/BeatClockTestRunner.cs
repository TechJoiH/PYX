using UnityEngine;
using ShadowRhythm.Core.Audio;
using ShadowRhythm.Core.Persistence;
using ShadowRhythm.Data.Models;
using ShadowRhythm.Rhythm;

namespace ShadowRhythm.Debugging
{
    /// <summary>
    /// 节拍时钟测试运行器 - 用于 Sandbox_BeatClock 场景
    /// </summary>
    public sealed class BeatClockTestRunner : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private string songId = "001";
        [SerializeField] private AudioClip testMusicClip;
        [SerializeField] private float startDelay = 1f;

        [Header("组件引用")]
        [SerializeField] private MusicPlaybackService musicPlaybackService;
        [SerializeField] private BeatClockSystem beatClockSystem;
        [SerializeField] private BeatClockDebugView debugView;

        [Header("视觉测试")]
        [SerializeField] private Transform beatPulseObject;
        [SerializeField] private float pulseScale = 1.5f;
        [SerializeField] private float normalScale = 1f;

        private JsonLoadBridge _jsonLoadBridge;
        private SongRuntime _songRuntime;
        private Vector3 _originalScale;

        private void Awake()
        {
            // 初始化 JSON 加载
            _jsonLoadBridge = new JsonLoadBridge(JsonDataManager.Instance);

            // 自动查找组件
            if (musicPlaybackService == null)
                musicPlaybackService = FindObjectOfType<MusicPlaybackService>();

            if (beatClockSystem == null)
                beatClockSystem = FindObjectOfType<BeatClockSystem>();

            if (debugView == null)
                debugView = FindObjectOfType<BeatClockDebugView>();

            // 记录原始缩放
            if (beatPulseObject != null)
                _originalScale = beatPulseObject.localScale;
        }

        private void Start()
        {
            InitializeTest();
        }

        private void Update()
        {
            // 按键控制
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                
            {
                if (musicPlaybackService.IsPlaying)
                    PauseTest();
                else
                    ResumeTest();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                RestartTest();
            }

            // 更新脉冲视觉效果
            UpdatePulseVisual();
        }

        private void InitializeTest()
        {
            Debug.Log("========== 板块1 节拍时钟测试 ==========");

            // 1. 加载歌曲配置
            var songMeta = _jsonLoadBridge.LoadSongMeta(songId);
            if (songMeta == null)
            {
                Debug.LogError($"[BeatClockTest] 无法加载歌曲配置: {songId}");
                return;
            }

            _songRuntime = new SongRuntime(songMeta);
            Debug.Log($"[BeatClockTest] 歌曲加载: {_songRuntime.DisplayName}");
            Debug.Log($"[BeatClockTest] BPM: {_songRuntime.Bpm} | 每拍: {_songRuntime.SecondsPerBeat:F4}s");

            // 2. 确保有 MusicPlaybackService
            if (musicPlaybackService == null)
            {
                var go = new GameObject("MusicPlaybackService");
                musicPlaybackService = go.AddComponent<MusicPlaybackService>();
            }

            // 3. 确保有 BeatClockSystem
            if (beatClockSystem == null)
            {
                var go = new GameObject("BeatClockSystem");
                beatClockSystem = go.AddComponent<BeatClockSystem>();
            }

            // 4. 加载音乐
            if (testMusicClip != null)
            {
                musicPlaybackService.LoadClip(testMusicClip);
            }
            else
            {
                Debug.LogWarning("[BeatClockTest] 未指定测试音乐，请在 Inspector 中拖入 AudioClip");
            }

            // 5. 初始化节拍时钟
            beatClockSystem.Initialize(_songRuntime, musicPlaybackService);

            // 6. 订阅事件
            beatClockSystem.OnNewBeat += OnBeatTick;
            beatClockSystem.OnNewBar += OnBarTick;

            // 7. 开始播放
            StartTest();
        }

        private void StartTest()
        {
            Debug.Log($"[BeatClockTest] {startDelay}秒后开始播放...");

            musicPlaybackService.Play(startDelay);
            beatClockSystem.StartClock();

            Debug.Log("[BeatClockTest] 按 Space 暂停/恢复 | 按 R 重新开始");
        }

        private void PauseTest()
        {
            musicPlaybackService.Pause();
            beatClockSystem.Pause();
            Debug.Log("[BeatClockTest] === 已暂停 ===");
        }

        private void ResumeTest()
        {
            musicPlaybackService.Resume();
            beatClockSystem.Resume();
            Debug.Log("[BeatClockTest] === 已恢复 ===");
        }

        private void RestartTest()
        {
            musicPlaybackService.Stop();
            beatClockSystem.StopClock();
            StartTest();
            Debug.Log("[BeatClockTest] === 已重新开始 ===");
        }

        private void OnBeatTick(BeatFrame frame)
        {
            // 触发脉冲
            if (beatPulseObject != null)
            {
                beatPulseObject.localScale = _originalScale * pulseScale;
            }
        }

        private void OnBarTick(BeatFrame frame)
        {
            // 新小节可以做更强的视觉反馈
        }

        private void UpdatePulseVisual()
        {
            if (beatPulseObject == null) return;

            // 平滑恢复原始大小
            beatPulseObject.localScale = Vector3.Lerp(
                beatPulseObject.localScale,
                _originalScale * normalScale,
                Time.deltaTime * 10f
            );
        }

        private void OnDestroy()
        {
            if (beatClockSystem != null)
            {
                beatClockSystem.OnNewBeat -= OnBeatTick;
                beatClockSystem.OnNewBar -= OnBarTick;
            }
        }
    }
}