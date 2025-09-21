using UnityEngine;

namespace PuzzleGames
{
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    public class InvokeEventDragHandle : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public UnityEvent OnDragStart;
        public UnityEvent OnDragEnd;

        public void OnBeginDrag(PointerEventData eventData) { OnDragStart?.Invoke(); }
        public void OnEndDrag(PointerEventData eventData)   { OnDragEnd?.Invoke(); }
    }
}