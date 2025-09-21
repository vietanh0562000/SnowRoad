using System.Collections.Generic;
using System.Linq;
using HoleBox;
using UnityEngine;

public class CreateLevelValidator
{
    public struct ValidationResult
    {
        public int    ContainerId;
        public Color  IdColor;
        public string ErrorMessage;
    }

    /// <summary>
    /// Kiểm tra mức độ hợp lệ của CreateLevel dựa trên BoxData và ContainerData.
    /// </summary>
    /// <param name="createLevel">CreateLevel cần kiểm tra</param>
    /// <param name="containerQueueDatas">Danh sách các ContainerQueueData để đối chiếu</param>
    /// <param name="results">Danh sách kết quả kiểm tra (chi tiết theo id)</param>
    /// <returns>True nếu hợp lệ, ngược lại trả về False</returns>
    public static bool ValidateLevel(List<BoxData> boxes, List<ContainerQueueData> containerQueueDatas, out List<ValidationResult> results)
    {
        var boxGroups = boxes
            .OfType<StickManData>() // Lọc các BoxData có kiểu TunnelData
            .GroupBy(box => box.id)
            .ToDictionary(group => group.Key, group => group.Sum(box => box.size.x * box.size.y));

        // Lấy danh sách ID tồn tại trong BoxData
        var boxIds = boxes.Select(box => box is StickManData ? box.id : -1).Distinct().ToHashSet();

        // Tìm các TunnelData trong BoxData (nếu có)
        var tunnelDataGroups = new Dictionary<int, int>();
        foreach (var box in boxes)
        {
            if (box is TunnelData tunnel)
            {
                if (tunnel.randomColor)
                {
                    Queue<int> colors = new(tunnel.colorQueue);

                    while (colors.Count > 0)
                    {
                        var id = colors.Dequeue();

                        if (!tunnelDataGroups.TryAdd(id, 4))
                        {
                            tunnelDataGroups[id] += 4;
                        }

                        boxIds.Add(id);
                    }
                }
                else
                {
                    var id = tunnel.id;

                    if (!tunnelDataGroups.TryAdd(id, 4 * tunnel.remainSpawn))
                    {
                        tunnelDataGroups[id] += 4 * tunnel.remainSpawn;
                    }

                    boxIds.Add(id);
                }
            }
        }

        // Gom nhóm các ContainerData theo id và tính tổng capacity của từng nhóm
        var containerGroups = containerQueueDatas
            .SelectMany(containerQueue => containerQueue.containerDatas)
            .Where(container => container.id > 0)
            .GroupBy(container => container.id)
            .ToDictionary(group => group.Key, group => group.Sum(container => container.capacity));

        // Kết hợp BoxData và TunnelData
        foreach (var tunnelGroup in tunnelDataGroups)
        {
            if (boxGroups.ContainsKey(tunnelGroup.Key))
            {
                boxGroups[tunnelGroup.Key] += tunnelGroup.Value; // Cộng size của TunnelData vào BoxData
            }
            else
            {
                boxGroups[tunnelGroup.Key] = tunnelGroup.Value; // Nếu chưa có, gán size của TunnelData
            }
        }


        results = new List<ValidationResult>();
        bool isValid = true;

        foreach (var containerGroup in containerGroups)
        {
            int id            = containerGroup.Key;
            int totalCapacity = containerGroup.Value;
            int boxSizeTotal  = boxGroups.GetValueOrDefault(id, 0);
            var idColor       = GameAssetManager.Instance.GetColor(id); // Lấy màu theo ID

            // So sánh tổng capacity với tổng size của Box
            if (boxSizeTotal != totalCapacity)
            {
                isValid = false;

                results.Add(new ValidationResult
                {
                    ContainerId  = id,
                    IdColor      = idColor,
                    ErrorMessage = $"Yêu cầu tổng capacity: {totalCapacity}, Nhưng BoxSize tổng cộng: {boxSizeTotal}"
                });
            }
        }


        // Lấy danh sách ID từ ContainerQueueData
        var containerIds = containerQueueDatas
            .SelectMany(queue => queue.containerDatas)
            .Select(container => container.id)
            .Distinct();

        // Tìm Container ID không có trong BoxData
        foreach (var containerId in containerIds)
        {
            if (!boxIds.Contains(containerId))
            {
                isValid = false;

                // Lấy màu sắc dựa trên ID
                var idColor = GameAssetManager.Instance.GetColor(containerId);

                results.Add(new ValidationResult
                {
                    ContainerId  = containerId,
                    IdColor      = idColor,
                    ErrorMessage = $"Container ID {containerId} không tồn tại trong BoxData."
                });
            }
        }

        foreach (var boxID in boxIds)
        {
            if (boxID < 0)
            {
                continue;
            }

            if (!containerIds.Contains(boxID))
            {
                isValid = false;

                // Lấy màu sắc dựa trên ID
                var idColor = GameAssetManager.Instance.GetColor(boxID);

                results.Add(new ValidationResult
                {
                    ContainerId  = boxID,
                    IdColor      = idColor,
                    ErrorMessage = $"Container ID {boxID} không tồn tại trong Queue."
                });
            }
        }

        return isValid;
    }
}