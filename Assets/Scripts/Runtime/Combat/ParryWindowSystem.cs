using UnityEngine;

namespace ShadowRhythm.Combat
{
    /// <summary>
    /// 弹反窗口系统 - 管理弹反时机判定
    /// </summary>
    public class ParryWindowSystem : MonoBehaviour
    {
        [Header("弹反设置")]
        [SerializeField] private float parryWindowMs = 100f; // 弹反窗口（毫秒）
        [SerializeField] private float flashWindowMs = 200f; // 闪避窗口（毫秒）

        /// <summary>弹反窗口（秒）</summary>
        public float ParryWindowSeconds => parryWindowMs / 1000f;

        /// <summary>闪避窗口（秒）</summary>
        public float FlashWindowSeconds => flashWindowMs / 1000f;

        /// <summary>
        /// 检查是否在弹反窗口内
        /// </summary>
        public bool IsInParryWindow(float inputTime, float attackTime)
        {
            float delta = Mathf.Abs(inputTime - attackTime) * 1000f;
            return delta <= parryWindowMs;
        }

        /// <summary>
        /// 检查是否在闪避窗口内
        /// </summary>
        public bool IsInFlashWindow(float inputTime, float attackTime)
        {
            float delta = Mathf.Abs(inputTime - attackTime) * 1000f;
            return delta <= flashWindowMs;
        }
    }
}