using UnityEngine;

namespace Unity.BossRoom.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Tooltip Data", menuName = "ScriptableObjects/Tooltips")]
    public class TooltipData : ScriptableObject
    {
        public string LinkName;
        public string DisplayedText;
        [Multiline]
        public string TooltipText;
    }
}
