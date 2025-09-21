namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UnityEngine;

    [Serializable]
    public class TunnelData : BoxData
    {
        public bool       randomColor;
        public Queue<int> colorQueue = new();
        public int        remainSpawn;
        public Vector2Int direction;


        [JsonIgnore] private         Queue<int> colors = new();
        [JsonIgnore] public override bool       IsAvailable => false;
        [JsonIgnore] public          int        Remain      => randomColor ? colors.Count : remainSpawn;

        public override void InitData()
        {
            base.InitData();
            colors = new(colorQueue);
        }

        public BoxData GetCurrentStickman()
        {
            var spawnID = id;

            if (randomColor)
            {
                if (colors.Count == 0)
                {
                    return null;
                }

                spawnID = colors.Peek();
            }
            else
            {
                if (remainSpawn <= 0)
                {
                    return null;
                }
            }

            return new StickManData()
            {
                position = position + direction * 2,
                id       = spawnID,
                size     = Vector2Int.one * 2
            };
        }

        public void SpawnStickman()
        {
            if (randomColor)
            {
                if (colors.Count == 0)
                {
                    return;
                }

                colors.Dequeue();
            }
            else
            {
                if (remainSpawn <= 0)
                {
                    return;
                }

                remainSpawn--;
            }
        }
    }
}