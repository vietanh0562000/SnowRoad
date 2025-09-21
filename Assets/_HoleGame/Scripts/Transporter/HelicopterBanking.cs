namespace HoleBox
{
    using UnityEngine;
    using UnityEngine;

    public class HelicopterBanking : MonoBehaviour
    {
        [Header("Parent Reference")] [Tooltip("Nếu để null thì sẽ tự lấy transform.parent")]
        public Transform parentTransform;

        [Header("Bank (Roll) Settings")] [Tooltip("Góc nghiêng (roll) tối đa (°) khi quẹo trái/phải")]
        public float maxBankAngle = 30f;

        [Tooltip("Độ mượt khi chuyển nghiêng")]
        public float bankSmooth = 3f;

        [Header("Pitch Settings")] [Tooltip("Góc hạ/mũi (pitch) tối đa (°) khi tăng/giảm tốc")]
        public float maxPitchAngle = 10f;

        [Tooltip("Độ mượt khi chuyển pitch")] public float pitchSmooth = 3f;

        private Vector3 _lastParentPos;

        void Start()
        {
            if (parentTransform == null)
                parentTransform = transform.parent;
            _lastParentPos = parentTransform.position;
        }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0) return;

            // 1) Tính velocity của parent
            Vector3 currentPos = parentTransform.position;
            Vector3 worldVel   = (currentPos - _lastParentPos) / dt;
            _lastParentPos = currentPos;

            // Nếu gần như đứng yên thì quay dần về local-identity
            if (worldVel.sqrMagnitude < 0.001f)
            {
                transform.localRotation = Quaternion.Slerp(
                    transform.localRotation,
                    Quaternion.identity,
                    Mathf.Max(bankSmooth, pitchSmooth) * dt
                );
                return;
            }

            // 2) Chuyển velocity về local space của parent
            Vector3 localVel = parentTransform.InverseTransformDirection(worldVel);

            // 3) Tính góc roll (nghiêng trái/phải) dựa trên thành phần lateral (x)
            float normalizedLateral = Mathf.Clamp(localVel.x / worldVel.magnitude, -1f, 1f);
            float targetRoll        = -normalizedLateral * maxBankAngle;

            // 4) Tính góc pitch (mũi lên/xuống) dựa trên thành phần forward (z)
            float normalizedForward = Mathf.Clamp(localVel.z / worldVel.magnitude, -1f, 1f);
            // khi bay về phía trước, thường muốn hạ mũi xuống một chút → dấu âm
            float targetPitch = -normalizedForward * maxPitchAngle;

            // 5) Lerp về rotation mục tiêu (cùng yaw=0, chỉ local pitch/roll)
            Quaternion targetLocalRot = Quaternion.Euler(targetPitch, 0f, targetRoll);
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetLocalRot,
                dt * Mathf.Max(bankSmooth, pitchSmooth)
            );
        }
    }
}