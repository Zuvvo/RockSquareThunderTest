using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.BossRoom.ScriptableObjects;
using System.Text.RegularExpressions;

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
        private string m_AdvancedTooltipText;

        private int m_CurrentUITooltipDetectorInstanceId = -1;
        private bool m_IsListeningForClick;

        private const string TOOLTIPS_PATH = "Tooltips";
        private const string HYPERLINK_PATTERN = @"\[hl\](.*?)\[/hl\]";

        private Dictionary<string, TooltipData> m_TooltipsData = new();

        private void Awake()
        {
            m_UITooltipPopup.Setup(m_Canvas);
            m_UITooltipAdvancedTempPopup.Setup(m_Canvas);
            m_UITooltipAdvancedTempPopup.HyperlinkHandler.OnHyperlinkClicked += OnHyperLinkClicked;

            CreateTooltipsDict();
        }

        private void Update()
        {
            if (m_PointerEnterTime != 0 && m_CurrentUITooltipDetectorInstanceId != -1 && (Time.time - m_PointerEnterTime) > m_AdvancedTooltipDelay)
            {
                HideBasicTooltip();
                ShowAdvancedTooltip();
            }

            //if (m_IsListeningForClick && UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetMouseButtonDown(1) || UnityEngine.Input.GetMouseButtonDown(2))
            //{
            //    m_UITooltipAdvancedTempPopup.HideTooltip();
            //    m_IsListeningForClick = false;
            //}
        }

        public void TryHideTooltip(int instanceId)
        {
            HideBasicTooltip();
        }

        public void TryShowTooltip(int instanceId, string tooltipText, string advancedTooltipText)
        {
            m_PointerEnterTime = Time.time;
            m_CurrentUITooltipDetectorInstanceId = instanceId;
            m_tooltipText = tooltipText;
            m_AdvancedTooltipText = advancedTooltipText;
            m_UITooltipPopup.ShowTooltip(m_tooltipText);
        }

        private void HideBasicTooltip()
        {
            m_UITooltipPopup.HideTooltip();
            m_CurrentUITooltipDetectorInstanceId = -1;
        }

        private void OnHyperLinkClicked(string link)
        {
            Debug.Log("clicked: " + link);
        }

        private void CreateTooltipsDict()
        {
            TooltipData[] tooltipsData = Resources.LoadAll<TooltipData>(TOOLTIPS_PATH);
            foreach (TooltipData data in tooltipsData)
            {
                if(m_TooltipsData.ContainsKey(data.LinkName) == false)
                {
                    m_TooltipsData.Add(data.LinkName, data);
                }
                else
                {
                    Debug.LogError($"Duplicated hyperlink: {data.LinkName}");
                }
            }
        }

        private void ShowAdvancedTooltip()
        {
            m_AdvancedTooltipText = GetTextWithHyperlinks(m_AdvancedTooltipText);
            m_UITooltipAdvancedTempPopup.ShowTooltip(m_AdvancedTooltipText);
            m_IsListeningForClick = true;
        }

        private string GetTextWithHyperlinks(string input)
        {
            string result = input;
            Match match = Regex.Match(result, HYPERLINK_PATTERN);

            MatchCollection matches = Regex.Matches(input, HYPERLINK_PATTERN);

            for (int i = 0; i < matches.Count; i++)
            {
                string extractedText = matches[i].Value;
                string linkText = matches[i].Groups[1].Value;

                if(m_TooltipsData.TryGetValue(linkText, out TooltipData data))
                {
                    result = result.Replace(extractedText, $"<color=white><u><link=\"{data.LinkName}\">{data.DisplayedText}</link></u></color>");
                }
                else
                {
                    result = result.Replace(extractedText, linkText);
                    Debug.LogError($"Can't find hyperlink data: {linkText}");
                }
            }

            return result;
        }
    }

}
