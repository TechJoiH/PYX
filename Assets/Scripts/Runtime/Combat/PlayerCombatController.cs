using UnityEngine;
using ShadowRhythm.Fighter;

namespace ShadowRhythm.Combat
{
    /// <summary>
    /// 玩家战斗控制器 - 管理玩家的 Hitbox/Hurtbox
    /// </summary>
    public class PlayerCombatController : MonoBehaviour
    {
        [Header("组件")]
        [SerializeField] private FighterRuntime fighterRuntime;
        [SerializeField] private HitboxController hitbox;
        [SerializeField] private HurtboxController hurtbox;

        public FighterRuntime FighterRuntime => fighterRuntime;
        public HitboxController Hitbox => hitbox;
        public HurtboxController Hurtbox => hurtbox;

        private void Awake()
        {
            if (fighterRuntime == null)
                fighterRuntime = GetComponent<FighterRuntime>();

            // 设置所有者 ID
            string ownerId = fighterRuntime != null ? fighterRuntime.FighterId : "player";

            if (hitbox != null)
                hitbox.SetOwner(ownerId);
            if (hurtbox != null)
                hurtbox.SetOwner(ownerId);
        }

        private void OnEnable()
        {
            if (fighterRuntime != null)
            {
                fighterRuntime.StateMachine.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (fighterRuntime != null)
            {
                fighterRuntime.StateMachine.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(FighterState oldState, FighterState newState)
        {
            // Active 状态时激活 Hitbox
            if (newState == FighterState.Active)
            {
                hitbox?.Activate();
                var move = fighterRuntime.CurrentMove;
                if (move != null)
                {
                    hitbox?.SetDamage(move.damage);
                }
            }
            else
            {
                hitbox?.Deactivate();
            }

            // Dash 状态时关闭 Hurtbox（无敌）
            if (newState == FighterState.Dash)
            {
                hurtbox?.Deactivate();
            }
            else if (oldState == FighterState.Dash)
            {
                hurtbox?.Activate();
            }
        }
    }
}