namespace HoleBox
{
    using System;
    using Newtonsoft.Json;
    using UnityEngine;
    using UnityEngine.Serialization;

    [Serializable]
    public class HoleBoxData : BoxData
    {
        /// <summary>
        /// Closed Hole
        /// </summary>
        public bool closedHole;

        public int numberToClose = 8;

        /// <summary>
        /// Locked Hole
        /// </summary>
        public bool lockedHole;

        public  Vector2Int keyPos = new Vector2Int();
        private bool       getKey;
        private bool       isKey;
        private Action     onUnlocked;

        public void OnStickEndMove()
        {
            numberToClose--;

            if (numberToClose == 0)
            {
                _isClaimed = true;
            }
        }

        public void UnlockKey()
        {
            if (!isKey) return;
            onUnlocked?.Invoke();
        }

        [JsonIgnore]
        public override bool IsAvailable
        {
            get
            {
                if (lockedHole && !getKey)
                {
                    return false;
                }

                return id > 0 && !_isClaimed;
            }
        }

        public void SetKey(Action unlock)
        {
            onUnlocked = unlock;
            isKey      = true;
        }

        public void SetUnlocked() { getKey = true; }
    }
}