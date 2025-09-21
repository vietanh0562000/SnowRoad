using UnityEngine;

namespace PuzzleGames
{
    using BasePuzzle.Core.Scripts.ABTesting.Scripts.Model;
    using HoleBox;

    public class AddSlotBtn : BoosterBtn
    {
        protected override void OnClickButton()
        {
            if (TemporaryBoardVisualize.Instance.IsMaxSlot())
            {
                return;
            }

            base.OnClickButton();
        }

        public override void ActivateBooster()
        {
            HapticController.instance.Play();
            GameManager.Instance.SetPlayed();
            AudioController.PlaySound(SoundKind.UseBoosterAddSlot);
            MinusPowerUpCount();
            InGameTracker.UseBooster();
            TemporaryBoardVisualize.Instance.AddSlot();
        }
        public override int          LevelUnlock => ServerConfig.Instance<ValueRemoteConfig>().levelUnlockBooserAddSlot;
        public override ResourceType Type        => ResourceType.Powerup_AddSlot;
    }
}