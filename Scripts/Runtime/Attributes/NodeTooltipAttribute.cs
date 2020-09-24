using System;

namespace CZFramework.CZNode
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Field)]
    public class NodeTooltipAttribute : Attribute
    {
        public string Tooltip;

        public NodeTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }
}