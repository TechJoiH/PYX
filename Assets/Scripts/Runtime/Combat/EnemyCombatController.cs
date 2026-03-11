using UnityEngine;
using ShadowRhythm.Fighter;

namespace ShadowRhythm.Combat
{
    /// <summary>
    /// 敌人战斗控制器 - 管理敌人的 Hitbox/Hurtbox
    /// </summary>
    public class EnemyCombatController : MonoBehaviour
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

            string ownerId = fighterRuntime != null ? fighterRuntime.FighterId : "enemy";

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
            if (newState == FighterState.Active)
            {
                hitbox?.Activate();
            }
            else
            {
                hitbox?.Deactivate();
            }

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