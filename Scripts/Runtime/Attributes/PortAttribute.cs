using System;

namespace CZFramework.CZNode
{
    /// <summary> 接口特性，标记此特性的字段将被绘制为一个接口 </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PortAttribute : Attribute
    {
        /// <summary> 接口方向 </summary>
        public readonly NodePort.PortDirection Direction;

        /// <summary> 接口数量规则 </summary>
        public NodePort.PortCapacity Capacity;

        /// <summary> 接口类型匹配规则 </summary>
        public NodePort.PortTypeConstraint TypeConstraint;

        /// <summary> 是否绘制字段 </summary>
        public ShowBackingValue ShowBackValue;

        /// <summary> 自定义端口显示名称 </summary>
        public string PortName;

        public Type CustomPortConnectionType;

        public PortAttribute(NodePort.PortDirection direction,
            NodePort.PortCapacity capacity = NodePort.PortCapacity.Multi,
            NodePort.PortTypeConstraint typeConstraint = NodePort.PortTypeConstraint.None,
            ShowBackingValue showBackingValue = ShowBackingValue.Never)
        {
            Direction = direction;
            Capacity = capacity;
            TypeConstraint = typeConstraint;
            ShowBackValue = showBackingValue;
        }

        public enum ShowBackingValue
        {
            /// <summary> 不显示 </summary>
            Never,

            /// <summary> 未连接时显示 </summary>
            Unconnected,

            /// <summary> 总是显示 </summary>
            Always
        }
    }
}