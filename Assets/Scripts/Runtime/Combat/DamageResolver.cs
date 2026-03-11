using UnityEngine;
using ShadowRhythm.Fighter;
using ShadowRhythm.Data.Models;

namespace ShadowRhythm.Combat
{
    /// <summary>
    /// 伤害计算器 - 计算最终伤害
    /// </summary>
    public static class DamageResolver
    {
        /// <summary>Perfect 时间的伤害加成</summary>
        public const float PerfectDamageMultiplier = 1.5f;

        /// <summary>Good 时间的伤害加成</summary>
        public const float GoodDamageMultiplier = 1.0f;

        /// <summary>
        /// 计算最终伤害
        /// </summary>
        public static int CalculateDamage(
            MoveDefinitionModel move,
            FighterStatsRuntime attackerStats,
            bool isPerfectTiming)
        {
            if (move == null) return 0;

            float baseDamage = move.damage + attackerStats.baseAttack;
            float multiplier = isPerfectTiming ? PerfectDamageMultiplier : GoodDamageMultiplier;

            int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);
            return Mathf.Max(1, finalDamage);
        }

        /// <summary>
        /// 计算简化伤害（无招式定义时使用）
        /// </summary>
        public static int CalculateSimpleDamage(int baseAttack, bool isPerfectTiming)
        {
            float multiplier = isPerfectTiming ? PerfectDamageMultiplier : GoodDamageMultiplier;
            return Mathf.RoundToInt(baseAttack * multiplier);
        }
    }
}