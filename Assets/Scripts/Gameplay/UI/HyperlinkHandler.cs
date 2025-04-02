using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Unity.BossRoom.Gameplay.UI
{

    public class HyperlinkHandler : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private TMP_Text m_TmpText;

        public event UnityAction<string> OnHyperlinkClicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector3 mousePos = new Vector3(eventData.position.x, eventData.position.y, 0);

            var linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TmpText, mousePos, null);

            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = m_TmpText.textInfo.linkInfo[linkIndex];

                OnHyperlinkClicked?.Invoke(linkInfo.GetLinkID());
            }
        }
    }
}
