using System;
using UnityEngine;

namespace CZFramework.CZNode
{
    [Serializable]
    public class PortConnection
    {
        [SerializeField] private int nodeInstanceID;
        [SerializeField] public NodeData node;
        [SerializeField] public string fieldName;
        [NonSerialized] private NodePort port;

        public NodePort Port
        {
            get { return port != null ? port : port = GetPort(); }
        }

        public int NodeInstanceID
        {
            get { return nodeInstanceID; }
        }

        public PortConnection(NodePort port)
        {
            this.port = port;
            node = port.Node;
            nodeInstanceID = port.Node.GetInstanceID();
            fieldName = port.FieldName;
        }

        /// <summary> 返回此连接的目标接口 </summary>
        private NodePort GetPort()
        {
            if (node == null || string.IsNullOrEmpty(fieldName)) return null;
            return node.GetPort(fieldName);
        }
    }
}