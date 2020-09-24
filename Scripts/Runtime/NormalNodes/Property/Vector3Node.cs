using UnityEngine;

namespace CZFramework.CZNode
{
    [Title("Normal", "Vector3")]
    [NodeWidthCoefficient(12)]
    [NodeTitleTint(1,0,1)]
    public class Vector3Node : NormalNode
    {
        [Port(NodePort.PortDirection.Input, NodePort.PortCapacity.Single, NodePort.PortTypeConstraint.Inherited, PortAttribute.ShowBackingValue.Unconnected)]
        public float X, Y, Z;

        [Port(NodePort.PortDirection.Output, NodePort.PortCapacity.Multi, NodePort.PortTypeConstraint.Inherited)]
        public Vector3 vector3;

        public float outputX;
        [Port(NodePort.PortDirection.Output, NodePort.PortCapacity.Multi, NodePort.PortTypeConstraint.Inherited, PortName = "Y")]
        public float outputY;
        [Port(NodePort.PortDirection.Output, NodePort.PortCapacity.Multi, NodePort.PortTypeConstraint.Inherited, PortName = "Z")]
        public float outputZ;

        public override object GetValue(NodePort port)
        {
            switch (port.FieldName)
            {
                case "vector3":
                    return new Vector3(GetInputValue("X", X), GetInputValue("Y", Y), GetInputValue("Z", Z));
                case "outputX":
                    return GetInputValue("X", X);
                case "outputY":
                    return GetInputValue("Y", Y);
                case "outputZ":
                    return GetInputValue("Z", Z);
                default:
                    return null;
            }
        }
    }
}