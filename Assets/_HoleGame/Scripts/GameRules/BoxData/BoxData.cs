namespace HoleBox
{
    using System;
    using Newtonsoft.Json;
    using UnityEngine;

    [Serializable]
    public class BoxData
    {
        public int        id = -1;
        public Vector2Int size;
        public Vector2Int position;

        protected bool _isClaimed;

        [JsonIgnore] public bool IsClaimed { get => _isClaimed; set => _isClaimed = value; }

        public Vector3 GetMiddlePosition()
        {
            Vector2 tmp = size / 2;

            if (size.x % 2 == 0)
            {
                tmp.x -= 0.5f;
            }

            if (size.y % 2 == 0)
            {
                tmp.y -= 0.5f;
            }

            return new Vector3(position.x + tmp.x, 0, position.y + tmp.y);
        }

        public bool InsideBox(Vector2 point)
        {
            return point.x >= this.position.x && point.x < this.position.x + size.x &&
                   point.y >= this.position.y && point.y < this.position.y + size.y;
        }
        [JsonIgnore] public virtual bool IsAvailable => id > 0;

        [JsonIgnore]
        public int MatrixValue
        {
            get
            {
                if (IsClaimed)
                {
                    return 0;
                }

                if (IsAvailable)
                {
                    return id;
                }

                return -1;
            }
        }

        public virtual void UpdateBoxData() { OnUpdateData?.Invoke(); }
        public virtual void InitData()      { }

        [JsonIgnore] public Action OnUpdateData { get; set; }
    }
}