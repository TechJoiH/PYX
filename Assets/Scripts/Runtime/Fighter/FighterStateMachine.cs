using System;
using UnityEngine;

namespace ShadowRhythm.Fighter
{
    /// <summary>
    /// 角色状态机 - 管理状态转换和时间
    /// </summary>
    public class FighterStateMachine
    {
        /// <summary>当前状态</summary>
        public FighterState CurrentState { get; private set; } = FighterState.Idle;

        /// <summary>上一个状态</summary>
        public FighterState PreviousState { get; private set; } = FighterState.Idle;

        /// <summary>状态开始拍点</summary>
        public int StateStartBeat { get; private set; }

        /// <summary>状态持续拍数</summary>
        public int StateDurationBeats { get; private set; }

        /// <summary>状态结束拍点</summary>
        public int StateEndBeat => StateStartBeat + StateDurationBeats;

        /// <summary>状态变化事件</summary>
        public event Action<FighterState, FighterState> OnStateChanged;

        /// <summary>
        /// 切换状态
        /// </summary>
        public void ChangeState(FighterState newState, int currentBeat, int durationBeats = 0)
        {
            if (CurrentState == newState) return;

            PreviousState = CurrentState;
            CurrentState = newState;
            StateStartBeat = currentBeat;
            StateDurationBeats = durationBeats;

            OnStateChanged?.Invoke(PreviousState, newState);
        }

        /// <summary>
        /// 检查状态是否过期并自动返回 Idle
        /// </summary>
        public void UpdateBeat(int currentBeat)
        {
            // 只有有时限的状态才自动结束
            if (StateDurationBeats > 0 && currentBeat >= StateEndBeat)
            {
                if (CurrentState != FighterState.Dead && CurrentState != FighterState.Idle)
                {
                    ChangeState(FighterState.Idle, currentBeat);
                }
            }
        }

        /// <summary>
        /// 检查当前状态是否允许出招
        /// </summary>
        public bool CanAct()
        {
            return CurrentState == FighterState.Idle ||
                   CurrentState == FighterState.Recovery; // Recovery 可以取消（可选）
        }

        /// <summary>
        /// 检查当前状态是否可被攻击
        /// </summary>
        public bool IsVulnerable()
        {
            return CurrentState != FighterState.Dash &&
                   CurrentState != FighterState.Parry &&
                   CurrentState != FighterState.Dead;
        }

        /// <summary>
        /// 检查是否处于格挡/弹反状态
        /// </summary>
        public bool IsGuarding()
        {
            return CurrentState == FighterState.Guard || CurrentState == FighterState.Parry;
        }

        /// <summary>
        /// 检查是否在 Active（攻击生效）状态
        /// </summary>
        public bool IsInActiveFrame()
        {
            return CurrentState == FighterState.Active;
        }

        /// <summary>
        /// 强制重置到 Idle
        /// </summary>
        public void ForceIdle(int currentBeat)
        {
            ChangeState(FighterState.Idle, currentBeat);
        }
    }
}