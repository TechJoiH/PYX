using UnityEngine;
using ShadowRhythm.Fighter;
using ShadowRhythm.Command;
using ShadowRhythm.Data.Models;

namespace ShadowRhythm.Animation
{
    /// <summary>
    /// 战斗动画桥接器 - 连接命令系统与动画系统
    /// </summary>
    public class CombatAnimationBridge : MonoBehaviour
    {
        [Header("依赖")]
        [SerializeField] private FighterRuntime fighterRuntime;
        [SerializeField] private PuppetRigController rigController;
        [SerializeField] private PuppetPoseDriver poseDriver;

        [Header("设置")]
        [SerializeField] private bool syncAnimationToBeat = true;

        private void Awake()
        {
            if (fighterRuntime == null)
                fighterRuntime = GetComponentInParent<FighterRuntime>();
            if (rigController == null)
                rigController = GetComponentInChildren<PuppetRigController>();
            if (poseDriver == null)
                poseDriver = GetComponentInChildren<PuppetPoseDriver>();
        }

        private void OnEnable()
        {
            if (fighterRuntime != null)
            {
                fighterRuntime.OnMoveStarted += HandleMoveStarted;
                fighterRuntime.OnMoveEnded += HandleMoveEnded;
                fighterRuntime.OnDamaged += HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (fighterRuntime != null)
            {
                fighterRuntime.OnMoveStarted -= HandleMoveStarted;
                fighterRuntime.OnMoveEnded -= HandleMoveEnded;
                fighterRuntime.OnDamaged -= HandleDamaged;
            }
        }

        private void HandleMoveStarted(MoveDefinitionModel move)
        {
            if (rigController == null || move == null) return;

            // 取消待机等待
            poseDriver?.CancelIdleReturn();

            // 解析命令类型并播放对应动画
            if (System.Enum.TryParse<CommandType>(move.commandType, out var commandType))
            {
                rigController.PlayCommandAnimation(commandType);
                Debug.Log($"[CombatAnimationBridge] 播放招式动画: {move.displayName} -> {commandType}");
            }
        }

        private void HandleMoveEnded(MoveDefinitionModel move)
        {
            // 招式结束后，由 PoseDriver 处理返回 Idle
        }

        private void HandleDamaged(int damage, int remainingHp)
        {
            // 受击时播放受击动画
            rigController?.PlayHitReaction();
        }

        /// <summary>
        /// 手动触发命令动画（用于测试）
        /// </summary>
        public void TriggerCommandAnimation(CommandType commandType)
        {
            poseDriver?.CancelIdleReturn();
            rigController?.PlayCommandAnimation(commandType);
        }
    }
}