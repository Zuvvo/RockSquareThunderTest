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
        [Tooltip("Time the mouse needs to hover over this element before the tooltip blocks (in seconds)")]
        private float m_blockTooltipDelay = 1f;

        private float m_PointerEnterTime = 0;
        private string m_tooltipText;

        private int m_CurrentTooltipRootInstanceId = -1;
        private bool m_IsTooltipBlocked;

        private const string TOOLTIPS_PATH = "Tooltips";
        private const string HYPERLINK_PATTERN = @"\[hl\](.*?)\[/hl\]";

        private Dictionary<string, TooltipData> m_TooltipsData = new();
        private Dictionary<int, bool> m_PointerOverState = new();

        private void Awake()
        {
            m_UITooltipPopup.Setup(m_Canvas);
            m_UITooltipPopup.HyperlinkHandler.OnHyperlinkMouseMovedOver += TryOpenHyperlink;

            m_UITooltipAdvancedTempPopup.Setup(m_Canvas);

            m_UITooltipPopup.OnTooltipPointerEnter += OnTooltipPoinerEnter;
            m_UITooltipPopup.OnTooltipPointerExit += TryHideTooltipWithDelay;

            CreateTooltipsDict();
        }

        private void Update()
        {
            if (m_IsTooltipBlocked == false && m_PointerEnterTime != 0 && m_CurrentTooltipRootInstanceId != -1 && (Time.time - m_PointerEnterTime) > m_blockTooltipDelay)
            {
                BlockBasicTooltip();
              //  HideBasicTooltip();
              //  ShowAdvancedTooltip();
            }
        }

        private void BlockBasicTooltip()
        {
            m_IsTooltipBlocked = true;
            m_UITooltipPopup.BlockTooltip();
        }

        public void TryHideTooltip(int instanceId)
        {
            UpdatePointerOverState(instanceId, false);
            m_CurrentTooltipRootInstanceId = -1;
            if(m_IsTooltipBlocked == false)
            {
                HideBasicTooltip();
            }
            else
            {
                TryHideTooltipWithDelay(instanceId);
            }
        }

        private void UpdatePointerOverState(int instanceId, bool state)
        {
            if (m_PointerOverState.ContainsKey(instanceId))
            {
                m_PointerOverState[instanceId] = state;
            }
            else
            {
                m_PointerOverState.Add(instanceId, state);
            }
        }

        public void TryShowTooltip(int instanceId, string tooltipText)
        {
            UpdatePointerOverState(instanceId, true);
            m_PointerEnterTime = Time.time;
            m_CurrentTooltipRootInstanceId = instanceId;

            //don't try to show the same tooltip if moved mouse from blocked tooltip to root obj
            if (m_IsTooltipBlocked == true && instanceId == m_UITooltipPopup.RootInstanceId)
            {
                return;
            }

            m_tooltipText = tooltipText;
            m_UITooltipPopup.ShowTooltip(GetTextWithHyperlinks(m_tooltipText), m_CurrentTooltipRootInstanceId);
        }

        private void HideBasicTooltip()
        {
            m_IsTooltipBlocked = false;
            m_UITooltipPopup.HideTooltip();
            m_CurrentTooltipRootInstanceId = -1;
        }

        private void TryOpenHyperlink(string link)
        {
            Debug.Log(link);
            //TooltipData data = m_TooltipsData[link];
            //m_tooltipText = data.TooltipText;
            //ShowAdvancedTooltip();
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
            if(string.IsNullOrEmpty(m_tooltipText) == false)
            {
                m_tooltipText = GetTextWithHyperlinks(m_tooltipText);
                m_UITooltipAdvancedTempPopup.ShowTooltip(m_tooltipText, m_CurrentTooltipRootInstanceId);
            }
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

        private void OnTooltipPoinerEnter(int rootInstanceId)
        {
            m_CurrentTooltipRootInstanceId = rootInstanceId;
        }

        private void TryHideTooltipWithDelay(int rootInstanceId)
        {
            StartCoroutine(TryHideTooltipOnMouseMovedAwayAfterDelay(rootInstanceId));
        }

        private IEnumerator TryHideTooltipOnMouseMovedAwayAfterDelay(int rootInstanceId)
        {
            yield return new WaitForSecondsRealtime(0.2f);

            if(m_PointerOverState.TryGetValue(rootInstanceId, out bool isPointerOverRoot))
            {
                bool isPoitnerOverTooltip = m_UITooltipPopup.IsPointerOver;
                if(isPointerOverRoot == false && isPoitnerOverTooltip == false)
                {
                    HideBasicTooltip();
                }
            }
        }
    }

}
