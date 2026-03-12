using System;
using UnityEngine;
using ShadowRhythm.Fighter;
using ShadowRhythm.Command;

namespace ShadowRhythm.Animation
{
    /// <summary>
    /// 皮影骨骼控制器 - 管理角色 Animator 状态切换
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PuppetRigController : MonoBehaviour
    {
        [Header("动画参数名")]
        [SerializeField] private string stateParamName = "AnimState";
        [SerializeField] private string triggerParamName = "Trigger";
        [SerializeField] private string speedParamName = "Speed";

        [Header("设置")]
        [SerializeField] private float transitionDuration = 0.05f;
        [SerializeField] private bool useSnappyTransitions = true;

        private Animator _animator;
        private int _stateHash;
        private int _triggerHash;
        private int _speedHash;

        private PuppetAnimationState _currentState = PuppetAnimationState.Idle;

        /// <summary>当前动画状态</summary>
        public PuppetAnimationState CurrentState => _currentState;

        /// <summary>动画状态变化事件</summary>
        public event Action<PuppetAnimationState, PuppetAnimationState> OnAnimationStateChanged;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            CacheParameterHashes();
        }

        private void CacheParameterHashes()
        {
            _stateHash = Animator.StringToHash(stateParamName);
            _triggerHash = Animator.StringToHash(triggerParamName);
            _speedHash = Animator.StringToHash(speedParamName);
        }

        /// <summary>
        /// 设置动画状态（整数参数方式）
        /// </summary>
        public void SetAnimationState(PuppetAnimationState newState)
        {
            if (_currentState == newState) return;

            var prevState = _currentState;
            _currentState = newState;

            _animator.SetInteger(_stateHash, (int)newState);

            if (useSnappyTransitions)
            {
                _animator.SetTrigger(_triggerHash);
            }

            OnAnimationStateChanged?.Invoke(prevState, newState);
            Debug.Log($"[PuppetRigController] 动画切换: {prevState} -> {newState}");
        }

        /// <summary>
        /// 根据命令类型设置动画
        /// </summary>
        public void PlayCommandAnimation(CommandType commandType)
        {
            var animState = CommandToAnimationState(commandType);
            SetAnimationState(animState);
        }

        /// <summary>
        /// 播放受击动画
        /// </summary>
        public void PlayHitReaction()
        {
            SetAnimationState(PuppetAnimationState.HitReaction);
        }

        /// <summary>
        /// 返回待机状态
        /// </summary>
        public void ReturnToIdle()
        {
            SetAnimationState(PuppetAnimationState.Idle);
        }

        /// <summary>
        /// 设置动画速度（用于节拍同步）
        /// </summary>
        public void SetAnimationSpeed(float speed)
        {
            _animator.SetFloat(_speedHash, speed);
        }

        /// <summary>
        /// 命令类型到动画状态的映射
        /// </summary>
        private PuppetAnimationState CommandToAnimationState(CommandType commandType)
        {
            switch (commandType)
            {
                case CommandType.Lift:
                    return PuppetAnimationState.Lift;
                case CommandType.Flick:
                    return PuppetAnimationState.Flick;
                case CommandType.Shake:
                    return PuppetAnimationState.Shake;
                case CommandType.Flash:
                    return PuppetAnimationState.Flash;
                case CommandType.DashSlash:
                    return PuppetAnimationState.DashSlash;
                case CommandType.RisingStrike:
                    return PuppetAnimationState.RisingStrike;
                case CommandType.ParryGuard:
                    return PuppetAnimationState.Parry;
                default:
                    return PuppetAnimationState.Idle;
            }
        }

        /// <summary>
        /// 获取 Animator 组件（供外部访问）
        /// </summary>
        public Animator GetAnimator() => _animator;
    }
}