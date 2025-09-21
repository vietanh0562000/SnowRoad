namespace HoleBox
{
    using DG.Tweening;
    using Spine.Unity;
    using UnityEngine;

    public class Barrier : MonoBehaviour
    {
        public Transform         _barrierTrans;
        public SkeletonAnimation _barrierAnim;

        private bool IsOpen;
        
        public void SetBarrier(bool isOpen)
        {
            if (isOpen == IsOpen)
            {
                return;
            }

            IsOpen = isOpen;
            _barrierAnim.AnimationState.SetAnimation(0, isOpen ? "open" : "close", false);

            AudioController.PlaySound(isOpen ? SoundKind.BarrierOn : SoundKind.BarrierOff);
        }
    }
}