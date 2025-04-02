using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Unity.BossRoom.Gameplay.Actions;
using Unity.BossRoom.ScriptableObjects;
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
        private TooltipPopupPool m_TooltipPopupPool;

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

        private List<UITooltipPopup> m_AdvancedTooltips = new();
        private ActionConfig m_ActionConfig;

        #region Unity callbacks
        private void Awake()
        {
            m_UITooltipPopup.Setup(m_Canvas);
            m_TooltipPopupPool.Setup(m_Canvas);
            SubscribeToTooltipActions(m_UITooltipPopup);
            CreateTooltipsDataDict();
        }

        private void OnDestroy()
        {
            UnsubscribeTooltipActions(m_UITooltipPopup);
        }

        private void Update()
        {
            if (m_IsTooltipBlocked == false && m_PointerEnterTime != 0 && m_CurrentTooltipRootInstanceId != -1 && (Time.time - m_PointerEnterTime) > m_blockTooltipDelay)
            {
                BlockBasicTooltip();
            }
        }
        #endregion

        #region Setup
        private void SubscribeToTooltipActions(UITooltipPopup popup)
        {
            popup.HyperlinkHandler.OnHyperlinkMouseMovedOver += TryOpenHyperlink;
            popup.OnTooltipPointerEnter += OnTooltipPoinerEnter;
            popup.OnTooltipPointerExit += TryHideTooltipsWithDelay;
        }

        private void UnsubscribeTooltipActions(UITooltipPopup popup)
        {
            popup.HyperlinkHandler.OnHyperlinkMouseMovedOver -= TryOpenHyperlink;
            popup.OnTooltipPointerEnter -= OnTooltipPoinerEnter;
            popup.OnTooltipPointerExit -= TryHideTooltipsWithDelay;
        }

        private void CreateTooltipsDataDict()
        {
            TooltipData[] tooltipsData = Resources.LoadAll<TooltipData>(TOOLTIPS_PATH);
            foreach (TooltipData data in tooltipsData)
            {
                if (m_TooltipsData.ContainsKey(data.LinkName) == false)
                {
                    m_TooltipsData.Add(data.LinkName, data);
                }
                else
                {
                    Debug.LogError($"Duplicated hyperlink: {data.LinkName}");
                }
            }
        }
        #endregion

        #region Show/Hide Tooltips
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
                TryHideTooltipsWithDelay(instanceId);
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

        public void TryShowTooltip(int instanceId, string tooltipText, ActionConfig config)
        {
            UpdatePointerOverState(instanceId, true);
            m_PointerEnterTime = Time.time;
            m_CurrentTooltipRootInstanceId = instanceId;

            //don't try to show the same tooltip if moved mouse from blocked tooltip to root obj
            if (m_IsTooltipBlocked == true && instanceId == m_UITooltipPopup.RootInstanceId)
            {
                return;
            }

            m_ActionConfig = config;
            m_tooltipText = tooltipText;
            m_UITooltipPopup.ShowTooltip(GetTextWithHyperlinks(m_tooltipText), m_CurrentTooltipRootInstanceId);
        }

        private void HideBasicTooltip()
        {
            m_IsTooltipBlocked = false;
            m_UITooltipPopup.HideTooltip();
            m_CurrentTooltipRootInstanceId = -1;
        }

        private void OnTooltipPoinerEnter(int rootInstanceId)
        {
            m_CurrentTooltipRootInstanceId = rootInstanceId;
        }

        private void TryHideTooltipsWithDelay(int rootInstanceId)
        {
            StartCoroutine(TryHideTooltipsOnMouseMovedAwayAfterDelay(rootInstanceId));
        }

        private IEnumerator TryHideTooltipsOnMouseMovedAwayAfterDelay(int rootInstanceId)
        {
            yield return new WaitForSecondsRealtime(0.2f);

            if (m_PointerOverState.TryGetValue(rootInstanceId, out bool isPointerOverRoot))
            {
                bool isPoitnerOverTooltip = m_UITooltipPopup.IsPointerOver || m_AdvancedTooltips.Exists(x => x.IsPointerOver);
                if (isPointerOverRoot == false && isPoitnerOverTooltip == false)
                {
                    HideBasicTooltip();
                    for (int i = 0; i < m_AdvancedTooltips.Count; i++)
                    {
                        var tooltip = m_AdvancedTooltips[i];
                        UnsubscribeTooltipActions(tooltip);
                        m_TooltipPopupPool.ReturnTooltip(tooltip);
                    }
                    m_AdvancedTooltips.Clear();
                }
            }
        }

        #endregion

        #region Hyperlinks
        private void TryOpenHyperlink(string link)
        {
            var tooltip = m_TooltipPopupPool.GetNewTooltip();
            string text = GetTextForHyperLink(link);
            tooltip.Setup(m_Canvas);
            tooltip.ShowTooltip(text, m_CurrentTooltipRootInstanceId);
            tooltip.BlockTooltip();
            SubscribeToTooltipActions(tooltip);
            m_AdvancedTooltips.Add(tooltip);
        }

        private string GetTextForHyperLink(string link)
        {
            if(m_ActionConfig != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Amount: {m_ActionConfig.Amount}");
                sb.AppendLine($"Mana cost: {m_ActionConfig.ManaCost}");
                sb.AppendLine($"Range: {m_ActionConfig.Range}");
                sb.AppendLine($"Duration: {m_ActionConfig.DurationSeconds}s");
                return sb.ToString();
            }
            else
            {
                return GetTextWithHyperlinks(m_TooltipsData[link].TooltipText);
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

                if (m_ActionConfig != null)
                {
                    result = result.Replace(extractedText, $"<color=white><u><link=\"{linkText}\">{linkText}</link></u></color>");
                    return result;
                }

                if (m_TooltipsData.TryGetValue(linkText, out TooltipData data))
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
        #endregion
    }

}
