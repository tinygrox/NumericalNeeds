using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace tinygrox.DuckovMods.NumericalStats.UIElements
{
    public class DraggableUIElement : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private RectTransform _rectTransform;
        private Vector2 _dragOffset;
        private bool _isDragging = false;

        private float _snapSize = 1f;

        public delegate void SavePositionDelegate(Vector2 position);
        public SavePositionDelegate OnSavePositionRequested;

        public bool IsDraggingAllowed { get; set; } = false;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform is null)
            {
                // Debug.LogError("[NumericalStats] DraggableUIElement: 需要一个 RectTransform 组件才能工作。");
                enabled = false;
            }
            if (_snapSize <= 0) _snapSize = 1f;
        }

        private void OnEnable()
        {
            if (DragToggleManager.Instance is not null)
            {
                DragToggleManager.Instance.RegisterDraggable(this);
            }
            else
            {
                // Debug.LogWarning("[NumericalStats] DraggableUIElement: DragToggleManager 未找到。拖拽元素将不会响应 Ctrl 键。");
            }
        }

        public void Initialize(Vector2 initialPosition)
        {
            if (_rectTransform is not null)
            {
                _rectTransform.anchoredPosition = SnapToGrid(initialPosition);
            }
        }

        private Vector2 SnapToGrid(Vector2 position)
        {
            float snappedX = Mathf.Round(position.x / _snapSize) * _snapSize;
            float snappedY = Mathf.Round(position.y / _snapSize) * _snapSize;
            return new Vector2(snappedX, snappedY);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsDraggingAllowed)
            {
                _isDragging = true;
                // Debug.Log($"[NumericalStats] {gameObject.name} DraggableUIElement: 拖拽开始 (Ctrl 键被按下)。");

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform.parent as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPointerPosition
                );
                _dragOffset = (Vector2)_rectTransform.localPosition - localPointerPosition;
            }
            else
            {
                _isDragging = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging && eventData.pointerCurrentRaycast.isValid)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _rectTransform.parent as RectTransform,
                        eventData.position,
                        eventData.pressEventCamera,
                        out Vector2 localPointerPosition
                    ))
                {
                    Vector2 targetLocalPosition = localPointerPosition + _dragOffset;
                    _rectTransform.localPosition = SnapToGrid(targetLocalPosition);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Vector2 finalSnappedPosition = SnapToGrid(_rectTransform.anchoredPosition);
                _rectTransform.anchoredPosition = finalSnappedPosition;
                OnSavePositionRequested?.Invoke(finalSnappedPosition);
                // Debug.Log("[NumericalStats] DraggableUIElement: 拖拽结束。");
            }
        }

        private void OnDisable()
        {
            _isDragging = false;
            IsDraggingAllowed = false;
            // Debug.Log("[NumericalStats] DraggableUIElement OnDisable: 拖拽状态已重置。");

            if (DragToggleManager.Instance is not null)
            {
                DragToggleManager.Instance.UnregisterDraggable(this);
            }
        }
    }
}
