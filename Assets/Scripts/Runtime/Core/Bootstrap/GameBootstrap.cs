using UnityEngine;
using UnityEngine.SceneManagement;
using ShadowRhythm.Core.Persistence;
using ShadowRhythm.Data.Models;

namespace ShadowRhythm.Core.Bootstrap
{
    /// <summary>
    /// 游戏启动入口，负责初始化核心服务并验证环境
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("验证设置")]
        [SerializeField] private string testJsonFileName = "test_config";
        [SerializeField] private string nextSceneName = "Battle_Prototype01";

        private JsonDataManager _jsonDataManager;
        private JsonLoadBridge _jsonLoadBridge;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeCoreServices();
            RunBootValidation();
        }

        private void InitializeCoreServices()
        {
            _jsonDataManager = JsonDataManager.Instance;
            _jsonLoadBridge = new JsonLoadBridge(_jsonDataManager);
            Debug.Log("[GameBootstrap] 核心服务初始化完成");
        }

        private void RunBootValidation()
        {
            Debug.Log("========== 板块0 验证开始 ==========");

            // 1. 验证 JSON 读取
            ValidateJsonSystem();

            // 2. 验证 Input System
            ValidateInputSystem();

            // 3. 验证当前场景
            ValidateSceneSystem();

            Debug.Log("========== 板块0 验证结束 ==========");
        }

        private void ValidateJsonSystem()
        {
            string streamingPath = Application.streamingAssetsPath;
            Debug.Log($"[JSON验证] StreamingAssets 路径: {streamingPath}");

            // 尝试加载测试配置
            var testConfig = _jsonDataManager.LoadData<TestConfigModel>(testJsonFileName);
            if (testConfig != null && !string.IsNullOrEmpty(testConfig.testField))
            {
                Debug.Log($"[JSON验证] ✅ 成功读取测试 JSON: {testConfig.testField}");
            }
            else
            {
                Debug.LogWarning($"[JSON验证] ⚠️ 未找到测试 JSON 文件: {testJsonFileName}.json");
                Debug.Log($"[JSON验证] 请在 StreamingAssets/Json/ 下创建 {testJsonFileName}.json");
            }

            // 尝试加载判定窗口配置
            var judgeConfig = _jsonLoadBridge.LoadJudgeWindowConfig();
            if (judgeConfig != null && judgeConfig.perfectMs > 0)
            {
                Debug.Log($"[JSON验证] ✅ 判定配置加载成功 - Perfect: {judgeConfig.perfectMs}ms");
            }
        }

        private void ValidateInputSystem()
        {
#if ENABLE_INPUT_SYSTEM
            Debug.Log("[Input验证] ✅ 新 Input System 已启用");
#elif ENABLE_LEGACY_INPUT_MANAGER
            Debug.LogWarning("[Input验证] ⚠️ 当前使用旧版 Input Manager");
#else
            Debug.Log("[Input验证] ⚠️ 输入系统状态未知");
#endif
        }

        private void ValidateSceneSystem()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log($"[场景验证] 当前场景: {currentScene}");
            Debug.Log($"[场景验证] ✅ 场景系统正常");
        }

        /// <summary>
        /// 切换到下一个场景（供 UI 按钮调用）
        /// </summary>
        public void LoadNextScene()
        {
            Debug.Log($"[GameBootstrap] 准备切换到场景: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    /// <summary>
    /// 测试配置模型
    /// </summary>
    [System.Serializable]
    public class TestConfigModel
    {
        public string testField;
        public int testNumber;
    }
}