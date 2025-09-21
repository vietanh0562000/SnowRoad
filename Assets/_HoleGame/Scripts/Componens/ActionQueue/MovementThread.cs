namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using PuzzleGames;

    public class MovementThread : Singleton<MovementThread>
    {
        private bool _isEnd;

        private ActionQueue                         _actionQueues     = new ActionQueue();
        private Dictionary<AContainer, ActionQueue> _containerActions = new();

        public void AddAction(AContainer container, Func<UniTask> action, bool isPiority = false)
        {
            if (_isEnd) return;

            _containerActions.TryAdd(container, new ActionQueue());
            if (!_containerActions.TryGetValue(container, out var actionQueue)) return;
            actionQueue ??= new ActionQueue();

            if (isPiority)
            {
                actionQueue.AddPiorityAction(action);
            }
            else
            {
                actionQueue.AddAction(action);
            }
        }

        public void AddAction(Func<UniTask> action)
        {
            if (_isEnd) return;
            _actionQueues ??= new ActionQueue();
            _actionQueues.AddAction(action);
        }

        protected override void OnDestroy()
        {
            StopActionQueue();
            base.OnDestroy();
        }

        public void StopActionQueue()
        {
            _isEnd = true;
            _actionQueues.Stop();

            foreach (var containerAction in _containerActions)
            {
                containerAction.Value.Stop();
            }
        }
    }
}