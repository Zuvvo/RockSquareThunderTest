using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.UI
{
    public class TooltipManager : MonoBehaviour
    {
        [SerializeField]
        private Canvas m_Canvas;

        [SerializeField]
        private UITooltipPopup m_UITooltipPopup;

        [SerializeField]
        private UITooltipPopup m_UITooltipAdvancedTempPopup;

        [SerializeField]
        [Tooltip("The length of time the mouse needs to hover over this element before the tooltip advanced appears (in seconds)")]
        private float m_AdvancedTooltipDelay = 1f;

        private float m_PointerEnterTime = 0;
        private string m_tooltipText;

        private int m_CurrentUITooltipDetectorInstanceId = -1;

        private void Awake()
        {
            m_UITooltipPopup.Setup(m_Canvas);
            m_UITooltipAdvancedTempPopup.Setup(m_Canvas);
        }

        public void TryHideTooltip(int instanceId)
        {
            HideBasicTooltip();
        }

        public void TryShowTooltip(int instanceId, string tooltipText)
        {
            m_PointerEnterTime = Time.time;
            m_CurrentUITooltipDetectorInstanceId = instanceId;
            m_tooltipText = tooltipText;
            m_UITooltipPopup.ShowTooltip(m_tooltipText);
        }

        private void HideBasicTooltip()
        {
            m_UITooltipPopup.HideTooltip();
            m_CurrentUITooltipDetectorInstanceId = -1;
        }

        private void Update()
        {
            if (m_PointerEnterTime != 0 && m_CurrentUITooltipDetectorInstanceId != -1 && (Time.time - m_PointerEnterTime) > m_AdvancedTooltipDelay)
            {
                HideBasicTooltip();
                m_UITooltipAdvancedTempPopup.ShowTooltip(m_tooltipText);
                Debug.Log("show advanced tooltip");
            }
        }
    }

}
