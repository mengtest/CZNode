using System.Collections;
using System.Collections.Generic;
using CZFramework.CZNode;
using UnityEngine;

namespace CZFramework.CZNode.Example.Math
{
    [Title("Math", "DisplayValue")]
    public class DisplayValueNode : NodeData
    {
        [Port(NodePort.PortDirection.Input, NodePort.PortCapacity.Single)]
        public string result;

        public override object GetValue(NodePort port)
        {
            return GetInputValue<object>("result", "");
        }
    }
}