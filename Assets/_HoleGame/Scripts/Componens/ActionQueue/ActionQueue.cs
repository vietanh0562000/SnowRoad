using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ActionQueue : IDisposable
{
    private readonly List<Func<UniTask>>     _actionList              = new List<Func<UniTask>>(); // Normal actions
    private readonly List<Func<UniTask>>     _piorityActionList       = new List<Func<UniTask>>(); // Priority actions
    private          bool                    _isProcessing            = false;
    private          CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    /// Thêm một action vào danh sách thông thường.
    /// Action này phải trả về một `UniTask`.
    /// </summary>
    public void AddAction(Func<UniTask> action)
    {
        _actionList.Add(action);

        // Nếu không có hành động nào đang chạy, bắt đầu xử lý.
        if (!_isProcessing)
        {
            ProcessActions();
        }
    }

    /// <summary>
    /// Thêm một action ưu tiên vào danh sách (ở vị trí đầu tiên).
    /// Action này phải trả về một `UniTask`.
    /// </summary>
    public void AddPiorityAction(Func<UniTask> action)
    {
        _piorityActionList.Add(action); // Add to priority list

        // Nếu đang xử lý danh sách thông thường, tạm dừng
        if (_isProcessing)
        {
            _cancellationTokenSource.Cancel(); // Cancel current tasks in progress
        }
        else
        {
            ProcessActions(); // Start processing
        }
    }

    /// <summary>
    /// Xử lý các action trong danh sách ưu tiên trước, sau đó đến danh sách thông thường.
    /// </summary>
    private async void ProcessActions()
    {
        _isProcessing = true;

        while (_piorityActionList.Count > 0 || _actionList.Count > 0)
        {
            try
            {
                while (_piorityActionList.Count > 0)
                {
                    var priorityAction = _piorityActionList[0];
                    _piorityActionList.RemoveAt(0);

                    await priorityAction.Invoke();

                    await UniTask.Yield();
                }

                if (_piorityActionList.Count == 0 && _actionList.Count > 0)
                {
                    var normalAction = _actionList[0];
                    _actionList.RemoveAt(0);

                    await normalAction.Invoke();

                    // 👇 THÊM dòng này để tránh block main thread
                    await UniTask.Yield();
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("ActionQueue: Processing interrupted by priority actions.");
                ResetCancellationSource();
            }
            catch (Exception ex)
            {
                Debug.LogError($"ActionQueue: Error during action execution - {ex.Message}");
            }
        }

        _isProcessing = false;
    }


    /// <summary>
    /// Dừng ActionQueue và xóa tất cả các hành động trong danh sách.
    /// </summary>
    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _actionList.Clear();
        _piorityActionList.Clear();
        _isProcessing = false;
    }

    /// <summary>
    /// Hủy tài nguyên khi đối tượng bị hủy.
    /// </summary>
    public void Dispose()
    {
        Stop();
        _cancellationTokenSource.Dispose();
    }

    /// <summary>
    /// Reset the cancellation token source when processing is disrupted.
    /// </summary>
    private void ResetCancellationSource()
    {
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }
}