using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Unity.BossRoom.Gameplay.UI
{

    public class HyperlinkHandler : MonoBehaviour, IPointerMoveHandler
    {
        [SerializeField]
        private TMP_Text m_TmpText;
        private bool[] m_ActivatedLinks;

        private bool m_IsInitialized;
        public event UnityAction<string> OnHyperlinkMouseMovedOver;


        public void Clear()
        {
            m_ActivatedLinks = null;
            m_IsInitialized = false;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            Vector3 mousePos = new Vector3(eventData.position.x, eventData.position.y, 0);

            var linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TmpText, mousePos, null);

            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = m_TmpText.textInfo.linkInfo[linkIndex];
                string linkText = linkInfo.GetLinkText();

                if(m_IsInitialized == false)
                {
                    m_ActivatedLinks = new bool[m_TmpText.textInfo.linkCount];
                    m_IsInitialized = true;
                }

                if (m_ActivatedLinks[linkIndex] == false)
                {
                    m_ActivatedLinks[linkIndex] = true;
                    ChangeLinkColor(linkIndex);
                    OnHyperlinkMouseMovedOver?.Invoke(linkInfo.GetLinkID());
                }
            }
        }

        private void ChangeLinkColor(int linkIndex)
        {
            string pattern = @"<color=[^>]+><u><link";
            int matchCount = -1;

            m_TmpText.text = Regex.Replace(m_TmpText.text, pattern, match =>
            {
                matchCount++;
                return matchCount == linkIndex ? "<color=grey><u><link" : match.Value;
            }); ;
        }
    }
}
