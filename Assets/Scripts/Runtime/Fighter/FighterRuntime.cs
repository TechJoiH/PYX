using System;
using UnityEngine;
using ShadowRhythm.Data.Models;

namespace ShadowRhythm.Fighter
{
    /// <summary>
    /// 角色运行时 - 整合状态机、属性、当前招式等
    /// </summary>
    public class FighterRuntime : MonoBehaviour
    {
        [Header("身份")]
        [SerializeField] private string fighterId = "player";
        [SerializeField] private bool isPlayer = true;

        [Header("属性")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int baseAttack = 10;
        [SerializeField] private int baseDefense = 5;

        /// <summary>角色 ID</summary>
        public string FighterId => fighterId;

        /// <summary>是否是玩家</summary>
        public bool IsPlayer => isPlayer;

        /// <summary>状态机</summary>
        public FighterStateMachine StateMachine { get; private set; }

        /// <summary>属性</summary>
        public FighterStatsRuntime Stats { get; private set; }

        /// <summary>当前执行的招式</summary>
        public MoveDefinitionModel CurrentMove { get; private set; }

        /// <summary>当前招式开始拍点</summary>
        public int MoveStartBeat { get; private set; }

        /// <summary>是否存活</summary>
        public bool IsAlive => Stats.IsAlive;

        /// <summary>受伤事件</summary>
        public event Action<int, int> OnDamaged; // (damage, remainingHp)

        /// <summary>死亡事件</summary>
        public event Action OnDeath;

        /// <summary>招式开始事件</summary>
        public event Action<MoveDefinitionModel> OnMoveStarted;

        /// <summary>招式结束事件</summary>
        public event Action<MoveDefinitionModel> OnMoveEnded;

        private void Awake()
        {
            StateMachine = new FighterStateMachine();
            Stats = new FighterStatsRuntime(maxHealth, baseAttack, baseDefense);
        }

        /// <summary>
        /// 每拍更新
        /// </summary>
        public void UpdateBeat(int currentBeat)
        {
            // 更新状态机
            var prevState = StateMachine.CurrentState;
            StateMachine.UpdateBeat(currentBeat);

            // 检查招式阶段转换
            if (CurrentMove != null)
            {
                UpdateMovePhase(currentBeat);
            }
        }

        /// <summary>
        /// 执行招式
        /// </summary>
        public bool ExecuteMove(MoveDefinitionModel move, int currentBeat)
        {
            if (move == null) return false;

            // 检查是否可以出招
            if (!StateMachine.CanAct())
            {
                Debug.Log($"[FighterRuntime] {fighterId} 当前状态 {StateMachine.CurrentState} 不能出招");
                return false;
            }

            CurrentMove = move;
            MoveStartBeat = currentBeat;

            // 进入 Startup 阶段（如果有）
            if (move.startupBeats > 0)
            {
                StateMachine.ChangeState(FighterState.Startup, currentBeat, move.startupBeats);
            }
            else
            {
                // 无 startup，直接进入 Active
                StateMachine.ChangeState(FighterState.Active, currentBeat, move.activeBeats);
            }

            OnMoveStarted?.Invoke(move);
            Debug.Log($"[FighterRuntime] {fighterId} 执行招式: {move.displayName}");

            return true;
        }

        /// <summary>
        /// 尝试闪避
        /// </summary>
        public bool TryDash(int currentBeat, int durationBeats = 1)
        {
            if (!StateMachine.CanAct()) return false;

            StateMachine.ChangeState(FighterState.Dash, currentBeat, durationBeats);
            Debug.Log($"[FighterRuntime] {fighterId} 闪避");
            return true;
        }

        /// <summary>
        /// 尝试弹反
        /// </summary>
        public bool TryParry(int currentBeat, int windowBeats = 1)
        {
            if (!StateMachine.CanAct()) return false;

            StateMachine.ChangeState(FighterState.Parry, currentBeat, windowBeats);
            Debug.Log($"[FighterRuntime] {fighterId} 弹反姿态");
            return true;
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(int damage, int currentBeat)
        {
            if (!IsAlive) return;

            int finalDamage = Stats.TakeDamage(damage);
            OnDamaged?.Invoke(finalDamage, Stats.currentHealth);

            Debug.Log($"[FighterRuntime] {fighterId} 受到 {finalDamage} 伤害, 剩余 {Stats.currentHealth} HP");

            if (!Stats.IsAlive)
            {
                StateMachine.ChangeState(FighterState.Dead, currentBeat);
                OnDeath?.Invoke();
                Debug.Log($"[FighterRuntime] {fighterId} 死亡！");
            }
            else
            {
                // 进入受击硬直
                StateMachine.ChangeState(FighterState.Hitstun, currentBeat, 1);
                ClearCurrentMove();
            }
        }

        /// <summary>
        /// 进入格挡硬直（弹反失败或普通格挡）
        /// </summary>
        public void EnterGuardStun(int currentBeat, int durationBeats = 1)
        {
            StateMachine.ChangeState(FighterState.Guard, currentBeat, durationBeats);
        }

        /// <summary>
        /// 重置角色
        /// </summary>
        public void Reset()
        {
            Stats.Reset();
            StateMachine.ForceIdle(0);
            ClearCurrentMove();
        }

        private void UpdateMovePhase(int currentBeat)
        {
            if (CurrentMove == null) return;

            int elapsed = currentBeat - MoveStartBeat;
            int startupEnd = CurrentMove.startupBeats;
            int activeEnd = startupEnd + CurrentMove.activeBeats;
            int recoveryEnd = activeEnd + CurrentMove.recoveryBeats;

            var currentState = StateMachine.CurrentState;

            // Startup -> Active
            if (currentState == FighterState.Startup && elapsed >= startupEnd)
            {
                StateMachine.ChangeState(FighterState.Active, currentBeat, CurrentMove.activeBeats);
            }
            // Active -> Recovery
            else if (currentState == FighterState.Active && elapsed >= activeEnd)
            {
                StateMachine.ChangeState(FighterState.Recovery, currentBeat, CurrentMove.recoveryBeats);
            }
            // Recovery -> Idle (招式结束)
            else if (currentState == FighterState.Recovery && elapsed >= recoveryEnd)
            {
                OnMoveEnded?.Invoke(CurrentMove);
                ClearCurrentMove();
                StateMachine.ChangeState(FighterState.Idle, currentBeat);
            }
        }

        private void ClearCurrentMove()
        {
            CurrentMove = null;
            MoveStartBeat = -1;
        }
    }
}