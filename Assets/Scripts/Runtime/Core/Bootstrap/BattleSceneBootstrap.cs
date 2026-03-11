using UnityEngine;
using ShadowRhythm.Core.Persistence;
using ShadowRhythm.Data.Models;
using ShadowRhythm.Rhythm;

namespace ShadowRhythm.Core.Bootstrap
{
    /// <summary>
    /// 战斗场景启动器，负责初始化战斗所需的所有子系统
    /// </summary>
    public sealed class BattleSceneBootstrap : MonoBehaviour
    {
        [Header("歌曲配置")]
        [SerializeField] private string songId = "001";

        [Header("系统引用（可选，自动查找）")]
        [SerializeField] private AudioSource musicSource;

        private JsonDataManager _jsonDataManager;
        private JsonLoadBridge _jsonLoadBridge;
        private SongRuntime _songRuntime;
        private JudgeWindowConfigModel _judgeConfig;

        public SongRuntime SongRuntime => _songRuntime;
        public JudgeWindowConfigModel JudgeConfig => _judgeConfig;

        private void Awake()
        {
            InitializeDataServices();
        }

        private void Start()
        {
            LoadBattleData();
            LogBattleInfo();
        }

        private void InitializeDataServices()
        {
            _jsonDataManager = JsonDataManager.Instance;
            _jsonLoadBridge = new JsonLoadBridge(_jsonDataManager);

            if (musicSource == null)
            {
                musicSource = GetComponent<AudioSource>();
            }
        }

        private void LoadBattleData()
        {
            // 加载歌曲元数据
            var songMeta = _jsonLoadBridge.LoadSongMeta(songId);
            if (songMeta != null)
            {
                _songRuntime = new SongRuntime(songMeta);
                Debug.Log($"[BattleBootstrap] ✅ 歌曲加载成功: {songMeta.displayName}");
            }
            else
            {
                Debug.LogWarning($"[BattleBootstrap] ⚠️ 未找到歌曲: {songId}");
            }

            // 加载判定配置
            _judgeConfig = _jsonLoadBridge.LoadJudgeWindowConfig();
            if (_judgeConfig != null)
            {
                Debug.Log($"[BattleBootstrap] ✅ 判定配置加载成功");
            }
        }

        private void LogBattleInfo()
        {
            Debug.Log("========== 战斗场景初始化完成 ==========");

            if (_songRuntime != null)
            {
                Debug.Log($"歌曲ID: {_songRuntime.SongId}");
                Debug.Log($"BPM: {_songRuntime.Bpm}");
                Debug.Log($"每拍秒数: {_songRuntime.SecondsPerBeat:F4}s");
                Debug.Log($"偏移: {_songRuntime.OffsetSeconds}s");
            }

            if (_judgeConfig != null)
            {
                Debug.Log($"Perfect 窗口: {_judgeConfig.perfectMs}ms");
                Debug.Log($"Good 窗口: {_judgeConfig.goodMs}ms");
                Debug.Log($"Miss 窗口: {_judgeConfig.missMs}ms");
            }

            Debug.Log("==========================================");
        }

        /// <summary>
        /// 供外部调用开始战斗
        /// </summary>
        public void StartBattle()
        {
            if (_songRuntime == null)
            {
                Debug.LogError("[BattleBootstrap] 无法开始战斗：歌曲未加载");
                return;
            }

            Debug.Log("[BattleBootstrap] 战斗开始！");
            // TODO: 启动 BeatClockSystem
            // TODO: 启动 GameplayInputRouter
        }
    }
}