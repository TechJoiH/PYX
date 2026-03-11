using System;
using UnityEngine;
using ShadowRhythm.Fighter;

namespace ShadowRhythm.Combat
{
    /// <summary>
    /// 战斗结果类型
    /// </summary>
    public enum CombatResultType
    {
        None,
        Hit,           // 正常命中
        Blocked,       // 被格挡
        Parried,       // 被弹反
        Dodged,        // 被闪避
        Missed         // 落空
    }

    /// <summary>
    /// 战斗判定结果
    /// </summary>
    public struct CombatResult
    {
        public CombatResultType resultType;
        public FighterRuntime attacker;
        public FighterRuntime defender;
        public int damage;
        public int beatIndex;

        public bool IsHit => resultType == CombatResultType.Hit;
    }

    /// <summary>
    /// 战斗判定器 - 处理命中、格挡、弹反逻辑
    /// </summary>
    public class CombatResolver : MonoBehaviour
    {
        [Header("弹反设置")]
        [SerializeField] private int parryStunBeats = 2; // 弹反成功后对方硬直拍数

        /// <summary>战斗结果事件</summary>
        public event Action<CombatResult> OnCombatResult;

        /// <summary>
        /// 判定攻击结果
        /// </summary>
        public CombatResult ResolveAttack(FighterRuntime attacker, FighterRuntime defender, int damage, int currentBeat)
        {
            var result = new CombatResult
            {
                attacker = attacker,
                defender = defender,
                damage = damage,
                beatIndex = currentBeat,
                resultType = CombatResultType.None
            };

            if (attacker == null || defender == null)
            {
                result.resultType = CombatResultType.Missed;
                return result;
            }

            // 攻击者不在 Active 状态
            if (!attacker.StateMachine.IsInActiveFrame())
            {
                result.resultType = CombatResultType.Missed;
                Debug.Log($"[CombatResolver] 攻击者 {attacker.FighterId} 不在 Active 状态");
                return result;
            }

            // 检查防御者状态
            var defenderState = defender.StateMachine.CurrentState;

            // 弹反判定（最高优先）
            if (defenderState == FighterState.Parry)
            {
                result.resultType = CombatResultType.Parried;
                // 弹反成功，攻击者进入硬直
                attacker.StateMachine.ChangeState(FighterState.Hitstun, currentBeat, parryStunBeats);
                Debug.Log($"[CombatResolver] {defender.FighterId} 弹反成功！{attacker.FighterId} 进入硬直");
            }
            // 闪避判定
            else if (defenderState == FighterState.Dash)
            {
                result.resultType = CombatResultType.Dodged;
                Debug.Log($"[CombatResolver] {defender.FighterId} 闪避成功！");
            }
            // 格挡判定
            else if (defenderState == FighterState.Guard)
            {
                result.resultType = CombatResultType.Blocked;
                // 格挡会造成少量硬直
                defender.EnterGuardStun(currentBeat, 1);
                Debug.Log($"[CombatResolver] {defender.FighterId} 格挡成功！");
            }
            // 不可被攻击
            else if (!defender.StateMachine.IsVulnerable())
            {
                result.resultType = CombatResultType.Missed;
                Debug.Log($"[CombatResolver] {defender.FighterId} 处于无敌状态");
            }
            // 正常命中
            else
            {
                result.resultType = CombatResultType.Hit;
                result.damage = damage;
                defender.TakeDamage(damage, currentBeat);
                Debug.Log($"[CombatResolver] {attacker.FighterId} 命中 {defender.FighterId}，造成 {damage} 伤害");
            }

            OnCombatResult?.Invoke(result);
            return result;
        }
    }
}