using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretOfLostTemple.Story
{
    public class StoryDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Item Config")]
        public string itemId;
        public StoryDropSlot targetSlot;
        public float snapDistance = 160f;

        private RectTransform rectTransform;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private Vector2 originalAnchoredPosition;
        private Vector2 originalSizeDelta;
        private Transform originalParent;
        private int originalSiblingIndex;
        private bool isSnapped = false;
        private bool hasBeenDragged = false;

        public bool IsSnapped => isSnapped;
        public event Action<StoryDragItem, StoryDropSlot> OnItemSnapped;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            originalAnchoredPosition = rectTransform.anchoredPosition;
            originalSizeDelta = rectTransform.sizeDelta;
            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
        }

        public void SetOriginalConfig(Transform parent, Vector2 pos, Vector2 size)
        {
            originalParent = parent;
            originalAnchoredPosition = pos;
            originalSizeDelta = size;
            if (!isSnapped && rectTransform != null)
            {
                rectTransform.anchoredPosition = pos;
                rectTransform.sizeDelta = size;
            }
        }

        public void ResetItem()
        {
            isSnapped = false;
            hasBeenDragged = false;
            if (originalParent != null)
            {
                transform.SetParent(originalParent, false);
                transform.SetSiblingIndex(originalSiblingIndex);
            }
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = originalAnchoredPosition;
                rectTransform.sizeDelta = originalSizeDelta;
            }
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isSnapped) return;
            hasBeenDragged = true;

            if (canvas == null) canvas = GetComponentInParent<Canvas>();

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0.88f;
            }

            // Bring to top-level canvas so dragging is never obscured by other panels
            if (canvas != null)
            {
                transform.SetParent(canvas.transform, true);
                transform.SetAsLastSibling();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isSnapped || !hasBeenDragged) return;
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isSnapped) return;
            if (!hasBeenDragged) return;
            hasBeenDragged = false;

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }

            // Check distance to target slot or if dropped over target slot
            if (targetSlot != null && !targetSlot.IsOccupied)
            {
                RectTransform slotRt = targetSlot.GetComponent<RectTransform>();
                if (slotRt != null)
                {
                    Camera cam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : (canvas != null ? canvas.worldCamera : null);
                    Vector2 itemScreen = RectTransformUtility.WorldToScreenPoint(cam, rectTransform.position);
                    Vector2 slotScreen = RectTransformUtility.WorldToScreenPoint(cam, slotRt.position);

                    float effectiveSnapDist = snapDistance * (Screen.height / 720f);
                    if (effectiveSnapDist < snapDistance) effectiveSnapDist = snapDistance;

                    bool isInsideSlot = RectTransformUtility.RectangleContainsScreenPoint(slotRt, itemScreen, cam);
                    bool isNearSlot = Vector2.Distance(itemScreen, slotScreen) <= effectiveSnapDist;

                    if (isInsideSlot || isNearSlot)
                    {
                        SnapToSlot(targetSlot);
                        return;
                    }
                }
            }

            // Return to original parent shelf and spot
            if (originalParent != null)
            {
                transform.SetParent(originalParent, false);
                transform.SetSiblingIndex(originalSiblingIndex);
            }
            rectTransform.anchoredPosition = originalAnchoredPosition;
            rectTransform.sizeDelta = originalSizeDelta;
        }

        public void SnapToSlot(StoryDropSlot slot)
        {
            isSnapped = true;
            hasBeenDragged = false;
            slot.SetOccupied(this);

            // Parent inside slot at perfect center and match slot dimensions
            transform.SetParent(slot.transform, false);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = slot.GetComponent<RectTransform>().sizeDelta;

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 1f;
            }

            Debug.Log($"[StoryDragItem] Successfully snapped item '{itemId}' into slot '{slot.slotId}'");
            OnItemSnapped?.Invoke(this, slot);
        }
    }
}
