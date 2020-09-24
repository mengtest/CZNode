using System;

namespace CZFramework.CZNode
{
    /// <summary> 自定义节点宽度特性 </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeWidthCoefficientAttribute : Attribute
    {
        public const int DefaultWidthCoefficient = 12;

        public readonly int WidthCoefficient;

        /// <summary> 节点的宽度，以格子为单位 </summary>
        public NodeWidthCoefficientAttribute(int widthCoefficient = DefaultWidthCoefficient) { WidthCoefficient = widthCoefficient; }
    }
}
