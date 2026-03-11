using UnityEngine;
using ShadowRhythm.Core.Audio;
using ShadowRhythm.Core.Persistence;
using ShadowRhythm.Command;
using ShadowRhythm.Input;
using ShadowRhythm.Rhythm;

namespace ShadowRhythm.Debugging
{
    /// <summary>
    /// 命令测试运行器 - 用于 Sandbox_Command 场景
    /// </summary>
    public sealed class CommandTestRunner : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private string songId = "001";
        [SerializeField] private AudioClip testMusicClip;
        [SerializeField] private float startDelay = 1f;

        [Header("场景组件")]
        [SerializeField] private MusicPlaybackService musicPlaybackService;
        [SerializeField] private BeatClockSystem beatClockSystem;
        [SerializeField] private GameplayInputRouter inputRouter;
        [SerializeField] private CommandResolver commandResolver;
        [SerializeField] private CommandDebugPanel debugPanel;

        [Header("视觉反馈")]
        [SerializeField] private Transform beatPulseObject;

        private JsonLoadBridge _jsonLoadBridge;
        private SongRuntime _songRuntime;
        private Vector3 _originalScale;

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
            if (commandResolver == null)
                commandResolver = FindObjectOfType<CommandResolver>();
            if (debugPanel == null)
                debugPanel = FindObjectOfType<CommandDebugPanel>();

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
            Debug.Log("========== 层级3 命令测试开始 ==========");

            // 1. 加载歌曲数据
            var songMeta = _jsonLoadBridge.LoadSongMeta(songId);
            if (songMeta == null)
            {
                Debug.LogError($"[CommandTest] 无法加载歌曲数据: {songId}");
                return;
            }

            _songRuntime = new SongRuntime(songMeta);
            Debug.Log($"[CommandTest] 歌曲: {_songRuntime.DisplayName} | BPM: {_songRuntime.Bpm}");

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
                Debug.LogWarning("[CommandTest] 未指定测试音乐！");
            }

            // 5. 初始化系统
            beatClockSystem.Initialize(_songRuntime, musicPlaybackService);
            inputRouter.Initialize(beatClockSystem, evaluator);
            commandResolver.Initialize(inputRouter, beatClockSystem);

            // 6. 订阅事件
            beatClockSystem.OnNewBeat += OnBeatTick;
            commandResolver.OnCommandResolved += OnCommandResolved;

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

            if (commandResolver == null)
            {
                var go = new GameObject("CommandResolver");
                commandResolver = go.AddComponent<CommandResolver>();
            }
        }

        private void StartTest()
        {
            Debug.Log($"[CommandTest] {startDelay}秒后开始...");

            musicPlaybackService.Play(startDelay);
            beatClockSystem.StartClock();
            inputRouter.EnableInput();
            commandResolver.Enable();

            Debug.Log("[CommandTest] ═══════════════════════════════════════");
            Debug.Log("[CommandTest] 命令输入说明：");
            Debug.Log("[CommandTest] 单键: ↑=抬起, ↓=摇动, ←=轻弹, →=闪避");
            Debug.Log("[CommandTest] 同拍组合: ↑+←=上劈, ↓+←=弹反, →+←=闪击");
            Debug.Log("[CommandTest] 连续键: ←→←=连刺, →←→=反击");
            Debug.Log("[CommandTest] ═══════════════════════════════════════");
            Debug.Log("[CommandTest] 按 Space 暂停/恢复 | R 重新开始 | Tab 查看统计");
        }

        private void HandleTestControls()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                if (musicPlaybackService.IsPlaying)
                {
                    musicPlaybackService.Pause();
                    beatClockSystem.Pause();
                    Debug.Log("[CommandTest] === 已暂停 ===");
                }
                else
                {
                    musicPlaybackService.Resume();
                    beatClockSystem.Resume();
                    Debug.Log("[CommandTest] === 已恢复 ===");
                }
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                RestartTest();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
            {
                PrintCommandStats();
            }
        }

        private void RestartTest()
        {
            musicPlaybackService.Stop();
            beatClockSystem.StopClock();
            inputRouter.ClearBuffer();
            commandResolver.Reset();

            if (debugPanel != null)
            {
                debugPanel.ResetStats();
            }

            StartTest();
            Debug.Log("[CommandTest] === 测试重新开始 ===");
        }

        private void OnBeatTick(BeatFrame frame)
        {
            if (beatPulseObject != null)
            {
                beatPulseObject.localScale = _originalScale * 1.3f;
            }
        }

        private void OnCommandResolved(CommandExecutionRequest request)
        {
            string perfect = request.isPerfectTiming ? "★" : "";
            string inputs = string.Join("+", request.triggerInputs);

            Debug.Log($"[Command] {perfect}{request.commandType.GetDisplayName()} @ Beat {request.sourceBeatIndex} ({inputs})");
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

        private void PrintCommandStats()
        {
            Debug.Log("[CommandTest] ═══════ 命令统计 ═══════");
            // 统计由 CommandDebugPanel 维护
            Debug.Log("[CommandTest] 查看屏幕右下角的统计信息");
        }

        private void OnDestroy()
        {
            if (beatClockSystem != null)
                beatClockSystem.OnNewBeat -= OnBeatTick;
            if (commandResolver != null)
                commandResolver.OnCommandResolved -= OnCommandResolved;
        }
    }
}