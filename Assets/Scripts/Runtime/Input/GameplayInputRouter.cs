using System;
using UnityEngine;
using UnityEngine.InputSystem;
using ShadowRhythm.Rhythm;

namespace ShadowRhythm.Input
{
    /// <summary>
    /// 游戏输入路由 - 接收 Input System 输入并转换为带节奏信息的 InputSample
    /// </summary>
    public sealed class GameplayInputRouter : MonoBehaviour
    {
        [Header("依赖")]
        [SerializeField] private BeatClockSystem beatClockSystem;

        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActionAsset;

        [Header("调试")]
        [SerializeField] private bool enableDebugLog = true;

        /// <summary>输入缓冲区</summary>
        public InputBuffer InputBuffer { get; private set; }

        /// <summary>输入判定器</summary>
        public InputWindowEvaluator Evaluator { get; private set; }

        /// <summary>是否启用输入</summary>
        public bool IsEnabled { get; private set; } = true;

        // Input Actions
        private InputAction _liftAction;
        private InputAction _flickAction;
        private InputAction _shakeAction;
        private InputAction _flashAction;

        // Action Map
        private InputActionMap _gameplayMap;

        /// <summary>输入事件（带完整判定信息）</summary>
        public event Action<InputSample> OnInputReceived;

        private void Awake()
        {
            InputBuffer = new InputBuffer(8);
            Evaluator = new InputWindowEvaluator();

            // 自动查找 BeatClockSystem
            if (beatClockSystem == null)
            {
                beatClockSystem = FindObjectOfType<BeatClockSystem>();
            }

            SetupInputActions();
        }

        private void OnEnable()
        {
            EnableInput();
        }

        private void OnDisable()
        {
            DisableInput();
        }

        private void OnDestroy()
        {
            CleanupInputActions();
        }

        /// <summary>
        /// 初始化（外部调用）
        /// </summary>
        public void Initialize(BeatClockSystem clockSystem, InputWindowEvaluator evaluator = null)
        {
            beatClockSystem = clockSystem;

            if (evaluator != null)
            {
                Evaluator = evaluator;
            }

            Debug.Log("[GameplayInputRouter] 初始化完成");
        }

        /// <summary>
        /// 启用输入
        /// </summary>
        public void EnableInput()
        {
            IsEnabled = true;
            _gameplayMap?.Enable();
        }

        /// <summary>
        /// 禁用输入
        /// </summary>
        public void DisableInput()
        {
            IsEnabled = false;
            _gameplayMap?.Disable();
        }

        /// <summary>
        /// 清空输入缓冲
        /// </summary>
        public void ClearBuffer()
        {
            InputBuffer.Clear();
        }

        private void SetupInputActions()
        {
            if (inputActionAsset != null)
            {
                // 从资产获取 Action Map
                _gameplayMap = inputActionAsset.FindActionMap("Gameplay");

                if (_gameplayMap != null)
                {
                    _liftAction = _gameplayMap.FindAction("Lift");
                    _flickAction = _gameplayMap.FindAction("Flick");
                    _shakeAction = _gameplayMap.FindAction("Shake");
                    _flashAction = _gameplayMap.FindAction("Flash");

                    // 绑定回调
                    if (_liftAction != null) _liftAction.performed += OnLiftPerformed;
                    if (_flickAction != null) _flickAction.performed += OnFlickPerformed;
                    if (_shakeAction != null) _shakeAction.performed += OnShakePerformed;
                    if (_flashAction != null) _flashAction.performed += OnFlashPerformed;

                    Debug.Log("[GameplayInputRouter] Input Actions 已绑定");
                }
                else
                {
                    Debug.LogWarning("[GameplayInputRouter] 未找到 Gameplay Action Map，使用键盘回退");
                    SetupKeyboardFallback();
                }
            }
            else
            {
                Debug.LogWarning("[GameplayInputRouter] 未指定 InputActionAsset，使用键盘回退");
                SetupKeyboardFallback();
            }
        }

        private void SetupKeyboardFallback()
        {
            // 创建简单的键盘绑定作为回退
            var map = new InputActionMap("GameplayFallback");

            _liftAction = map.AddAction("Lift", binding: "<Keyboard>/w");
            _flickAction = map.AddAction("Flick", binding: "<Keyboard>/j");
            _shakeAction = map.AddAction("Shake", binding: "<Keyboard>/k");
            _flashAction = map.AddAction("Flash", binding: "<Keyboard>/l");

            // 添加额外绑定
            _liftAction.AddBinding("<Keyboard>/upArrow");

            _liftAction.performed += OnLiftPerformed;
            _flickAction.performed += OnFlickPerformed;
            _shakeAction.performed += OnShakePerformed;
            _flashAction.performed += OnFlashPerformed;

            _gameplayMap = map;
            map.Enable();

            Debug.Log("[GameplayInputRouter] 使用键盘回退: W/↑=Lift, J=Flick, K=Shake, L=Flash");
        }

        private void CleanupInputActions()
        {
            if (_liftAction != null) _liftAction.performed -= OnLiftPerformed;
            if (_flickAction != null) _flickAction.performed -= OnFlickPerformed;
            if (_shakeAction != null) _shakeAction.performed -= OnShakePerformed;
            if (_flashAction != null) _flashAction.performed -= OnFlashPerformed;

            _gameplayMap?.Disable();
        }

        // ========== 输入回调 ==========

        private void OnLiftPerformed(InputAction.CallbackContext ctx)
        {
            RegisterInput(RhythmInputType.Lift);
        }

        private void OnFlickPerformed(InputAction.CallbackContext ctx)
        {
            RegisterInput(RhythmInputType.Flick);
        }

        private void OnShakePerformed(InputAction.CallbackContext ctx)
        {
            RegisterInput(RhythmInputType.Shake);
        }

        private void OnFlashPerformed(InputAction.CallbackContext ctx)
        {
            RegisterInput(RhythmInputType.Flash);
        }

        /// <summary>
        /// 手动注册输入（用于测试或触摸输入）
        /// </summary>
        public void ManualInput(RhythmInputType inputType)
        {
            RegisterInput(inputType);
        }

        private void RegisterInput(RhythmInputType inputType)
        {
            if (!IsEnabled) return;

            // 获取当前歌曲时间
            float songTime = beatClockSystem != null ? beatClockSystem.CurrentSongTime : 0f;

            // 获取节拍信息
            BeatFrame frame = beatClockSystem != null
                ? beatClockSystem.GetCurrentBeatFrame()
                : default;

            // 计算量化拍点和偏差
            int quantizedBeat = frame.beatIndex;
            float deltaMs = frame.deltaMs;

            // 判定时机
            RhythmJudgeResult result = Evaluator.Evaluate(deltaMs);

            // 创建输入采样
            var sample = new InputSample(
                inputType,
                songTime,
                quantizedBeat,
                deltaMs,
                result,
                Time.frameCount
            );

            // 添加到缓冲区
            InputBuffer.Push(sample);

            // 清理旧输入
            InputBuffer.TrimOldSamples(quantizedBeat);

            // 触发事件
            OnInputReceived?.Invoke(sample);

            if (enableDebugLog)
            {
                string timing = deltaMs >= 0 ? $"+{deltaMs:F1}ms" : $"{deltaMs:F1}ms";
                Debug.Log($"[Input] {inputType} @ Beat {quantizedBeat} | {timing} | {result}");
            }
        }

        /// <summary>
        /// 获取输入类型的显示名称
        /// </summary>
        public static string GetInputTypeName(RhythmInputType type)
        {
            return type switch
            {
                RhythmInputType.Lift => "提",
                RhythmInputType.Flick => "拨",
                RhythmInputType.Shake => "抖",
                RhythmInputType.Flash => "闪",
                _ => "?"
            };
        }
    }
}