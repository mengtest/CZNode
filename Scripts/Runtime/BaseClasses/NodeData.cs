using System;
using System.Collections.Generic;
using UnityEngine;

namespace CZFramework.CZNode
{
    /// <summary>
    /// 所有节点的基类
    /// </summary>
    [Serializable]
    public abstract class NodeData : ScriptableObject
    {
        [SerializeField] public GraphData graph;
        [SerializeField] private Vector2 position;

        /// <summary> 接口数据 </summary>
        [SerializeField] private NodePortDictionary ports = new NodePortDictionary();

        public IEnumerable<NodePort> Ports
        {
            get { foreach (NodePort port in ports.Values) yield return port; }
        }

        public virtual Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public virtual void OnCopy()
        {

        }

        protected virtual void OnEnable()
        {
            UpdateStaticPorts();
        }

        /// <summary> 更新接口. 在启用时和编译后调用. </summary>
        public void UpdateStaticPorts()
        {
            NodeDataCache.UpdatePorts(this, ports);
        }

        #region ports

        public NodePort GetInputPort(string fieldName)
        {
            NodePort port = GetPort(fieldName);
            if (port == null || port.Direction != NodePort.PortDirection.Input) return null;
            else return port;
        }

        public NodePort GetOutputPort(string fieldName)
        {
            NodePort port = GetPort(fieldName);
            if (port == null || port.Direction != NodePort.PortDirection.Output) return null;
            else return port;
        }

        public NodePort GetPort(string fieldName)
        {
            if (ports.TryGetValue(fieldName, out NodePort port)) return port;
            else return null;
        }

        public bool HasPort(string fieldName)
        {
            return ports.ContainsKey(fieldName);
        }

        #endregion

        #region Inputs/Outputs

        public T GetInputValue<T>(string fieldName, T fallback = default(T))
        {
            NodePort port = GetInputPort(fieldName);
            if (port != null && port.IsConnected) return (T)port.GetConnectValue();
            else return fallback;
        }

        public T GetOutputValue<T>(string fieldName, T fallback = default(T))
        {
            NodePort port = GetOutputPort(fieldName);
            if (port != null && port.IsConnected) return (T)port.GetConnectValue();
            else return fallback;
        }

        public T GetConnectValue<T>(string fieldName, T fallback = default(T))
        {
            NodePort port = GetPort(fieldName);
            if (port != null && port.IsConnected) return (T)port.GetConnectValue();
            else return fallback;
        }

        /// <summary> 通过input或output接口返回的值 </summary>
        public virtual object GetValue(NodePort port)
        {
            Debug.LogWarning("No GetValue(NodePort port) override defined for " + GetType());
            return null;
        }

        #endregion

        /// <summary> 当接口连接时触发 </summary>
        /// <param name="from">from为调用者本身的端口</param>
        public virtual void OnCreateConnection(NodePort from, NodePort to)
        {
        }

        /// <summary> 当接口断开时触发 </summary>
        /// <param name="from">from为调用者本身的端口</param>
        public virtual void OnRemoveConnection(NodePort from, NodePort to)
        {
        }

        /// <summary> 断开所有连接 </summary>
        public void ClearConnections()
        {
            foreach (NodePort port in ports.Values) port.ClearConnections();
        }


        [Serializable]
        private class NodePortDictionary : Dictionary<string, NodePort>, ISerializationCallbackReceiver
        {
            [SerializeField] private List<string> keys = new List<string>();
            [SerializeField] private List<NodePort> values = new List<NodePort>();

            public void OnBeforeSerialize()
            {
                keys.Clear();
                values.Clear();
                foreach (KeyValuePair<string, NodePort> pair in this)
                {
                    keys.Add(pair.Key);
                    values.Add(pair.Value);
                }
            }

            public void OnAfterDeserialize()
            {
                this.Clear();

                if (keys.Count != values.Count)
                    throw new System.Exception("there are " + keys.Count + " keys and " + values.Count + " values after deserialization. Make sure that both key and value types are serializable.");

                for (int i = 0; i < keys.Count; i++)
                    this.Add(keys[i], values[i]);
            }
        }
    }
}