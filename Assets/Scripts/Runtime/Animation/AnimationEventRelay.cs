using System;
using UnityEngine;

namespace ShadowRhythm.Animation
{
    /// <summary>
    /// 动画事件中继器 - 将动画事件转发给游戏逻辑
    /// </summary>
    public class AnimationEventRelay : MonoBehaviour
    {
        /// <summary>Hitbox 激活事件</summary>
        public event Action OnHitboxActivate;

        /// <summary>Hitbox 关闭事件</summary>
        public event Action OnHitboxDeactivate;

        /// <summary>播放音效事件</summary>
        public event Action<string> OnPlaySound;

        /// <summary>播放特效事件</summary>
        public event Action<string> OnPlayEffect;

        /// <summary>动画结束事件</summary>
        public event Action OnAnimationEnd;

        // ========== 动画事件回调（在 Animation Clip 中设置） ==========

        /// <summary>
        /// 激活 Hitbox（动画事件调用）
        /// </summary>
        public void AnimEvent_HitboxOn()
        {
            OnHitboxActivate?.Invoke();
            Debug.Log("[AnimationEventRelay] Hitbox ON");
        }

        /// <summary>
        /// 关闭 Hitbox（动画事件调用）
        /// </summary>
        public void AnimEvent_HitboxOff()
        {
            OnHitboxDeactivate?.Invoke();
            Debug.Log("[AnimationEventRelay] Hitbox OFF");
        }

        /// <summary>
        /// 播放攻击音效（动画事件调用）
        /// </summary>
        public void AnimEvent_AttackSound()
        {
            OnPlaySound?.Invoke("attack");
        }

        /// <summary>
        /// 播放受击音效（动画事件调用）
        /// </summary>
        public void AnimEvent_HitSound()
        {
            OnPlaySound?.Invoke("hit");
        }

        /// <summary>
        /// 播放特效（动画事件调用）
        /// </summary>
        public void AnimEvent_Effect(string effectName)
        {
            OnPlayEffect?.Invoke(effectName);
        }

        /// <summary>
        /// 动画播放完毕（动画事件调用）
        /// </summary>
        public void AnimEvent_End()
        {
            OnAnimationEnd?.Invoke();
        }
    }
}