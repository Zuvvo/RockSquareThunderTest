using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.BossRoom.Utils;
using UnityEngine.UI;

namespace Unity.BossRoom.Gameplay.UI
{
    /// <summary>
    /// This controls the tooltip popup -- the little text blurb that appears when you hover your mouse
    /// over an ability icon.
    /// </summary>
    public class UITooltipPopup : MonoBehaviour
    {
        [SerializeField]
        private Canvas m_Canvas;
        [SerializeField]
        private RectTransform m_TooltipHolder;
        [SerializeField]
        private TextMeshProUGUI m_TextField;
        [SerializeField]
        private Vector2 m_CursorOffset;
        [SerializeField]
        private float m_TooltipScreenBorderMargin;

        private void Awake()
        {
            Assert.IsNotNull(m_Canvas);
        }

        /// <summary>
        /// Shows a tooltip at the mouse coordinates.
        /// </summary>
        public void ShowTooltip(string text)
        {
            gameObject.SetActive(true);
            m_TextField.text = text;
            m_TooltipHolder.localPosition = GetPositionFromMouse(m_TooltipHolder as RectTransform);
        }

        /// <summary>
        /// Hides the current tooltip.
        /// </summary>
        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Maps screen coordinates to coordinates on our Canvas and clamps it to not go beyond the canvas.
        /// </summary>
        private Vector3 GetPositionFromMouse(RectTransform tooltipTransform)
        {
            Vector2 newPosition;
            var canvasBounds = new Bounds(Vector3.zero, (m_Canvas.transform as RectTransform).GetSize());

            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, Input.mousePosition, m_Canvas.worldCamera, out newPosition);

            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipTransform);

            float tooltipWidth = tooltipTransform.GetWidth();
            float tooltipHeight = tooltipTransform.GetHeight();

            newPosition.x += m_CursorOffset.x;
            newPosition.y += m_CursorOffset.y;

            float minXPos = -canvasBounds.size.x + m_TooltipScreenBorderMargin;
            float maxXPos = -m_TooltipScreenBorderMargin - tooltipWidth;

            float minYPos = -canvasBounds.size.y + m_TooltipScreenBorderMargin;
            float maxYPos = -m_TooltipScreenBorderMargin - tooltipHeight;

            newPosition.x = Mathf.Clamp(newPosition.x, minXPos, maxXPos);
            newPosition.y = Mathf.Clamp(newPosition.y, minYPos, maxYPos);

            return newPosition;
        }
    }
}
