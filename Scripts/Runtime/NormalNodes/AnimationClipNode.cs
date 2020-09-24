using CZFramework.CZNode;
using UnityEngine;

[NodeTooltip("动画资源节点")]
[Title("Unity", "Animation Clip")]
public class AnimationClipNode : NormalNode
{
    [Port(NodePort.PortDirection.Output, NodePort.PortCapacity.Multi, NodePort.PortTypeConstraint.Inherited,
        PortAttribute.ShowBackingValue.Always)]
    public AnimationClip clip;

    [Port(NodePort.PortDirection.Output, NodePort.PortCapacity.Multi, NodePort.PortTypeConstraint.Inherited)]
    [NodeTooltip("Clip's length")]
    public float length;



    public override object GetValue(NodePort port)
    {
        switch (port.FieldName)
        {
            case "clip":
                return clip;
            case "length":
                return clip == null ? 0 : clip.length;
        }

        return null;
    }
}