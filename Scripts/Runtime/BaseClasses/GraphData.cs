using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;

#endif

namespace CZFramework.CZNode
{
    [Serializable]
    public abstract class GraphData : ScriptableObject
    {
        /// <summary> 所有的节点 <para/>
        /// See: <see cref="AddNode{T}"/>  </summary>
        [SerializeField] public List<NodeData> nodes = new List<NodeData>();

#if UNITY_EDITOR
        public List<GroupData> groups = new List<GroupData>();
#endif

        protected virtual void Awake()
        {
#if UNITY_EDITOR
            string ourAssetPath = AssetDatabase.GetAssetPath(this);
            string fileName = Path.GetFileNameWithoutExtension(ourAssetPath);
            name = fileName;
#endif
        }

        public T AddNode<T>() where T : NodeData
        {
            return AddNode(typeof(T)) as T;
        }

        /// <summary> 根据类型添加一个节点 </summary>
        public virtual NodeData AddNode(Type type)
        {
            NodeData node = CreateInstance(type) as NodeData;
            node.graph = this;
            nodes.Add(node);
            return node;
        }

        /// <summary> 复制节点 </summary>
        public virtual NodeData CopyNode(NodeData original)
        {
            NodeData node = Instantiate(original);
            node.graph = this;
            node.ClearConnections();
            nodes.Add(node);
            return node;
        }

        /// <summary> 移除节点和连接 </summary>
        public void RemoveNode(NodeData node)
        {
            node.ClearConnections();
            nodes.Remove(node);

#if UNITY_EDITOR
            foreach (GroupData groupData in groups)
            {
                if (groupData.nodes.Contains(node))
                    groupData.nodes.Remove(node);
            }

            groups.RemoveAll(a => a.nodes.Count == 0);
#endif

            if (Application.isPlaying) Destroy(node);
        }

        /// <summary> 删除所有节点和连接 </summary>
        public virtual void Clear()
        {
            if (Application.isPlaying)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    Destroy(nodes[i]);
                }
            }

            nodes.Clear();
        }

        /// <summary> 深拷贝 </summary>
        public virtual GraphData Copy()
        {
            // Instantiate a new nodegraph instance
            GraphData graph = Instantiate(this);
            graph.name = this.name;
            // Instantiate all nodes inside the graph
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null) continue;
                NodeData node = Instantiate(nodes[i]) as NodeData;
                node.graph = graph;
                graph.nodes[i] = node;
            }

            // Redirect all connections
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                if (graph.nodes[i] == null) continue;
                foreach (NodePort port in graph.nodes[i].Ports)
                {
                    port.Redirect(nodes, graph.nodes);
                }
            }

            graph.OnCopy();
            foreach (NodeData nodeData in graph.nodes)
            {
                nodeData.OnCopy();
            }

            return graph;
        }

        public virtual void OnCopy()
        {
        }

        protected virtual void OnDestroy()
        {
            Clear();
        }

#if UNITY_EDITOR
        public void AddGroup(GroupData groupData)
        {
            foreach (GroupData tempGroup in groups)
            {
                tempGroup.nodes.RemoveAll(a => groupData.nodes.Contains(a));
            }
            groups.Add(groupData);
        }

        public void RemoveGroup(GroupData groupData)
        {
            groups.Remove(groupData);
        }
#endif
    }
}