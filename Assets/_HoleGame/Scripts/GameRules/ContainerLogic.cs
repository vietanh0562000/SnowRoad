namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class ContainerLogic
    {
        private List<ContainerData>[]           _containerQueues;
        private readonly List<ContainerData>             _staticContainers;
        private readonly WaitToProcessQueue<IngressData> _waitToProcessQueue;

        public Action<IngressData> OnLoseGame = null;

        private List<IngressData> listIngressData;
        private bool              isLose = false;

        public ContainerLogic(List<ContainerQueueData> containerQueues, ContainerQueueData staticContainers)
        {
            _containerQueues = new List<ContainerData>[containerQueues.Count];
            for (int i = 0; i < containerQueues.Count; i++)
            {
                _containerQueues[i] = new List<ContainerData>();
                foreach (var containerData in containerQueues[i].containerDatas)
                {
                    containerData.capacity = 4;
                    _containerQueues[i].Add(containerData); // Use Add instead of Enqueue
                }
            }

            _staticContainers   = staticContainers.containerDatas;

            foreach (var container in _staticContainers)
            {
                container.capacity = 8;
            }
            
            _waitToProcessQueue = new WaitToProcessQueue<IngressData>();

            isLose          = false;
            listIngressData = new List<IngressData>();
        }

        public void AddIngressData(IngressData ingressData)
        {
            if (isLose)
            {
                listIngressData.Add(ingressData);
                return;
            }

            _waitToProcessQueue.Enqueue(ingressData);
            OnQueueProcess();
        }

        public void AddIngressDataByUfo(IngressData ingressData)
        {
            List<(ContainerData, int)> listContainers = new();

            // Kiểm tra số lượng của ingressData, nếu bằng 0 thì kết thúc
            if (ingressData == null || ingressData.Number <= 0) return;

            // Duyệt qua từng hàng đợi trong _containerQueues
            foreach (var queue in _containerQueues)
            {
                // Duyệt qua từng ContainerData trong hàng đợi
                foreach (var container in queue)
                {
                    // Kiểm tra nếu container có ID khớp với ingressData.ID
                    if (container.ID == ingressData.ID)
                    {
                        listContainers.Add((container, queue.IndexOf(container)));
                    }
                }
            }

            listContainers.Sort((tuple1, tuple2) => tuple1.Item2.CompareTo(tuple2.Item2));

            foreach (var c in listContainers)
            {
                var container = c.Item1;

                // Tính toán lượng có thể thêm vào container
                int addable = Mathf.Min(container.Remaining, ingressData.Number);

                if (addable > 0)
                {
                    // Thêm dữ liệu vào container
                    container.UpdateNumber(addable);

                    // Giảm số lượng còn lại trong ingressData
                    ingressData.Number -= addable;

                    // Nếu đã thêm hết số lượng của ingressData -> Kết thúc
                    if (ingressData.Number <= 0) break;
                }
            }
        }
        
        public void AddIngressDataByHelicopter(IngressData ingressData, UfoTransporter ufo)
        {
            List<(ContainerData, int)> listContainers = new();

            // Kiểm tra số lượng của ingressData, nếu bằng 0 thì kết thúc
            if (ingressData == null || ingressData.Number <= 0) return;

            // Duyệt qua từng hàng đợi trong _containerQueues
            foreach (var queue in _containerQueues)
            {
                // Duyệt qua từng ContainerData trong hàng đợi
                foreach (var container in queue)
                {
                    // Kiểm tra nếu container có ID khớp với ingressData.ID
                    if (container.ID == ingressData.ID)
                    {
                        listContainers.Add((container, queue.IndexOf(container)));
                    }
                }
            }

            listContainers.Sort((tuple1, tuple2) => tuple1.Item2.CompareTo(tuple2.Item2));

            foreach (var c in listContainers)
            {
                var container = c.Item1;

                // Tính toán lượng có thể thêm vào container
                int addable = Mathf.Min(container.Remaining, ingressData.Number);

                if (addable > 0)
                {
                    // Thêm dữ liệu vào container
                    container.UpdateNumberWithUfo(addable, ufo);

                    // Giảm số lượng còn lại trong ingressData
                    ingressData.Number -= addable;

                    // Nếu đã thêm hết số lượng của ingressData -> Kết thúc
                    if (ingressData.Number <= 0) break;
                }
            }
        }

        private void SortQueue(int id)
        {
            _containerQueues = _containerQueues
                .OrderBy(queue => (queue.Count >= 1 && queue[0].ID == id) ? queue[0].Remaining : int.MaxValue)
                .ToArray();
        }


        private void OnQueueProcess()
        {
            var data = _waitToProcessQueue.Peek();

            if (isLose)
            {
                listIngressData.Add(data);
                return;
            }

            SortQueue(data.ID);

            // Step 1: Distribute to container queues
            bool distributed = false;
            for (int queueIndex = 0; queueIndex < _containerQueues.Length; queueIndex++)
            {
                var queue = _containerQueues[queueIndex];
                if (queue.Count <= 0) continue;
                var container = queue[0];
                if (container.ID == data.ID && container.CanUpdateQueue)
                {
                    var diff = container.Remaining - data.Number;

                    container.UpdateNumber(data.Number);
                    if (diff >= 0)
                    {
                        _waitToProcessQueue.Dequeue();
                        distributed = true;
                        break;
                    }

                    data.Number = -diff;
                }

                if (queue.Count >= 2 && queue[1].ID == container.ID && container.ID == data.ID)
                {
                    var diff = queue[1].Remaining - data.Number;

                    queue[1].UpdateNumber(data.Number);
                    if (diff >= 0)
                    {
                        _waitToProcessQueue.Dequeue();
                        distributed = true;
                        break;
                    }

                    data.Number = -diff;
                }
            }

            // Step 2: If not fully distributed, try static containers
            if (!distributed)
            {
                foreach (var staticContainer in _staticContainers)
                {
                    if (staticContainer.ID == data.ID && staticContainer.CanUpdateQueue)
                    {
                        var diff = staticContainer.Remaining - data.Number;

                        staticContainer.ChangeID(data.ID);
                        staticContainer.UpdateNumber(data.Number);

                        if (diff >= 0)
                        {
                            data.Number = 0;
                            _waitToProcessQueue.Dequeue();
                            distributed = true;

                            break;
                        }
                        else
                        {
                            data.Number = -diff;
                        }
                    }
                }

                if (data.Number > 0)
                {
                    foreach (var staticContainer in _staticContainers)
                    {
                        if (staticContainer.ID == -1 && !staticContainer.IsBusy)
                        {
                            staticContainer.ChangeID(data.ID);
                            var diff = staticContainer.Capacity - staticContainer.Number - data.Number;

                            staticContainer.UpdateNumber(data.Number);

                            if (diff >= 0)
                            {
                                _waitToProcessQueue.Dequeue();
                                distributed = true;

                                break;
                            }
                            else
                            {
                                data.Number = -diff;
                            }
                        }
                    }
                }

                // If still not distributed, lose game
                if (!distributed)
                {
                    isLose = true;

                    OnLoseGame?.Invoke(data);
                    _waitToProcessQueue.Dequeue();
                    return;
                }
            }
        }

        public void CheckEmptyContainer(ContainerData queueData)
        {
            for (int queueIndex = 0; queueIndex < _containerQueues.Length; queueIndex++)
            {
                var queue = _containerQueues[queueIndex];
                if (queue.Contains(queueData))
                {
                    queue.Remove(queueData);
                    break;
                }
            }

            ValidateContainers();
        }

        public void ValidateContainers()
        {
            if (isLose)
            {
                return;
            }

            bool isWaitQueueClear = false;
            for (int queueIndex = 0; queueIndex < _containerQueues.Length; queueIndex++)
            {
                var queue = _containerQueues[queueIndex];
                if (queue.Count <= 0) continue;
                var container = queue[0];
                if (container.Remaining != 0) continue;
                isWaitQueueClear = true;
            }

            if (isWaitQueueClear)
            {
                return;
            }

            foreach (var staticContainer in _staticContainers)
            {
                if (staticContainer.Number <= 0) continue;

                var totalNumber = staticContainer.Number;

                for (int queueIndex = 0; queueIndex < _containerQueues.Length; queueIndex++)
                {
                    if (totalNumber <= 0)
                    {
                        break;
                    }

                    var queue = _containerQueues[queueIndex];
                    if (queue.Count <= 0) continue;
                    var container = queue[0];

                    if (container.ID == staticContainer.ID &&
                        !staticContainer.IsBusy && container.CanUpdateQueue)
                    {
                        var delta = totalNumber < container.Remaining ? totalNumber : container.Remaining;

                        totalNumber -= delta;
                        staticContainer.Minus(container, delta);
                        container.UpdateNumber(delta, false);
                    }
                }
            }
        }

        public void LogAllContainers()
        {
            // Declare a StringBuilder for one-line formatted output
            System.Text.StringBuilder logBuilder = new System.Text.StringBuilder();

            // Log _containerQueues
            logBuilder.Append("Container Queues: ");
            logBuilder.Append("\n");
            if (_containerQueues != null)
            {
                for (int i = 0; i < _containerQueues.Length; i++)
                {
                    logBuilder.Append($"[Queue {i}: ");
                    if (_containerQueues[i] != null && _containerQueues[i].Count > 0)
                    {
                        foreach (var container in _containerQueues[i])
                        {
                            logBuilder.Append($"(ID: {container.ID}, N: {container.Number}) ");
                        }
                    }
                    else
                    {
                        logBuilder.Append("Empty ");
                    }

                    logBuilder.Append("] ");
                    logBuilder.Append("\n");
                }
            }
            else
            {
                logBuilder.Append("No container queues available. ");
            }

            logBuilder.Append("\n");
            // Log _staticContainers
            logBuilder.Append("Static Containers: ");
            logBuilder.Append("\n");
            if (_staticContainers != null && _staticContainers.Count > 0)
            {
                foreach (var container in _staticContainers)
                {
                    logBuilder.Append($"(ID: {container.ID}, Number: {container.Number}) ");
                }
            }
            else
            {
                logBuilder.Append("No static containers available.");
            }

            // Output the entire log in a single line
            Debug.LogError(logBuilder.ToString());
        }
        public void Revive(int noSlot)
        {
            var count = 0;

            for (int i = 0; i < listIngressData.Count; i++)
            {
                AddIngressDataByUfo(listIngressData[i]);
            }

            foreach (var staticContainer in _staticContainers)
            {
                if (staticContainer.ID < 0) continue;
                if (staticContainer.IsBusy) continue;
                if (staticContainer.Number <= 0) continue;

                if (count >= noSlot)
                {
                    break;
                }

                AddIngressDataByUfo(new IngressData(staticContainer.ID, staticContainer.number));

                count++;
                staticContainer.Reset();
            }

            isLose = false;
        }
    }
}