namespace HoleBox
{
    using UnityEngine;

    public class ComboManager : Singleton<ComboManager>
    {
        [Header("Combo Configuration")] [SerializeField]
        private float comboResetTime = 3.0f; // Thời gian reset combo nếu không có hành động

        private int   comboCount    = 0; // Số lần combo hiện tại
        private float lastComboTime = 0.0f; // Lưu thời gian lần combo cuối

        /// <summary>
        /// Xử lý logic cộng combo và phát âm thanh
        /// </summary>
        public void IncreaseCombo()
        {
            // Kiểm tra thời gian hiện tại để xác định có reset combo không
            if (Time.time - lastComboTime > comboResetTime)
            {
                ResetCombo();
            }

            // Tăng combo count
            comboCount++;
            lastComboTime = Time.time;

            // Phát âm thanh tương ứng với combo
            PlayComboSound();
        }

        /// <summary>
        /// Reset combo về 0.
        /// </summary>
        public void ResetCombo()
        {
            comboCount    = 0;
            lastComboTime = 0f;
        }

        /// <summary>
        /// Phát âm thanh tương ứng với combo hiện tại
        /// </summary>
        private void PlayComboSound() { AudioController.PlaySoundMatch3(comboCount); }
    }
}