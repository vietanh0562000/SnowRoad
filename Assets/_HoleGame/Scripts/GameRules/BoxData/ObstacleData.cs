namespace HoleBox
{
    using System;
    using Newtonsoft.Json;

    [Serializable]
    public class ObstacleData : BoxData
    {
        public bool IsBarrier;
        public bool IsOpenBarrier = true;

        [JsonIgnore] public bool AlwaysOpen = false;

        public override void UpdateBoxData()
        {
            if (!IsBarrier) return;

            if (AlwaysOpen) return;

            IsOpenBarrier = !IsOpenBarrier;
            _isClaimed    = !IsOpenBarrier;
            OnUpdateData?.Invoke();
        }

        public override void InitData()
        {
            _isClaimed = !IsOpenBarrier;
            AlwaysOpen = false;
        }

        public void OpenBarrier()
        {
            AlwaysOpen = true;
            _isClaimed = true;
            OnUpdateData?.Invoke();
        }
    }
}