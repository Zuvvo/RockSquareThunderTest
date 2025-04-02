using System;
using Unity.BossRoom.Gameplay.Actions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.BossRoom.Gameplay.UI
{
    /// <summary>
    /// Attach to any UI element that should have a tooltip popup. If the mouse hovers over this element
    /// long enough, the tooltip will appear and show the specified text.
    /// </summary>
    /// <remarks>
    /// Having trouble getting the tooltips to show up? The event-handlers use physics raycasting, so make sure:
    /// - the main camera in the scene has a PhysicsRaycaster component
    /// - if you're attaching this to a UI element such as an Image, make sure you check the "Raycast Target" checkbox
    /// </remarks>
    public class UITooltipDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private TooltipManager m_TooltipManager;

        [SerializeField]
        [Multiline]
        [Tooltip("The text of the tooltip (this is the default text; it can also be changed in code)")]
        private string m_TooltipText;

        private bool m_IsShowingTooltip;
        private int m_InstanceId;

        private ActionConfig m_Config;
        private bool m_UseActionConfig;

        private void Awake()
        {
            m_InstanceId = gameObject.GetInstanceID();
            if(m_TooltipManager == null)
            {
                m_TooltipManager = FindObjectOfType<TooltipManager>();
            }
        }

        public void SetupActionConfig(ActionConfig config)
        {
            m_UseActionConfig = true;
            m_Config = config;
        }

        public void SetText(string text)
        {
            bool wasChanged = text != m_TooltipText;
            m_TooltipText = text;
            if (wasChanged && m_IsShowingTooltip)
            {
                // we changed the text while of our tooltip was being shown! We need to re-show the tooltip!
                HideTooltip();
                ShowTooltip();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        private void ShowTooltip()
        {
            string toDisplay = m_UseActionConfig ? m_TooltipText + Environment.NewLine + "[hl]Action details[/hl]" : m_TooltipText;
            m_TooltipManager.TryShowTooltip(m_InstanceId, toDisplay, m_Config);
        }

        private void HideTooltip()
        {
            m_TooltipManager.TryHideTooltip(m_InstanceId);
        }
    }
}
