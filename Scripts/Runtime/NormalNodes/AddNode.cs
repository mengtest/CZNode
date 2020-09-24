
namespace CZFramework.CZNode
{
    [NodeTooltip("加法")]
    [Title("Normal", "Math", "Add")]
    public class AddNode : NormalNode
    {
        [Port(NodePort.PortDirection.Input, NodePort.PortCapacity.Single, NodePort.PortTypeConstraint.Inherited,
            PortAttribute.ShowBackingValue.Unconnected)]
        public float x, y;

        [Port(NodePort.PortDirection.Output, NodePort.PortCapacity.Multi, NodePort.PortTypeConstraint.Inherited)]
        public float result;
        
        public override object GetValue(NodePort port)
        {
            return GetInputValue("x", x) + GetInputValue("y", y);
        }
    }
}