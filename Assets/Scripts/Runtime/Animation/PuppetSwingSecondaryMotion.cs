using UnityEngine;

namespace ShadowRhythm.Animation
{
    /// <summary>
    /// 皮影次级晃动 - 给兵器、袖口、挂饰添加惯性晃动
    /// </summary>
    public class PuppetSwingSecondaryMotion : MonoBehaviour
    {
        [Header("晃动设置")]
        [SerializeField] private float swingIntensity = 15f;
        [SerializeField] private float swingDamping = 5f;
        [SerializeField] private float swingStiffness = 100f;

        [Header("触发设置")]
        [SerializeField] private float velocityThreshold = 1f;
        [SerializeField] private float hitImpulse = 30f;

        private float _currentSwing;
        private float _swingVelocity;
        private Vector3 _lastPosition;
        private float _baseRotationZ;

        private void Start()
        {
            _lastPosition = transform.position;
            _baseRotationZ = transform.localEulerAngles.z;
        }

        private void LateUpdate()
        {
            // 计算移动速度
            Vector3 currentPos = transform.position;
            Vector3 velocity = (currentPos - _lastPosition) / Time.deltaTime;
            _lastPosition = currentPos;

            // 根据速度添加晃动力
            if (velocity.magnitude > velocityThreshold)
            {
                // 横向移动产生晃动
                float impulse = velocity.x * swingIntensity * Time.deltaTime;
                _swingVelocity += impulse;
            }

            // 弹簧阻尼运动
            float springForce = -swingStiffness * _currentSwing;
            float dampingForce = -swingDamping * _swingVelocity;

            _swingVelocity += (springForce + dampingForce) * Time.deltaTime;
            _currentSwing += _swingVelocity * Time.deltaTime;

            // 应用晃动到旋转
            Vector3 euler = transform.localEulerAngles;
            euler.z = _baseRotationZ + _currentSwing;
            transform.localEulerAngles = euler;
        }

        /// <summary>
        /// 添加冲击力（受击时调用）
        /// </summary>
        public void AddImpulse(float direction = 1f)
        {
            _swingVelocity += hitImpulse * direction;
        }

        /// <summary>
        /// 重置晃动
        /// </summary>
        public void ResetSwing()
        {
            _currentSwing = 0f;
            _swingVelocity = 0f;
        }
    }
}