
namespace CZFramework.CZNode
{
    [NodeTooltip("乘法")]
    [Title("Normal", "Math", "Multiply")]
    public class MultiplyNode : NormalNode
    {
        [Port(NodePort.PortDirection.Input, NodePort.PortCapacity.Single, NodePort.PortTypeConstraint.Inherited,
            PortAttribute.ShowBackingValue.Unconnected)]
        public float x;
        
        [Port(NodePort.PortDirection.Input, NodePort.PortCapacity.Single, NodePort.PortTypeConstraint.Inherited,
            PortAttribute.ShowBackingValue.Unconnected)]
        public float y;
        
        [Port(NodePort.PortDirection.Output, NodePort.PortCapacity.Multi, NodePort.PortTypeConstraint.Inherited)]
        public float result;

        public override object GetValue(NodePort port)
        {
            return GetInputValue("x", x) * GetInputValue("y", y);
        }
    }
}