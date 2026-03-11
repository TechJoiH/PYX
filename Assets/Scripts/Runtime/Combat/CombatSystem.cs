using System;
using System.Collections.Generic;
using UnityEngine;
using ShadowRhythm.Fighter;
using ShadowRhythm.Command;
using ShadowRhythm.Data.Models;
using ShadowRhythm.Rhythm;

namespace ShadowRhythm.Combat
{
    /// <summary>
    /// 战斗系统 - 管理命令执行、碰撞检测、伤害计算
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        [Header("依赖")]
        [SerializeField] private CommandResolver commandResolver;
        [SerializeField] private BeatClockSystem beatClockSystem;
        [SerializeField] private CombatResolver combatResolver;

        [Header("战斗者")]
        [SerializeField] private FighterRuntime playerFighter;
        [SerializeField] private FighterRuntime enemyFighter;

        [Header("调试")]
        [SerializeField] private bool enableDebugLog = true;

        /// <summary>招式定义缓存</summary>
        private readonly Dictionary<CommandType, MoveDefinitionModel> _moveDefinitions = new Dictionary<CommandType, MoveDefinitionModel>();

        /// <summary>待处理的攻击</summary>
        private readonly List<PendingAttack> _pendingAttacks = new List<PendingAttack>();

        /// <summary>战斗结果事件</summary>
        public event Action<CombatResult> OnCombatResult;

        private struct PendingAttack
        {
            public FighterRuntime attacker;
            public FighterRuntime target;
            public MoveDefinitionModel move;
            public int beatIndex;
            public bool isPerfect;
            public bool hasBeenProcessed; // 防止重复处理
        }

        private void Awake()
        {
            if (commandResolver == null)
                commandResolver = FindObjectOfType<CommandResolver>();
            if (beatClockSystem == null)
                beatClockSystem = FindObjectOfType<BeatClockSystem>();
            if (combatResolver == null)
                combatResolver = GetComponent<CombatResolver>() ?? gameObject.AddComponent<CombatResolver>();
        }

        private void OnEnable()
        {
            if (commandResolver != null)
                commandResolver.OnCommandResolved += HandleCommandResolved;
            if (beatClockSystem != null)
                beatClockSystem.OnNewBeat += HandleNewBeat;
            if (combatResolver != null)
                combatResolver.OnCombatResult += HandleCombatResult;
        }

        private void OnDisable()
        {
            if (commandResolver != null)
                commandResolver.OnCommandResolved -= HandleCommandResolved;
            if (beatClockSystem != null)
                beatClockSystem.OnNewBeat -= HandleNewBeat;
            if (combatResolver != null)
                combatResolver.OnCombatResult -= HandleCombatResult;
        }

        private void Update()
        {
            // 每帧处理待攻击列表
            ProcessPendingAttacks();
        }

        /// <summary>
        /// 初始化战斗系统
        /// </summary>
        public void Initialize(FighterRuntime player, FighterRuntime enemy)
        {
            playerFighter = player;
            enemyFighter = enemy;

            Debug.Log($"[CombatSystem] 战斗系统初始化完成 - Player={player?.FighterId}, Enemy={enemy?.FighterId}");
        }

        /// <summary>
        /// 注册招式定义
        /// </summary>
        public void RegisterMoveDefinition(MoveDefinitionModel move)
        {
            if (move == null) return;

            if (Enum.TryParse<CommandType>(move.commandType, true, out var cmdType))
            {
                _moveDefinitions[cmdType] = move;
                if (enableDebugLog)
                    Debug.Log($"[CombatSystem] 注册招式: {move.displayName} -> {cmdType}");
            }
        }

        /// <summary>
        /// 批量注册招式
        /// </summary>
        public void RegisterMoveDefinitions(IEnumerable<MoveDefinitionModel> moves)
        {
            foreach (var move in moves)
            {
                RegisterMoveDefinition(move);
            }
        }

        /// <summary>
        /// 获取招式定义
        /// </summary>
        public MoveDefinitionModel GetMoveDefinition(CommandType commandType)
        {
            _moveDefinitions.TryGetValue(commandType, out var move);
            return move;
        }

        private void HandleCommandResolved(CommandExecutionRequest request)
        {
            if (playerFighter == null) return;

            int currentBeat = request.sourceBeatIndex;

            switch (request.commandType)
            {
                case CommandType.Flash:
                    playerFighter.TryDash(currentBeat, 1);
                    break;

                case CommandType.ParryGuard:
                    playerFighter.TryParry(currentBeat, 1);
                    break;

                default:
                    // 玩家攻击敌人
                    ExecuteMove(playerFighter, enemyFighter, request);
                    break;
            }
        }

        private void ExecuteMove(FighterRuntime attacker, FighterRuntime target, CommandExecutionRequest request)
        {
            var move = GetMoveDefinition(request.commandType);

            if (move == null)
            {
                move = CreateDefaultMove(request.commandType);
            }

            if (attacker.ExecuteMove(move, request.sourceBeatIndex))
            {
                // 如果是攻击类招式且有目标，添加到待攻击列表
                if (IsAttackCommand(request.commandType) && target != null)
                {
                    _pendingAttacks.Add(new PendingAttack
                    {
                        attacker = attacker,
                        target = target,
                        move = move,
                        beatIndex = request.sourceBeatIndex,
                        isPerfect = request.isPerfectTiming,
                        hasBeenProcessed = false
                    });

                    if (enableDebugLog)
                        Debug.Log($"[CombatSystem] 攻击已排队: {move.displayName} ({attacker.FighterId} -> {target.FighterId}, Perfect={request.isPerfectTiming})");
                }
            }
        }

        private void ProcessPendingAttacks()
        {
            if (_pendingAttacks.Count == 0) return;

            for (int i = _pendingAttacks.Count - 1; i >= 0; i--)
            {
                var attack = _pendingAttacks[i];

                // 跳过已处理的攻击
                if (attack.hasBeenProcessed)
                {
                    _pendingAttacks.RemoveAt(i);
                    continue;
                }

                // 检查攻击者是否仍在 Active 状态
                if (attack.attacker.StateMachine.IsInActiveFrame())
                {
                    // 确保目标有效
                    if (attack.target == null || attack.target == attack.attacker)
                    {
                        Debug.LogWarning($"[CombatSystem] 无效的攻击目标！attacker={attack.attacker?.FighterId}, target={attack.target?.FighterId}");
                        _pendingAttacks.RemoveAt(i);
                        continue;
                    }

                    // 执行攻击判定
                    int damage = DamageResolver.CalculateDamage(attack.move, attack.attacker.Stats, attack.isPerfect);
                    var result = combatResolver.ResolveAttack(attack.attacker, attack.target, damage, attack.beatIndex);

                    if (enableDebugLog)
                        Debug.Log($"[CombatSystem] 攻击判定: {attack.move.displayName} ({attack.attacker.FighterId} -> {attack.target.FighterId}) -> {result.resultType}");

                    // 标记为已处理并移除
                    _pendingAttacks.RemoveAt(i);
                }
                // 如果攻击者已不在 Active 或 Startup 状态，移除攻击
                else if (attack.attacker.StateMachine.CurrentState != FighterState.Startup &&
                         attack.attacker.StateMachine.CurrentState != FighterState.Active)
                {
                    if (enableDebugLog)
                        Debug.Log($"[CombatSystem] 攻击已过期: {attack.move.displayName}");
                    _pendingAttacks.RemoveAt(i);
                }
            }
        }

        private void HandleNewBeat(BeatFrame frame)
        {
            // 更新所有战斗者状态
            playerFighter?.UpdateBeat(frame.beatIndex);
            enemyFighter?.UpdateBeat(frame.beatIndex);
        }

        private void HandleCombatResult(CombatResult result)
        {
            OnCombatResult?.Invoke(result);
        }

        private MoveDefinitionModel CreateDefaultMove(CommandType commandType)
        {
            return new MoveDefinitionModel
            {
                moveId = $"default_{commandType}",
                displayName = commandType.GetDisplayName(),
                commandType = commandType.ToString(),
                startupBeats = 0,
                activeBeats = 1,
                recoveryBeats = 1,
                damage = 10,
                canParry = false,
                canBeCancelled = false
            };
        }

        private bool IsAttackCommand(CommandType type)
        {
            return type == CommandType.Flick ||
                   type == CommandType.Shake ||
                   type == CommandType.Lift ||
                   type == CommandType.DashSlash ||
                   type == CommandType.RisingStrike ||
                   type == CommandType.ComboStab ||
                   type == CommandType.CounterSlash ||
                   type == CommandType.JumpSlash ||
                   type == CommandType.FlashStrike;
        }
    }
}