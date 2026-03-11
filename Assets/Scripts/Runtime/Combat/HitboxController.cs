using System;
using UnityEngine;

namespace ShadowRhythm.Combat
{
    /// <summary>
    /// Hitbox 控制器 - 攻击判定区域
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HitboxController : MonoBehaviour
    {
        [Header("设置")]
        [SerializeField] private string ownerId;
        [SerializeField] private int damage = 10;

        private Collider2D _collider;
        private bool _isActive;

        /// <summary>所属者 ID</summary>
        public string OwnerId => ownerId;

        /// <summary>伤害值</summary>
        public int Damage => damage;

        /// <summary>是否激活</summary>
        public bool IsActive => _isActive;

        /// <summary>命中事件</summary>
        public event Action<HurtboxController> OnHit;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
            Deactivate();
        }

        /// <summary>
        /// 设置所属者
        /// </summary>
        public void SetOwner(string id)
        {
            ownerId = id;
        }

        /// <summary>
        /// 设置伤害
        /// </summary>
        public void SetDamage(int dmg)
        {
            damage = dmg;
        }

        /// <summary>
        /// 激活 Hitbox
        /// </summary>
        public void Activate()
        {
            _isActive = true;
            _collider.enabled = true;
        }

        /// <summary>
        /// 关闭 Hitbox
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
            _collider.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isActive) return;

            var hurtbox = other.GetComponent<HurtboxController>();
            if (hurtbox != null && hurtbox.OwnerId != ownerId)
            {
                OnHit?.Invoke(hurtbox);
            }
        }
    }
}