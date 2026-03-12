using UnityEngine;

namespace ShadowRhythm.Animation
{
    /// <summary>
    /// 皮影部件跟随器 - 让部件（如武器、饰品）延迟跟随骨骼
    /// </summary>
    public class PuppetPartFollower : MonoBehaviour
    {
        [Header("跟随目标")]
        [SerializeField] private Transform targetBone;

        [Header("延迟设置")]
        [SerializeField] private float positionLag = 0.05f;
        [SerializeField] private float rotationLag = 0.08f;

        [Header("限制")]
        [SerializeField] private float maxPositionOffset = 0.5f;
        [SerializeField] private float maxRotationOffset = 30f;

        private Vector3 _velocity;
        private float _angularVelocity;

        private void LateUpdate()
        {
            if (targetBone == null) return;

            // 位置延迟跟随
            Vector3 targetPos = targetBone.position;
            Vector3 currentPos = transform.position;

            Vector3 newPos = Vector3.SmoothDamp(currentPos, targetPos, ref _velocity, positionLag);

            // 限制最大偏移
            Vector3 offset = newPos - targetPos;
            if (offset.magnitude > maxPositionOffset)
            {
                newPos = targetPos + offset.normalized * maxPositionOffset;
            }

            transform.position = newPos;

            // 旋转延迟跟随（2D 只考虑 Z 轴）
            float targetAngle = targetBone.eulerAngles.z;
            float currentAngle = transform.eulerAngles.z;

            float newAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref _angularVelocity, rotationLag);

            // 限制最大旋转偏移
            float angleDiff = Mathf.DeltaAngle(newAngle, targetAngle);
            if (Mathf.Abs(angleDiff) > maxRotationOffset)
            {
                newAngle = targetAngle - Mathf.Sign(angleDiff) * maxRotationOffset;
            }

            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }

        /// <summary>
        /// 设置跟随目标
        /// </summary>
        public void SetTarget(Transform target)
        {
            targetBone = target;
        }

        /// <summary>
        /// 立即同步到目标位置
        /// </summary>
        public void SnapToTarget()
        {
            if (targetBone == null) return;
            transform.position = targetBone.position;
            transform.rotation = targetBone.rotation;
            _velocity = Vector3.zero;
            _angularVelocity = 0f;
        }
    }
}