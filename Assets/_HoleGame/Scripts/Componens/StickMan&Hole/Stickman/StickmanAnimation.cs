using UnityEngine;

namespace PuzzleGames
{
    using DG.Tweening;

    public class StickmanAnimation : MonoBehaviour
    {
        private static readonly int Run           = Animator.StringToHash("Run");
        private static readonly int Jump          = Animator.StringToHash("Jump");
        private static readonly int Fall          = Animator.StringToHash("Fall");
        private static readonly int Idle          = Animator.StringToHash("Idle");
        private static readonly int RandomJump    = Animator.StringToHash("RandJump");
        private static readonly int RandomRun     = Animator.StringToHash("RandRun");
        private static readonly int MultipleSpeed = Animator.StringToHash("MultipleSpeed");
        private static readonly int JumpAnimCount = 4;
        private static readonly int RunAnimCount  = 2;
        private static readonly int Wave          = Animator.StringToHash("Wave");
        private static readonly int Tap           = Animator.StringToHash("OnTap");
        private static readonly int JumpMiddle    = Animator.StringToHash("JumpMiddle");

        [SerializeField] private Animator  animator;
        [SerializeField] private Transform model;

        public float startScale = 1.45f;

        public void Reset()
        {
            model.localScale = Vector3.one * startScale;

            DOVirtual.DelayedCall(0.1f, () =>
            {
                int idleHash = Animator.StringToHash("Idle");

                animator.Play(idleHash, 0, Random.Range(0.1f, 1f));
            });
        }

        public void TriggerRun(float multiple = 1)
        {
            animator.SetTrigger(Run);
            animator.SetFloat(RandomRun, Random.Range(0, RunAnimCount));
            animator.SetFloat(MultipleSpeed, multiple);

            model.DOScale(1.4f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        public void TriggerJump()
        {
            animator.SetTrigger(Jump);
            animator.SetFloat(RandomJump, Random.Range(0, JumpAnimCount));
        }

        public void TriggerFall()
        {
            animator.SetTrigger(Fall);
            model.DOScale(1.4f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        public void TriggerJumpMiddle() { animator.SetTrigger(JumpMiddle); }

        public void SetInTunnel() { model.localScale = Vector3.one * 1.2f; }

        public void WaveAnim()
        {
            animator.SetFloat(RandomJump, Random.Range(0, JumpAnimCount));
            animator.SetTrigger(Wave);
        }

        public void OnTap() { animator.SetTrigger(Tap); }
    }
}