using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace CZFramework.CZNode.Editor
{
    public class BaseGraphView : GraphView
    {
        protected NodeSearchWindow SearchWindow;
        protected List<BaseNode> CopyCache = new List<BaseNode>();
        public readonly Dictionary<NodeData, BaseNode> NodeDic = new Dictionary<NodeData, BaseNode>();

        public bool Inited { get; private set; }
        public SerializedObject SerializedObject { get; private set; }
        public BaseGraphEditorWindow TargetWindow { get; private set; }
        public GraphData GraphData { get; private set; }

        public void SetWindow(BaseGraphEditorWindow targetWindow)
        {
            this.TargetWindow = targetWindow;
        }

        public virtual void Init(GraphData graphData)
        {
            GraphData = graphData;
            SerializedObject = new SerializedObject(GraphData);
            styleSheets.Add(Resources.Load<StyleSheet>("Graph"));

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            SetupZoom(NodePreference.Setting.min, NodePreference.Setting.max);

            SearchWindow = BuildSearchWindow();
            nodeCreationRequest += (context =>
            {
                UnityEditor.Experimental.GraphView.SearchWindow.Open(
                    new SearchWindowContext(context.screenMousePosition), SearchWindow);
            });

            RegisterCallbacks();

            GenerateAndLinkNodes();

            foreach (GroupData groupData in GraphData.groups)
            {
                GenerateGroup(groupData);
            }

            Inited = true;
        }

        /// <summary> Generate Nodes and Link Nodes </summary>
        protected virtual void GenerateAndLinkNodes()
        {
            Dictionary<NodeData, Dictionary<string, Port>> portDic =
                new Dictionary<NodeData, Dictionary<string, Port>>();

            foreach (var nodeData in GraphData.nodes)
            {
                if (nodeData == null)
                    continue;
                GenerateNode(nodeData, nodeData.Position);
                portDic[nodeData] = new Dictionary<string, Port>();
            }

            ports.ForEach(port =>
            {
                if (port.userData is NodePort portData) portDic[portData.Node][portData.FieldName] = port;
            });

            foreach (NodeData nodeData in GraphData.nodes)
            {
                if (nodeData == null)
                    continue;
                foreach (NodePort portData in nodeData.Ports)
                {
                    if (portData.Direction == NodePort.PortDirection.Output)
                    {
                        foreach (NodePort connect in portData.GetConnections())
                        {
                            if (connect.Node == null || string.IsNullOrEmpty(connect.FieldName))
                                continue;
                            Edge tempEdge = new Edge();
                            if (portDic[nodeData].TryGetValue(portData.FieldName, out Port outputPort))
                                tempEdge.output = outputPort;
                            if (portDic[connect.Node].TryGetValue(connect.FieldName, out Port inputPort))
                                tempEdge.input = inputPort;
                            if (tempEdge.output == null || tempEdge.input == null)
                                continue;

                            tempEdge.input = portDic[connect.Node][connect.FieldName];
                            tempEdge.input.Connect(tempEdge);
                            tempEdge.output.Connect(tempEdge);
                            Add(tempEdge);
                        }
                    }
                }
            }
        }

        /// <summary> 构建右键菜单 </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 v = this.LocalToWorld(evt.mousePosition) + TargetWindow.position.position;
            evt.menu.AppendAction("Craete Node", obj =>
            {
                UnityEditor.Experimental.GraphView.SearchWindow.Open(
                       new SearchWindowContext(v), SearchWindow);
            });
            if (selection.OfType<BaseNode>().Any())
            {
                evt.menu.AppendAction("Group", AddGroup);
                evt.menu.AppendAction("Delete", obj =>
                {
                    DeleteSelection();
                });
            }
            evt.menu.AppendAction("Preferences", obj =>
            {
                NodePreference.OpenPreferences();
            });
        }
        
        public void AddGroup(DropdownMenuAction obj)
        {
            GroupData groupData = new GroupData();
            foreach (BaseNode nodeView in selection.OfType<BaseNode>())
            {
                groupData.nodes.Add(nodeView.NodeData);
            }
            groupData.Init();
            GraphData.AddGroup(groupData);
            GenerateGroup(groupData);
            EditorUtility.SetDirty(GraphData);
        }

        private void GenerateGroup(GroupData groupData)
        {
            BaseGroup group = new BaseGroup(groupData, this);

            foreach (NodeData nodeData in groupData.nodes)
            {
                group.AddElement(NodeDic[nodeData]);
            }
            
            AddElement(group);
        }

        // 构建节点列表窗口
        public virtual NodeSearchWindow BuildSearchWindow()
        {
            NodeSearchWindow searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            searchWindow.Init(this, GetNodeTypes());
            return searchWindow;
        }

        public virtual List<Type> GetNodeTypes()
        {
            return ChildrenTypeCache.GetChildrenTypes<NodeData>();
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            var selections = new HashSet<NodeData>();
            foreach (var t in selection.OfType<BaseNode>())
            {
                selections.Add(t.NodeData);
            }

            Selection.objects = selections.ToArray();
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            var selections = new HashSet<NodeData>();
            foreach (var t in selection.OfType<BaseNode>())
            {
                selections.Add(t.NodeData);
            }
            Selection.objects = selections.ToArray();
        }

        #region Callbacks

        protected virtual void RegisterCallbacks()
        {
            RegisterCallback(new EventCallback<AttachToPanelEvent>(OnAttachPanel));
            RegisterCallback(new EventCallback<DetachFromPanelEvent>(OnDetachPanel));
        }

        /// <summary> 当打开任意GraphData时执行 </summary>
        private void OnAttachPanel(AttachToPanelEvent evt)
        {
            panel.visualTree.RegisterCallback(new EventCallback<KeyDownEvent>(this.OnKeyDownShortcut));
        }

        /// <summary> 当关闭窗口时执行 </summary>
        private void OnDetachPanel(DetachFromPanelEvent evt)
        {
            // 取消注册按键响应事件
            panel.visualTree.RegisterCallback<KeyDownEvent>(OnKeyDownShortcut);
        }

        /// <summary> 按键响应 </summary>
        protected virtual void OnKeyDownShortcut(KeyDownEvent evt)
        {
            if (evt.ctrlKey)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.C:
                        CopyCache = CopySelection();
                        break;
                    case KeyCode.V:
                        if (CopyCache.Count != 0)
                            Paste(CopyCache);
                        break;
                    case KeyCode.D:
                        Paste(CopySelection());
                        break;
                }
            }
        }

        #endregion

        #region Node Operation 节点操作

        /// <summary> 将选中节点添加到复制缓冲区 </summary>
        protected virtual List<BaseNode> CopySelection()
        {
            return new List<BaseNode>(selection.OfType<BaseNode>());
        }

        /// <summary> 复制节点数据，返回一个新实例 </summary>
        /// <param name="original">源数据</param>
        protected virtual NodeData Copy(NodeData original)
        {
            NodeData nodeData = GraphData.CopyNode(original);
            nodeData.name = original.name;
            AssetDatabase.AddObjectToAsset(nodeData, GraphData);
            return nodeData;
        }

        /// <summary> 粘贴节点 </summary>
        protected virtual void Paste(List<BaseNode> copyCache)
        {
            if (copyCache.Count == 0)
                return;
            ClearSelection();
            foreach (var copyCacheNode in copyCache)
            {
                NodeData nodeData = Copy(copyCacheNode.NodeData);
                nodeData.Position += Vector2.one * 50;
                BaseNode node = GenerateNode(nodeData, nodeData.Position);
                AddToSelection(node);
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary> 指定节点数据类型，添加节点 </summary>
        /// <param name="dataType">节点类型</param>
        /// <param name="position">节点坐标</param>
        public virtual BaseNode AddNode(Type dataType, Vector2 position)
        {
            NodeData nodeData = GraphData.AddNode(dataType);
            nodeData.name = dataType.Name;
            nodeData.Position = position;
            AssetDatabase.AddObjectToAsset(nodeData, GraphData);
            AssetDatabase.SaveAssets();
            return GenerateNode(nodeData, position);
        }

        /// <summary> 生成节点，根据节点数据类型，生成对应的节点 </summary>
        /// <param name="nodeData">节点数据</param>
        /// <param name="position">节点坐标</param>
        protected virtual BaseNode GenerateNode(NodeData nodeData, Vector2 position)
        {
            Type nodeType = BaseNode.GetNodeType(nodeData.GetType(), GetNodeViewType());
            BaseNode node = Activator.CreateInstance(nodeType) as BaseNode;
            node.SetGraphView(this);
            node.Init(nodeData);
            NodeDic[nodeData] = node;
            AddElement(node);
            return node;
        }

        /// <summary> 移除指定节点，同时将节点数据从GraphData的子物体中移除 </summary>
        protected virtual void RemoveNode(BaseNode node)
        {
            NodeDic.Remove(node.NodeData);
            GraphData.RemoveNode(node.NodeData);
            RemoveElement(node);
            AssetDatabase.RemoveObjectFromAsset(node.NodeData);
        }

        /// <summary> 移除选中节点 </summary>
        public override EventPropagation DeleteSelection()
        {
            foreach (var node in selection.OfType<BaseNode>().ToList())
            {
                if (node.IsDeletable())
                    RemoveNode(node);
                node.OnRemoved();
            }

            foreach (BaseGroup group in selection.OfType<BaseGroup>().ToList())
            {
                foreach (BaseNode node in group.containedElements.OfType<BaseNode>().ToList())
                {
                    group.RemoveElement(node);
                    AddElement(node);
                }

                GraphData.RemoveGroup(group.GroupData);
            }

            AssetDatabase.SaveAssets();
            return base.DeleteSelection();
        }

        #endregion

        #region Help

        /// <summary> 获取兼容接口 </summary>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach((targetPort) =>
            {
                if ((startPort.userData as NodePort).IsCompatible(targetPort.userData as NodePort)
                    && (targetPort.userData as NodePort).IsCompatible(startPort.userData as NodePort))
                    compatiblePorts.Add(targetPort);
            });
            return compatiblePorts;
        }

        #endregion

        protected virtual Type GetNodeViewType()
        {
            return typeof(BaseNode);
        }
    }
}