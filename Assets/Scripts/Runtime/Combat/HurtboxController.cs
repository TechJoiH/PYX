using System;
using UnityEngine;

namespace ShadowRhythm.Combat
{
    /// <summary>
    /// Hurtbox 控制器 - 受击判定区域
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HurtboxController : MonoBehaviour
    {
        [Header("设置")]
        [SerializeField] private string ownerId;

        private Collider2D _collider;
        private bool _isActive = true;

        /// <summary>所属者 ID</summary>
        public string OwnerId => ownerId;

        /// <summary>是否激活</summary>
        public bool IsActive => _isActive;

        /// <summary>被命中事件</summary>
        public event Action<HitboxController> OnReceiveHit;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
        }

        /// <summary>
        /// 设置所属者
        /// </summary>
        public void SetOwner(string id)
        {
            ownerId = id;
        }

        /// <summary>
        /// 激活 Hurtbox
        /// </summary>
        public void Activate()
        {
            _isActive = true;
            _collider.enabled = true;
        }

        /// <summary>
        /// 关闭 Hurtbox（无敌状态）
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
            _collider.enabled = false;
        }

        /// <summary>
        /// 接收命中通知（由 CombatResolver 调用）
        /// </summary>
        public void NotifyHit(HitboxController hitbox)
        {
            if (_isActive)
            {
                OnReceiveHit?.Invoke(hitbox);
            }
        }
    }
}