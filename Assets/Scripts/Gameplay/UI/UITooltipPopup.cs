using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.BossRoom.Utils;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening.Core;
using UnityEngine.UIElements;

namespace Unity.BossRoom.Gameplay.UI
{
    /// <summary>
    /// This controls the tooltip popup -- the little text blurb that appears when you hover your mouse
    /// over an ability icon.
    /// </summary>
    public class UITooltipPopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private RectTransform m_TooltipHolder;
        [SerializeField]
        private TextMeshProUGUI m_TextField;
        [SerializeField]
        private HyperlinkHandler m_HyperlinkHandler;
        [SerializeField]
        private Vector2 m_CursorOffset;
        [SerializeField]
        private float m_TooltipScreenBorderMargin;
        [SerializeField]
        private CanvasGroup m_CanvasGroup;
        [SerializeField]
        private float m_FadeInTime = 0.2f;
        [SerializeField]
        private float m_FadeOutTime = 0.2f;

        [SerializeField]
        private CanvasGroup m_BlockedImageCanvasGroup;

        public HyperlinkHandler HyperlinkHandler => m_HyperlinkHandler;
        public event UnityAction<int> OnTooltipPointerEnter;
        public event UnityAction<int> OnTooltipPointerExit;
        public bool IsPointerOver { get; private set; }
        public int RootInstanceId { get; private set; }
        private Canvas m_Canvas;
        private Tweener m_HideTween;

        /// <summary>
        /// Shows a tooltip at the mouse coordinates.
        /// </summary>
        public void ShowTooltip(string text, int rootInstanceId)
        {
            transform.SetAsLastSibling();
            m_CanvasGroup.alpha = 0;
            gameObject.SetActive(true);

            m_HideTween?.Kill();
            m_CanvasGroup.DOFade(1, m_FadeInTime);


            m_TextField.text = text;
            m_HyperlinkHandler.Clear();

            RootInstanceId = rootInstanceId;
            m_TooltipHolder.localPosition = GetPositionFromMouse(m_TooltipHolder as RectTransform);
        }

        /// <summary>
        /// Hides the current tooltip.
        /// </summary>
        public void HideTooltip()
        {
            m_CanvasGroup.blocksRaycasts = false;
            m_BlockedImageCanvasGroup.alpha = 0;
            m_HideTween = m_CanvasGroup.DOFade(0, m_FadeOutTime).OnComplete(() => gameObject.SetActive(false));
        }

        public void Setup(Canvas canvas)
        {
            m_Canvas = canvas;
        }

        public void Clear()
        {
            m_TextField.text = string.Empty;
            m_CanvasGroup.alpha = 0;
            m_BlockedImageCanvasGroup.alpha = 0;
            RootInstanceId = -1;
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

        public void BlockTooltip()
        {
            m_CanvasGroup.blocksRaycasts = true;
            m_BlockedImageCanvasGroup.alpha = 0;
            m_BlockedImageCanvasGroup.DOFade(1, m_FadeInTime);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnTooltipPointerEnter?.Invoke(RootInstanceId);
            IsPointerOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnTooltipPointerExit?.Invoke(RootInstanceId);
            IsPointerOver = false;
        }
    }
}
