namespace HoleBox
{
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class LevelData
    {
        public Vector2Int               Matrix = new(16, 16);
        public List<BoxData>            Boxes;
        public StaticContainerConfig    StaticConfig;
        public List<ContainerQueueData> ContainerQueues;
        
        public LevelData(Vector2Int matrix, List<BoxData> boxes, StaticContainerConfig staticConfig, List<ContainerQueueData> containerQueues)
        {
            Matrix = matrix;
            Boxes = new List<BoxData>();

            foreach (var box in boxes)
            {
                Boxes.Add(box);
            }

            StaticConfig = new StaticContainerConfig()
            {
                Count = staticConfig.Count,
                Capacity = 32
            };
            
            ContainerQueues = new List<ContainerQueueData>();
            foreach (var queue in containerQueues)
            {
                var myQueue = new ContainerQueueData();
                foreach (var container in queue.containerDatas)
                {
                    container.capacity = 16;
                    myQueue.containerDatas.Add(container);
                }
                ContainerQueues.Add(myQueue);
            }
        }
    }
}