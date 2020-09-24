namespace CZFramework.CZNode
{
    [Title("Normal", "Int")]
    public class IntNode : NormalNode
    {
        [Port(NodePort.PortDirection.Output, NodePort.PortCapacity.Multi, NodePort.PortTypeConstraint.Inherited)]
        public float _int;

        public int value;

        public override object GetValue(NodePort port)
        {
            return (float) value;
        }
    }
}