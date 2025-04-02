using System.Collections;
using System.Collections.Generic;
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

        public event UnityAction<string> OnHyperlinkMouseMovedOver;

        public void OnPointerMove(PointerEventData eventData)
        {
            Vector3 mousePos = new Vector3(eventData.position.x, eventData.position.y, 0);

            var linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TmpText, mousePos, null);

            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = m_TmpText.textInfo.linkInfo[linkIndex];

                OnHyperlinkMouseMovedOver?.Invoke(linkInfo.GetLinkID());
            }
        }
    }
}
