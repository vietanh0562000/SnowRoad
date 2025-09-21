namespace HoleBox
{
    using System;
    using Newtonsoft.Json;

    [Serializable]
    public class StickManData : BoxData
    {
        public bool IsHidden;
        public int  intFrozen;

        private bool hidden;

        [JsonIgnore] public Action<UfoTransporter> OnClaim;
        [JsonIgnore] public Action ShowFX;

        [JsonIgnore] public bool IsFrozen     => intFrozen > 0;
        public              void ShowHidden() { hidden = false; }
        public              void OnHidden()   { hidden = true; }

        public override void InitData()
        {
            if (IsHidden)
            {
                hidden = true;
            }
        }

        public void OnStickmanMoveHole()
        {
            if (intFrozen > 0)
            {
                intFrozen--;
            }
        }

        [JsonIgnore]
        public override bool IsAvailable
        {
            get
            {
                if (intFrozen > 0)
                {
                    return false;
                }

                if (IsHidden)
                {
                    return !hidden;
                }

                return base.IsAvailable;
            }
        }

        [JsonIgnore] public int SizeCount => 1;

        public void OnReset()
        {
            if (IsHidden)
            {
                OnHidden();
            }
        }
        public void Claim(UfoTransporter ufo) { OnClaim?.Invoke(ufo); }
        public void UseUfo()
        {
            _isClaimed = true; 
            ShowFX?.Invoke();
        }
    }
}