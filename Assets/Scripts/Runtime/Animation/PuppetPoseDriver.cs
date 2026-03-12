using UnityEngine;
using ShadowRhythm.Fighter;

namespace ShadowRhythm.Animation
{
    /// <summary>
    /// 皮影姿态驱动器 - 根据战斗状态驱动姿态切换
    /// </summary>
    public class PuppetPoseDriver : MonoBehaviour
    {
        [Header("依赖")]
        [SerializeField] private FighterRuntime fighterRuntime;
        [SerializeField] private PuppetRigController rigController;

        [Header("设置")]
        [SerializeField] private float idleReturnDelay = 0.1f;

        private float _idleTimer;
        private bool _waitingForIdle;

        private void Awake()
        {
            if (fighterRuntime == null)
                fighterRuntime = GetComponentInParent<FighterRuntime>();
            if (rigController == null)
                rigController = GetComponent<PuppetRigController>();
        }

        private void OnEnable()
        {
            if (fighterRuntime != null)
            {
                fighterRuntime.StateMachine.OnStateChanged += HandleFighterStateChanged;
            }
        }

        private void OnDisable()
        {
            if (fighterRuntime != null)
            {
                fighterRuntime.StateMachine.OnStateChanged -= HandleFighterStateChanged;
            }
        }

        private void Update()
        {
            if (_waitingForIdle)
            {
                _idleTimer -= Time.deltaTime;
                if (_idleTimer <= 0)
                {
                    _waitingForIdle = false;
                    rigController?.ReturnToIdle();
                }
            }
        }

        private void HandleFighterStateChanged(FighterState oldState, FighterState newState)
        {
            if (rigController == null) return;

            switch (newState)
            {
                case FighterState.Idle:
                    // 延迟返回 Idle，避免动画过于僵硬
                    _waitingForIdle = true;
                    _idleTimer = idleReturnDelay;
                    break;

                case FighterState.Hitstun:
                    rigController.PlayHitReaction();
                    break;

                case FighterState.Guard:
                    rigController.SetAnimationState(PuppetAnimationState.Guard);
                    break;

                case FighterState.Parry:
                    rigController.SetAnimationState(PuppetAnimationState.Parry);
                    break;

                case FighterState.Dash:
                    rigController.SetAnimationState(PuppetAnimationState.Flash);
                    break;

                // Startup/Active/Recovery 由 CombatAnimationBridge 处理
            }
        }

        /// <summary>
        /// 取消等待返回 Idle
        /// </summary>
        public void CancelIdleReturn()
        {
            _waitingForIdle = false;
        }
    }
}