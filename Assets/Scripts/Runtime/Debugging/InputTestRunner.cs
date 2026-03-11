using UnityEngine;
using ShadowRhythm.Core.Audio;
using ShadowRhythm.Core.Persistence;
using ShadowRhythm.Data.Models;
using ShadowRhythm.Input;
using ShadowRhythm.Rhythm;

namespace ShadowRhythm.Debugging
{
    /// <summary>
    /// 输入测试运行器 - 用于 Sandbox_Input 场景
    /// </summary>
    public sealed class InputTestRunner : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private string songId = "001";
        [SerializeField] private AudioClip testMusicClip;
        [SerializeField] private float startDelay = 1f;

        [Header("组件引用")]
        [SerializeField] private MusicPlaybackService musicPlaybackService;
        [SerializeField] private BeatClockSystem beatClockSystem;
        [SerializeField] private GameplayInputRouter inputRouter;
        [SerializeField] private BeatClockDebugView beatDebugView;
        [SerializeField] private InputDebugPanel inputDebugPanel;

        [Header("视觉反馈")]
        [SerializeField] private Transform beatPulseObject;

        private JsonLoadBridge _jsonLoadBridge;
        private SongRuntime _songRuntime;
        private Vector3 _originalScale;

        // 统计
        private int _perfectCount;
        private int _goodCount;
        private int _missCount;

        private void Awake()
        {
            _jsonLoadBridge = new JsonLoadBridge(JsonDataManager.Instance);

            // 自动查找组件
            if (musicPlaybackService == null)
                musicPlaybackService = FindObjectOfType<MusicPlaybackService>();
            if (beatClockSystem == null)
                beatClockSystem = FindObjectOfType<BeatClockSystem>();
            if (inputRouter == null)
                inputRouter = FindObjectOfType<GameplayInputRouter>();
            if (beatDebugView == null)
                beatDebugView = FindObjectOfType<BeatClockDebugView>();
            if (inputDebugPanel == null)
                inputDebugPanel = FindObjectOfType<InputDebugPanel>();

            if (beatPulseObject != null)
                _originalScale = beatPulseObject.localScale;
        }

        private void Start()
        {
            InitializeTest();
        }

        private void Update()
        {
            HandleTestControls();
            UpdatePulseVisual();
        }

        private void InitializeTest()
        {
            Debug.Log("========== 板块2 输入测试 ==========");

            // 1. 加载歌曲配置
            var songMeta = _jsonLoadBridge.LoadSongMeta(songId);
            if (songMeta == null)
            {
                Debug.LogError($"[InputTest] 无法加载歌曲配置: {songId}");
                return;
            }

            _songRuntime = new SongRuntime(songMeta);
            Debug.Log($"[InputTest] 歌曲: {_songRuntime.DisplayName} | BPM: {_songRuntime.Bpm}");

            // 2. 加载判定窗口配置
            var judgeConfig = _jsonLoadBridge.LoadJudgeWindowConfig();
            var evaluator = new InputWindowEvaluator(judgeConfig);

            // 3. 确保组件存在
            EnsureComponents();

            // 4. 加载音乐
            if (testMusicClip != null)
            {
                musicPlaybackService.LoadClip(testMusicClip);
            }
            else
            {
                Debug.LogWarning("[InputTest] 未指定测试音乐！");
            }

            // 5. 初始化系统
            beatClockSystem.Initialize(_songRuntime, musicPlaybackService);
            inputRouter.Initialize(beatClockSystem, evaluator);

            // 6. 订阅事件
            beatClockSystem.OnNewBeat += OnBeatTick;
            inputRouter.OnInputReceived += OnInputReceived;

            // 7. 开始测试
            StartTest();
        }

        private void EnsureComponents()
        {
            if (musicPlaybackService == null)
            {
                var go = new GameObject("MusicPlaybackService");
                musicPlaybackService = go.AddComponent<MusicPlaybackService>();
            }

            if (beatClockSystem == null)
            {
                var go = new GameObject("BeatClockSystem");
                beatClockSystem = go.AddComponent<BeatClockSystem>();
            }

            if (inputRouter == null)
            {
                var go = new GameObject("GameplayInputRouter");
                inputRouter = go.AddComponent<GameplayInputRouter>();
            }
        }

        private void StartTest()
        {
            Debug.Log($"[InputTest] {startDelay}秒后开始...");

            musicPlaybackService.Play(startDelay);
            beatClockSystem.StartClock();
            inputRouter.EnableInput();

            _perfectCount = 0;
            _goodCount = 0;
            _missCount = 0;

            Debug.Log("[InputTest] 使用 W/↑, J, K, L 测试输入");
            Debug.Log("[InputTest] 按 Space 暂停/恢复 | R 重新开始");
        }

        private void HandleTestControls()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                if (musicPlaybackService.IsPlaying)
                {
                    musicPlaybackService.Pause();
                    beatClockSystem.Pause();
                    Debug.Log("[InputTest] === 已暂停 ===");
                }
                else
                {
                    musicPlaybackService.Resume();
                    beatClockSystem.Resume();
                    Debug.Log("[InputTest] === 已恢复 ===");
                }
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                RestartTest();
            }

            // 打印统计
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
            {
                PrintStats();
            }
        }

        private void RestartTest()
        {
            musicPlaybackService.Stop();
            beatClockSystem.StopClock();
            inputRouter.ClearBuffer();
            StartTest();
            Debug.Log("[InputTest] === 已重新开始 ===");
        }

        private void OnBeatTick(BeatFrame frame)
        {
            if (beatPulseObject != null)
            {
                beatPulseObject.localScale = _originalScale * 1.3f;
            }
        }

        private void OnInputReceived(InputSample sample)
        {
            // 更新统计
            switch (sample.judgeResult)
            {
                case RhythmJudgeResult.Perfect:
                    _perfectCount++;
                    break;
                case RhythmJudgeResult.Good:
                    _goodCount++;
                    break;
                case RhythmJudgeResult.Miss:
                    _missCount++;
                    break;
            }
        }

        private void UpdatePulseVisual()
        {
            if (beatPulseObject == null) return;

            beatPulseObject.localScale = Vector3.Lerp(
                beatPulseObject.localScale,
                _originalScale,
                Time.deltaTime * 10f
            );
        }

        private void PrintStats()
        {
            int total = _perfectCount + _goodCount + _missCount;
            float accuracy = total > 0 ? (float)_perfectCount / total * 100f : 0f;

            Debug.Log($"========== 输入统计 ==========");
            Debug.Log($"Perfect: {_perfectCount}");
            Debug.Log($"Good: {_goodCount}");
            Debug.Log($"Miss: {_missCount}");
            Debug.Log($"Total: {total}");
            Debug.Log($"Accuracy: {accuracy:F1}%");
            Debug.Log($"==============================");
        }

        private void OnDestroy()
        {
            if (beatClockSystem != null)
                beatClockSystem.OnNewBeat -= OnBeatTick;
            if (inputRouter != null)
                inputRouter.OnInputReceived -= OnInputReceived;
        }

        private void OnGUI()
        {
            // 右下角显示统计
            GUILayout.BeginArea(new Rect(Screen.width - 200, Screen.height - 120, 190, 110));
            GUILayout.BeginVertical("box");

            int total = _perfectCount + _goodCount + _missCount;
            float accuracy = total > 0 ? (float)_perfectCount / total * 100f : 0f;

            GUILayout.Label("═══ Statistics ═══");
            GUI.color = new Color(1f, 0.84f, 0f);
            GUILayout.Label($"Perfect: {_perfectCount}");
            GUI.color = new Color(0.2f, 0.8f, 0.2f);
            GUILayout.Label($"Good: {_goodCount}");
            GUI.color = new Color(0.8f, 0.2f, 0.2f);
            GUILayout.Label($"Miss: {_missCount}");
            GUI.color = Color.white;
            GUILayout.Label($"Accuracy: {accuracy:F1}%");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}