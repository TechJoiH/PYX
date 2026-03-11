using System;

namespace ShadowRhythm.Fighter
{
    /// <summary>
    /// 角色属性运行时 - 管理角色的血量、伤害等属性
    /// </summary>
    [Serializable]
    public class FighterStatsRuntime
    {
        /// <summary>最大生命值</summary>
        public int maxHealth;

        /// <summary>当前生命值</summary>
        public int currentHealth;

        /// <summary>基础攻击力</summary>
        public int baseAttack;

        /// <summary>基础防御力</summary>
        public int baseDefense;

        /// <summary>是否存活</summary>
        public bool IsAlive => currentHealth > 0;

        /// <summary>生命百分比 (0~1)</summary>
        public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

        public FighterStatsRuntime(int maxHp = 100, int attack = 10, int defense = 5)
        {
            maxHealth = maxHp;
            currentHealth = maxHp;
            baseAttack = attack;
            baseDefense = defense;
        }

        /// <summary>
        /// 造成伤害
        /// </summary>
        public int TakeDamage(int rawDamage)
        {
            int finalDamage = Math.Max(1, rawDamage - baseDefense);
            currentHealth = Math.Max(0, currentHealth - finalDamage);
            return finalDamage;
        }

        /// <summary>
        /// 恢复生命
        /// </summary>
        public void Heal(int amount)
        {
            currentHealth = Math.Min(maxHealth, currentHealth + amount);
        }

        /// <summary>
        /// 重置为满血
        /// </summary>
        public void Reset()
        {
            currentHealth = maxHealth;
        }
    }
}