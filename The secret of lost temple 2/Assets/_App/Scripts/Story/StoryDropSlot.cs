using System;
using UnityEngine;
using UnityEngine.UI;

namespace SecretOfLostTemple.Story
{
    public class StoryDropSlot : MonoBehaviour
    {
        public string slotId;
        public Image slotBackground;
        public Image slotIcon;
        public Outline slotOutline;
        public Text slotLabel;

        public bool IsOccupied => CurrentItem != null;
        public StoryDragItem CurrentItem { get; private set; }

        public void SetOccupied(StoryDragItem item)
        {
            CurrentItem = item;
            if (slotOutline != null)
            {
                slotOutline.effectColor = new Color(0.2f, 1f, 0.4f, 1f); // Bright emerald green outline
            }
            if (slotBackground != null)
            {
                slotBackground.color = new Color(0.06f, 0.16f, 0.08f, 0.5f); // Soft green tint
            }
            if (slotIcon != null)
            {
                slotIcon.gameObject.SetActive(false); // Hide faint silhouette when real item is placed
            }
            if (slotLabel != null)
            {
                slotLabel.gameObject.SetActive(false); // Hide slot guide label so it does not collide with item name
            }
        }

        public void ResetSlot()
        {
            CurrentItem = null;
            if (slotOutline != null)
            {
                slotOutline.effectColor = new Color(1f, 0.85f, 0.3f, 0.85f); // Golden outline
            }
            if (slotBackground != null)
            {
                slotBackground.color = new Color(0.06f, 0.08f, 0.12f, 0.5f); // Dark stone hollow
            }
            if (slotIcon != null)
            {
                slotIcon.gameObject.SetActive(true);
                slotIcon.color = new Color(1f, 1f, 1f, 0.32f); // Faint guide silhouette
            }
            if (slotLabel != null)
            {
                slotLabel.gameObject.SetActive(true);
            }
        }
    }
}
