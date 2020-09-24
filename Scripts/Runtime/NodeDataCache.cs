using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CZFramework.CZNode
{
    /// <summary> 节点端口数据缓存 </summary>
    public static class NodeDataCache
    {
        private static PortDataCache _portDataCache;

        private static bool Initialized
        {
            get { return _portDataCache != null; }
        }

        /// <summary> 更新端口 </summary>
        public static void UpdatePorts(NodeData node, Dictionary<string, NodePort> ports)
        {
            if (!Initialized) BuildCache();

            Type nodeType = node.GetType();

            Dictionary<string, NodePort> staticPorts = new Dictionary<string, NodePort>();
            if (_portDataCache.TryGetValue(nodeType, out List<NodePort> typePortCache))
            {
                foreach (var nodePort in typePortCache)
                {
                    staticPorts[nodePort.FieldName] = nodePort;
                }
            }

            // 清理端口，移除不存在的端口
            // 通过遍历当前节点的接口实现
            foreach (NodePort port in ports.Values.ToList())
            {
                if (staticPorts.TryGetValue(port.FieldName, out NodePort cachePort))
                {
                    // 如果端口特性发生了更改，则把端口清理掉
                    if (port.DataType != cachePort.DataType || port.Direction != cachePort.Direction || port.Capacity != cachePort.Capacity || port.TypeConstraint != cachePort.TypeConstraint)
                    {
                        port.Reload(cachePort);
                        foreach (var item in port.GetConnections())
                        {
                            if (!port.IsCompatible(item))
                            {
                                port.Disconnect(item);
                            }
                        }
                    }
                    else
                    {
                        port.DataType = cachePort.DataType;
                    }
                    //// 如果端口特性发生了更改，则把端口清理掉
                    //if (port.DataType != cachePort.DataType || port.Direction != cachePort.Direction || port.Capacity != cachePort.Capacity || port.TypeConstraint != cachePort.TypeConstraint)
                    //{
                    //    port.ClearConnections();
                    //    ports.Remove(port.FieldName);
                    //}
                    //else
                    //    port.DataType = cachePort.DataType;
                }
                else
                {
                    // 如果端口特性已被移除，则把端口清理掉
                    port.ClearConnections();
                    ports.Remove(port.FieldName);
                }
            }

            // 添加缺失的接口
            foreach (NodePort staticPort in staticPorts.Values)
            {
                if (!ports.ContainsKey(staticPort.FieldName))
                {
                    ports[staticPort.FieldName] = new NodePort(staticPort, node);
                }
            }
        }

        private static void BuildCache()
        {
            _portDataCache = new PortDataCache();
            List<Type> nodeTypes = ChildrenTypeCache.GetChildrenTypes<NodeData>();
            foreach (var nodeType in nodeTypes)
            {
                CachePorts(nodeType);
            }
        }

        public static List<FieldInfo> GetNodeFields(Type nodeType)
        {
            List<FieldInfo> fieldInfos =
                new List<FieldInfo>(
                    nodeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));

            // 获取类包含的所有字段(包含私有)
            Type tempType = nodeType;
            while ((tempType = tempType.BaseType) != typeof(NodeData) && tempType != null)
            {
                fieldInfos.AddRange(tempType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
            }

            return fieldInfos;
        }

        private static void CachePorts(Type nodeType)
        {
            List<FieldInfo> fieldInfos = GetNodeFields(nodeType);

            foreach (var fieldInfo in fieldInfos)
            {
                // 获取接口特性
                if (!AttributeCache.TryGetFieldAttribute(nodeType, fieldInfo.Name, out PortAttribute portAttribute))
                {
                    continue;
                }

                if (!_portDataCache.ContainsKey(nodeType)) _portDataCache.Add(nodeType, new List<NodePort>());

                if (portAttribute.Direction != NodePort.PortDirection.Both)
                {
                    _portDataCache[nodeType].Add(new NodePort(fieldInfo));
                }
                else
                {
                    _portDataCache[nodeType].Add(new NodePort(fieldInfo, NodePort.GetBothName(fieldInfo.Name, NodePort.PortDirection.Input), NodePort.PortDirection.Input));
                    _portDataCache[nodeType].Add(new NodePort(fieldInfo, NodePort.GetBothName(fieldInfo.Name, NodePort.PortDirection.Output), NodePort.PortDirection.Output));
                }
            }
        }

        [Serializable]
        private class PortDataCache : Dictionary<Type, List<NodePort>>, ISerializationCallbackReceiver
        {
            [SerializeField] private List<Type> keys = new List<Type>();
            [SerializeField] private List<List<NodePort>> values = new List<List<NodePort>>();

            // 字典保存至List
            public void OnBeforeSerialize()
            {
                keys.Clear();
                values.Clear();
                foreach (var pair in this)
                {
                    keys.Add(pair.Key);
                    values.Add(pair.Value);
                }
            }

            // 加载列表至字典
            public void OnAfterDeserialize()
            {
                this.Clear();

                if (keys.Count != values.Count)
                    throw new Exception(
                        $"there are {keys.Count.ToString()} keys and {values.Count.ToString()} values after deserialization. Make sure that both key and value types are serializable.");

                for (int i = 0; i < keys.Count; i++)
                    Add(keys[i], values[i]);
            }
        }
    }
}