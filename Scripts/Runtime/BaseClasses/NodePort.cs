using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CZFramework.CZNode
{
    [Serializable]
    public class NodePort
    {
        public static string GetBothName(string fieldName,PortDirection direction)
        {
            switch (direction)
            {
                case PortDirection.Input:
                    return fieldName + " Iutput";
                case PortDirection.Output:
                    return fieldName + " Onput";
                default:
                    return fieldName;
            }
        }

        [SerializeField] private string fieldName;
        [SerializeField] private NodeData node;
        [SerializeField] private PortDirection direction = PortDirection.Input;
        [SerializeField] private PortCapacity capacity = PortCapacity.Multi;
        [SerializeField] private PortTypeConstraint typeConstraint;
        [SerializeField] private Type dataType;
        [SerializeField] private string _typeQualifiedName;
        [SerializeField] public List<PortConnection> connections = new List<PortConnection>();

        public string FieldName { get { return fieldName; } }

        public NodeData Node { get { return node; } }

        public PortDirection Direction { get { return direction; } }

        public PortCapacity Capacity { get { return capacity; } }

        public PortTypeConstraint TypeConstraint { get { return typeConstraint; } }

        public Type DataType
        {
            get
            {
                if (dataType == null && !string.IsNullOrEmpty(_typeQualifiedName)) dataType = Type.GetType(_typeQualifiedName, false);
                return dataType;
            }
            set
            {
                dataType = value;
                if (value != null) _typeQualifiedName = value.AssemblyQualifiedName;
            }
        }

        /// <summary> 返回第一个不为空的连接 </summary>
        public NodePort Connection
        {
            get
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    if (connections[i] != null) return connections[i].Port;
                }

                return null;
            }
        }

        /// <summary> 连接数 </summary>
        public int ConnectionCount
        {
            get { return connections.Count; }
        }

        /// <summary> 是否与任意端口连接？ </summary>
        public bool IsConnected
        {
            get { return ConnectionCount != 0; }
        }

        public NodePort(FieldInfo fieldInfo)
        {
            fieldName = fieldInfo.Name;

            if (AttributeCache.TryGetFieldAttribute(fieldInfo.DeclaringType, fieldInfo.Name, out PortAttribute attribute))
            {
                direction = attribute.Direction;
                capacity = attribute.Capacity;
                typeConstraint = attribute.TypeConstraint;
            }

            if (attribute != null && attribute.CustomPortConnectionType != null)
                DataType = attribute.CustomPortConnectionType;
            else
                DataType = fieldInfo.FieldType;
        }

        public NodePort(FieldInfo fieldInfo, string name, PortDirection direction)
        {
            fieldName = name;

            if (AttributeCache.TryGetFieldAttribute(fieldInfo.DeclaringType, fieldInfo.Name, out PortAttribute attribute))
            {
                capacity = attribute.Capacity;
                typeConstraint = attribute.TypeConstraint;
            }
            this.direction = direction;

            if (attribute != null && attribute.CustomPortConnectionType != null)
                DataType = attribute.CustomPortConnectionType;
            else
                DataType = fieldInfo.FieldType;
        }

        public NodePort(NodePort port, NodeData node)
        {
            fieldName = port.fieldName;
            direction = port.direction;
            capacity = port.capacity;
            typeConstraint = port.typeConstraint;
            DataType = port.DataType;
            this.node = node;
        }

        public void Reload(NodePort port)
        {
            fieldName = port.fieldName;
            direction = port.direction;
            capacity = port.capacity;
            typeConstraint = port.typeConstraint;
            DataType = port.DataType;
        }

        /// <summary> 依照旧节点连接新节点 </summary>
        public void Redirect(List<NodeData> oldNodes, List<NodeData> newNodes)
        {
            foreach (PortConnection connection in connections)
            {
                int index = oldNodes.IndexOf(connection.node);
                if (index >= 0) connection.node = newNodes[index];
            }
        }

        /// <summary> 获取此接口连接的所有接口 </summary>
        public List<NodePort> GetConnections()
        {
            List<NodePort> result = new List<NodePort>();
            foreach (PortConnection connection in connections)
            {
                if (connection.Port != null)
                    result.Add(connection.Port);

            }
            return result;
        }

        /// <summary> 从此接口获取值 </summary>
        public object GetValue()
        {
            return node.GetValue(this);
        }

        /// <summary> 获取第一个连接的接口的返回值 </summary>
        public object GetConnectValue()
        {
            NodePort port = Connection;
            if (port == null) return null;
            return port.GetValue();
        }

        /// <summary> 获取所有连接的接口返回值 </summary>
        public object[] GetConnectValues()
        {
            object[] values = new object[ConnectionCount];
            for (int i = 0; i < connections.Count; i++)
            {
                NodePort targetPort = connections[i].Port;
                values[i] = targetPort.node.GetValue(targetPort);
            }

            return values;
        }

        /// <summary> 检查接口兼容性 </summary>
        public bool IsCompatible(NodePort targetPort)
        {
            if (targetPort == this || targetPort.node == this.node)
                return false;

            if (direction == targetPort.direction)
                return false;

            if (targetPort.TypeConstraint == PortTypeConstraint.None || TypeConstraint == PortTypeConstraint.None) return true;
            if ((TypeConstraint == PortTypeConstraint.Inherited && dataType.IsAssignableFrom(targetPort.dataType))) return true;
            if (TypeConstraint == PortTypeConstraint.Strict && dataType == targetPort.dataType) return true;
            return false;
        }

        /// <summary> 与另一个接口连接，如果可以连接的话 </summary>
        public void Connect(NodePort targetPort)
        {
            if (targetPort == null)
            {
                Debug.LogWarning("Cannot connect to null port");
                return;
            }

            if (IsConnectedTo(targetPort))
            {
                Debug.LogWarning("Port already connected. ");
                return;
            }

            if (!IsCompatible(targetPort) || !targetPort.IsCompatible(this))
                return;

            connections.Add(new PortConnection(targetPort));
            if (targetPort.connections == null) targetPort.connections = new List<PortConnection>();
            if (!targetPort.IsConnectedTo(this))
                targetPort.connections.Add(new PortConnection(this));
            node.OnCreateConnection(this, targetPort);
            targetPort.node.OnCreateConnection(targetPort, this);
        }

        /// <summary> 断开与指定接口的连接 </summary>
        public void Disconnect(NodePort targetPort)
        {
            // 删除此接口与其它接口的连接
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                if (connections[i].Port == targetPort)
                {
                    connections.RemoveAt(i);
                }
            }

            if (targetPort != null)
            {
                // 移除其它接口与此接口的连接
                for (int i = 0; i < targetPort.connections.Count; i++)
                {
                    if (targetPort.connections[i].Port == this)
                    {
                        targetPort.connections.RemoveAt(i);
                    }
                }
            }

            // 触发断开连接的方法
            node.OnRemoveConnection(this, targetPort);
            if (targetPort != null) targetPort.node.OnRemoveConnection(targetPort, this);
        }

        /// <summary> 断开此接口与其它接口的连接 </summary>
        public void Disconnect(int i)
        {
            // 移除其它接口与此接口的连接
            NodePort targetPort = connections[i].Port;
            if (targetPort != null)
            {
                for (int k = 0; k < targetPort.connections.Count; k++)
                {
                    if (targetPort.connections[k].Port == this)
                    {
                        targetPort.connections.RemoveAt(k);
                        k--;
                    }
                }
            }

            // 删除此接口与其它接口的连接
            connections.RemoveAt(i);

            // 触发断开连接的方法
            node.OnRemoveConnection(this, targetPort);
            if (targetPort != null) targetPort.node.OnRemoveConnection(targetPort, this);
        }

        /// <summary> 清空掉所有连接 </summary>
        public void ClearConnections()
        {
            while (connections.Count > 0)
            {
                Disconnect(connections[0].Port);
            }

            connections.Clear();
        }

        /// <summary> 是否与该端口连接？ </summary>
        public bool IsConnectedTo(NodePort port)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].node == port.node && connections[i].fieldName == port.fieldName)
                    return true;
            }

            return false;
        }

        /// <summary> 接口方向(进/出) </summary>
        public enum PortDirection
        {
            /// <summary> 进方向 </summary>
            Input,

            /// <summary> 出方向 </summary>
            Output,

            /// <summary> 双方向 </summary>
            Both
        }

        /// <summary> 接口连接数量限制 </summary>
        public enum PortCapacity
        {
            /// <summary> 可以连接多个接口 </summary>
            Multi,

            /// <summary> 只能连接一个接口 </summary>
            Single
        }

        /// <summary> 接口连接类型限制 </summary>
        public enum PortTypeConstraint
        {
            /// <summary> 允许所有类型的连接 </summary>
            None,

            /// <summary> 同类型和子类可连接 </summary>
            Inherited,

            /// <summary> 仅同类型可连接 </summary>
            Strict,
        }
    }
}