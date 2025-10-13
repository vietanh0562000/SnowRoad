using System;
using Spine.Unity;
using UnityEngine;

namespace BasePuzzle.PuzzlePackages.Demo
{
    public class IconAnimationsHandler : MonoBehaviour
    {
        [SerializeField] private IconAnimation[] _animations;

        public void OnNewTabSelected(int oldIndex, int newIndex)
        {
            AudioController.PlaySound(SoundKind.UIClickButton);
        }
    }

    [Serializable]
    public class IconAnimation
    {
        [SerializeField] private SkeletonGraphic _skeleton;
        
        [SpineAnimation(dataField = "_skeleton"), SerializeField]
        private string _playForward;

        [SpineAnimation(dataField = "_skeleton"), SerializeField]
        private string _playBackward;
        
        public void PlayForward()
        {
            
        }

        public void PlayBackward()
        {
            
        }
    }
}
