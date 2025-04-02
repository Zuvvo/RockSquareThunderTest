using System.Collections.Generic;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.UI
{
    public class TooltipPopupPool : MonoBehaviour
    {
        [SerializeField]
        private UITooltipPopup m_TooltipPrefab;

        [SerializeField]
        private int m_StartPoolSize = 1;

        private Queue<UITooltipPopup> m_Pool = new();
        private Canvas m_Canvas;

        public void Setup(Canvas canvas)
        {
            m_Canvas = canvas;
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < m_StartPoolSize; i++)
            {
                CreateNewTooltip();
            }
        }

        private UITooltipPopup CreateNewTooltip()
        {
            var tooltip = Instantiate(m_TooltipPrefab, transform);
            tooltip.gameObject.SetActive(false);
            m_Pool.Enqueue(tooltip);

            tooltip.Setup(m_Canvas);

            return tooltip;
        }

        public UITooltipPopup GetNewTooltip()
        {
            if (m_Pool.Count > 0)
            {
                var tooltip = m_Pool.Dequeue();
                tooltip.Clear();
                return tooltip;
            }
            else
            {
                return CreateNewTooltip();
            }
        }

        public void ReturnTooltip(UITooltipPopup tooltip)
        {
            tooltip.HideTooltip();
            m_Pool.Enqueue(tooltip);
        }
    }
}
