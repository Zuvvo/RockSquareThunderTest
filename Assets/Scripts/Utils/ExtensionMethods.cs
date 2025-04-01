using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.BossRoom.Utils
{

    public static class ExtensionMethods
    {
        public static Vector2 GetSize(this RectTransform rectTransform)
        {
            return rectTransform.rect.size;
        }

        public static float GetWidth(this RectTransform rectTransform)
        {
            return rectTransform.rect.width;
        }

        public static float GetHeight(this RectTransform rectTransform)
        {
            return rectTransform.rect.height;
        }
    }
}
