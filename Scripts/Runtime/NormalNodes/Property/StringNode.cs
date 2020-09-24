using UnityEngine;

namespace CZFramework.CZNode
{
    [Title("Normal", "String")]
    public class StringNode : NormalNode
    {
        [Port(NodePort.PortDirection.Output, TypeConstraint = NodePort.PortTypeConstraint.Inherited,
            ShowBackValue = PortAttribute.ShowBackingValue.Always)]
        [TextArea]
        public string text;

        public override object GetValue(NodePort port)
        {
            return text;
        }
    }
}