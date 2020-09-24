using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace CZFramework.CZNode.Editor
{
    public class BaseNode : Node
    {
        /// <summary> Key is NodeDataType, Value is NodeType </summary>
        private static Dictionary<Type, Type> _nodeTypeCache;

        public Rect targetPosition;

        public static Type GetNodeType(Type nodeType, Type fallback)
        {
            if (_nodeTypeCache == null)
            {
                _nodeTypeCache = new Dictionary<Type, Type>();
                Type[] nodeTypes = BaseGraphEditorWindow.GetDerivedTypes(typeof(BaseNode));
                foreach (Type tempType in nodeTypes)
                {
                    var attribs = tempType.GetCustomAttributes(typeof(CustomNodeAttribute), false);
                    if (attribs == null || attribs.Length == 0) continue;
                    CustomNodeAttribute attrib = attribs[0] as CustomNodeAttribute;
                    _nodeTypeCache.Add(attrib.TargetType, tempType);

                }
            }

            if (_nodeTypeCache.TryGetValue(nodeType, out Type type))
                return type;
            return fallback;
        }

        #region 静态常量

        /// <summary> 默认节点大小 </summary>
        public static readonly Vector2 DefaultNodeSize = new Vector2(120, 200);

        /// <summary> MainContainer背景色 </summary>
        public static readonly Color DefaultMainContainerColor = new Color(0.35f, 0.35f, 0.35f);

        #endregion

        /// <summary> Graph View </summary>
        protected BaseGraphView GraphView;
        protected SerializedObject SerializedObject;
        protected readonly Dictionary<string, BasePort> PortDic = new Dictionary<string, BasePort>();

        /// <summary> 用户节点数据 </summary>
        public NodeData NodeData { get; private set; }

        public void SetGraphView(BaseGraphView graphView)
        {
            this.GraphView = graphView;
        }

        public virtual void Init(NodeData nodeData)
        {
            this.NodeData = nodeData;
            SerializedObject = new SerializedObject(nodeData);
            SetPosition(new Rect(nodeData.Position, DefaultNodeSize));
            mainContainer.style.backgroundColor = DefaultMainContainerColor;

            Type nodeDataType = nodeData.GetType();
            if (AttributeCache.TryGetTypeAttribute(nodeDataType, out NodeTooltipAttribute tooltipAttribute))
            {
                tooltip = tooltipAttribute.Tooltip;
            }

            if (AttributeCache.TryGetTypeAttribute(nodeData.GetType(), out TitleAttribute titleAttribute))
            {
                if (titleAttribute.Title != null && titleAttribute.Title.Length > 0)
                    title = titleAttribute.Title[titleAttribute.Title.Length - 1];
                else
                    title = ObjectNames.NicifyVariableName(nodeData.GetType().Name);
            }
            else
                title = ObjectNames.NicifyVariableName(nodeData.GetType().Name);

            if (AttributeCache.TryGetTypeAttribute(nodeData.GetType(),
                out NodeTitleTintAttribute nodeTitleTintAttribute))
            {
                titleContainer.style.backgroundColor = nodeTitleTintAttribute.Color;
                titleContainer.Q("title-label").style.color = new Color(1 - nodeTitleTintAttribute.Color.r,
                    1 - nodeTitleTintAttribute.Color.g, 1 - nodeTitleTintAttribute.Color.b);
            }

            if (AttributeCache.TryGetTypeAttribute(nodeData.GetType(), out NodeWidthCoefficientAttribute nodeWidth))
                style.maxWidth = style.minWidth = nodeWidth.WidthCoefficient * 12;
            else
                style.maxWidth = style.minWidth = NodeWidthCoefficientAttribute.DefaultWidthCoefficient * 12;

            OnNodeCreated();
        }

        protected virtual void OnNodeCreated()
        {
            GeneratePort();
            AddIMGUIContainer();
        }

        /// <summary> 生成接口 </summary>
        protected virtual void GeneratePort()
        {
            SerializedProperty iterator = SerializedObject.GetIterator();
            iterator.NextVisible(true);
            do
            {
                if (_ignoreFields.Contains(iterator.name))
                    continue;

                if (AttributeCache.TryGetFieldAttribute(NodeData.GetType(), iterator.name,
                    out PortAttribute portAttribute))
                {
                    Port.Capacity capacity = portAttribute.Capacity == NodePort.PortCapacity.Single
                        ? Port.Capacity.Single
                        : Port.Capacity.Multi;
                    if (portAttribute.Direction != NodePort.PortDirection.Both)
                    {
                        Direction direction = portAttribute.Direction == NodePort.PortDirection.Input
                            ? Direction.Input
                            : Direction.Output;
                        NodePort nodePort = NodeData.GetPort(iterator.name);
                        BasePort port =
                            InstantiatePort(Orientation.Horizontal, direction, capacity, nodePort.DataType) as BasePort;
                        port.userData = nodePort;
                        port.name = iterator.name;
                        if (!string.IsNullOrEmpty(portAttribute.PortName))
                            port.portName = portAttribute.PortName;
                        else
                            port.portName = iterator.displayName;

                        if (AttributeCache.TryGetFieldAttribute(NodeData.GetType(), iterator.name,
                            out NodeTooltipAttribute tooltipAttribute))
                            port.tooltip = tooltipAttribute.Tooltip;
                        
                        PortDic[port.name] = port;

                        switch (port.direction)
                        {
                            case Direction.Input:
                                inputContainer.Add(port);
                                break;
                            case Direction.Output:
                                outputContainer.Add(port);
                                port.onConnect += (Edge edge) =>
                                {
                                    if (GraphView.Inited)
                                    {
                                        (edge.output.userData as NodePort).Connect(edge.input.userData as NodePort);
                                        EditorUtility.SetDirty(NodeData);
                                    }
                                };
                                port.onDisconnect += (Edge edge) =>
                                {
                                    (edge.output.userData as NodePort).Disconnect(edge.input.userData as NodePort);
                                    EditorUtility.SetDirty(NodeData);
                                };
                                break;
                        }
                    }
                    else
                    {
                        NodePort nodePortIn =
                            NodeData.GetPort(NodePort.GetBothName(iterator.name, NodePort.PortDirection.Input));
                        GenerateBothPort(iterator, portAttribute, nodePortIn, Direction.Input);

                        NodePort nodePortOut =
                            NodeData.GetPort(NodePort.GetBothName(iterator.name, NodePort.PortDirection.Output));
                        GenerateBothPort(iterator, portAttribute, nodePortOut, Direction.Output);
                    }
                }
            } while (iterator.NextVisible(false));

            RefreshPorts();
        }

        /// <summary> 绘制双向接口 </summary>
        protected void GenerateBothPort(SerializedProperty iterator, PortAttribute attribute, NodePort nodePort,
            Direction direction)
        {
            Port.Capacity capacity = attribute.Capacity == NodePort.PortCapacity.Single
                ? Port.Capacity.Single
                : Port.Capacity.Multi;
            BasePort port = InstantiatePort(Orientation.Horizontal, direction, capacity, nodePort.DataType) as BasePort;
            port.userData = nodePort;
            port.name = NodePort.GetBothName(iterator.name, nodePort.Direction);
            if (!string.IsNullOrEmpty(attribute.PortName))
                port.portName = attribute.PortName;
            else
                port.portName = iterator.displayName;

            if (AttributeCache.TryGetFieldAttribute(NodeData.GetType(), iterator.name,
                out NodeTooltipAttribute tooltipAttribute))
                port.tooltip = tooltipAttribute.Tooltip;
            PortDic[port.name] = port;

            switch (port.direction)
            {
                case Direction.Input:
                    inputContainer.Add(port);
                    break;
                case Direction.Output:
                    outputContainer.Add(port);
                    port.onConnect += (Edge edge) =>
                    {
                        if (GraphView.Inited)
                        {
                            (edge.output.userData as NodePort).Connect(edge.input.userData as NodePort);
                            EditorUtility.SetDirty(NodeData);
                        }
                    };
                    port.onDisconnect += (Edge edge) =>
                    {
                        (edge.output.userData as NodePort).Disconnect(edge.input.userData as NodePort);
                        EditorUtility.SetDirty(NodeData);
                    };
                    break;
            }
        }

        /// <summary>
        /// 忽略绘制的字段名
        /// </summary>
        public static HashSet<string> _ignoreFields = new HashSet<string>() {"m_Script", "position", "graph", "ports"};

        /// <summary>
        /// 增加IMGUI形式的body绘制
        /// 可通过重写取消此方式
        /// </summary>
        protected virtual void AddIMGUIContainer()
        {
            IMGUIContainer imguiContainer = new IMGUIContainer(OnBodyGUI);
            mainContainer.Add(imguiContainer);
        }

        public virtual void OnBodyGUI()
        {
            EditorGUIUtility.labelWidth = 64;
            EditorGUI.BeginChangeCheck();
            SerializedProperty iterator = SerializedObject.GetIterator();
            iterator.NextVisible(true);
            do
            {
                if (_ignoreFields.Contains(iterator.name))
                    continue;
                if (AttributeCache.TryGetFieldAttribute(NodeData.GetType(), iterator.name,
                    out PortAttribute portAttribute))
                {
                    switch (portAttribute.ShowBackValue)
                    {
                        case PortAttribute.ShowBackingValue.Always:
                            EditorGUILayout.PropertyField(iterator);
                            break;
                        case PortAttribute.ShowBackingValue.Unconnected:
                            if (!PortDic[iterator.name].connected)
                                EditorGUILayout.PropertyField(iterator);
                            break;
                    }
                }
                else
                    EditorGUILayout.PropertyField(iterator);
            } while (iterator.NextVisible(false));

            if (EditorGUI.EndChangeCheck())
                SerializedObject.ApplyModifiedProperties();
            SerializedObject.Update();
        }

        /// <summary> 设置坐标，网格停靠 </summary>
        public override void SetPosition(Rect newPos)
        {
            targetPosition = newPos;
            newPos.x = newPos.x - newPos.x / newPos.x * newPos.x % 12f;
            newPos.y = newPos.y - newPos.y / newPos.y * newPos.y % 12f;
            base.SetPosition(newPos);
            if (!GraphView.Inited)
                return;
            if (NodeData.Position != newPos.position)
            {
                NodeData.Position = newPos.position;
                EditorUtility.SetDirty(NodeData);
            }
        }

        /// <summary> 设置坐标，无网格停靠 </summary>
        public virtual void SetPositionNoneGrid(Rect newPos)
        {
            targetPosition = newPos;
            base.SetPosition(newPos);
            if (!GraphView.Inited)
                return;
            if (NodeData.Position != newPos.position)
            {
                NodeData.Position = newPos.position;
                EditorUtility.SetDirty(NodeData);
            }
        }

        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity,
            Type type)
        {
            return BasePort.Create<BaseEdge>(orientation, direction, capacity, type);
        }

        public bool IsDeletable()
        {
            return capabilities.HasFlag(Capabilities.Deletable);
        }

        public void SetDeletable(bool deletable)
        {
            if (deletable)
                capabilities |= Capabilities.Deletable;
            else
                capabilities &= ~Capabilities.Deletable;
        }

        public void SetMovable(bool movable)
        {
            if (movable)
                capabilities |= Capabilities.Movable;
            else
                capabilities &= ~Capabilities.Movable;
        }

        public void SetRenamable(bool renamable)
        {
            if (renamable)
                capabilities |= Capabilities.Renamable;
            else
                capabilities &= ~Capabilities.Renamable;
        }

        public void SetSelectable(bool selectable)
        {
            if (selectable)
                capabilities |= Capabilities.Selectable;
            else
                capabilities &= ~Capabilities.Selectable;
        }

        public virtual void OnRemoved()
        {
            
        }
    }
}