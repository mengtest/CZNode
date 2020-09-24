using System.Collections;
using System.Collections.Generic;
using CZFramework.CZNode;
using UnityEngine;

[Title("Unity", "Input")]
[NodeWidthCoefficient(13)]
public class InputNode : NormalNode
{
    [Port(NodePort.PortDirection.Output, NodePort.PortCapacity.Multi, NodePort.PortTypeConstraint.Inherited)]
    public Vector2 input;


    [Port(NodePort.PortDirection.Output, NodePort.PortCapacity.Multi, NodePort.PortTypeConstraint.Inherited)]
    public float horizontal, vertical;

    public AxisType axisType;

    public override object GetValue(NodePort port)
    {
        switch (port.FieldName)
        {
            case "input":
                switch (axisType)
                {
                    case AxisType.Raw:
                        return new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                    case AxisType.Smooth:
                        return new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                }

                break;
            case "horizontal":
                switch (axisType)
                {
                    case AxisType.Raw:
                        return Input.GetAxisRaw("Horizontal");
                    case AxisType.Smooth:
                        return Input.GetAxis("Horizontal");
                }

                break;
            case "vertical":
                switch (axisType)
                {
                    case AxisType.Raw:
                        return Input.GetAxisRaw("Vertical");
                    case AxisType.Smooth:
                        return Input.GetAxis("Vertical");
                }

                break;
        }

        return Vector3.zero;
    }

    public enum AxisType
    {
        Smooth,
        Raw
    }
}