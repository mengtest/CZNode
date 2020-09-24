using UnityEngine;

namespace CZFramework.CZNode
{
    [Title("Normal", "Float")]
    public class FloatNode : NormalNode
    {
        [Port(NodePort.PortDirection.Output, NodePort.PortCapacity.Multi, NodePort.PortTypeConstraint.Inherited,
            PortAttribute.ShowBackingValue.Always, PortName = "Float")]
        public float value;

        public override object GetValue(NodePort port)
        {
            return value;
        }
    }
}