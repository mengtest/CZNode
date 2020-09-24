using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZFramework.CZNode.Editor
{
    public class BaseGroup : Group
    {
        public GroupData GroupData { get; private set; }
        private BaseGraphView GraphView;

        public BaseGroup(GroupData groupData, BaseGraphView graphView)
        {
            GroupData = groupData;
            GraphView = graphView;

            title = groupData.groupName;
            headerContainer.Q("titleLabel").style.color = new StyleColor(Color.white);
            headerContainer.style.backgroundColor = new StyleColor(new Color(0f, 0.09f, 1f, 0.22f));
            headerContainer.style.backgroundColor = groupData.headerColor;
            style.backgroundColor =
                new StyleColor(new Color(style.color.value.r, style.color.value.g, style.color.value.b, 0.4f));
        }

        public override void SetPosition(Rect newPos)
        {
            Vector2 delta = newPos.position - GetPosition().position;
            foreach (BaseNode node in containedElements.OfType<BaseNode>())
            {
                newPos = node.GetPosition();
                newPos.position += delta;
                node.SetPositionNoneGrid(newPos);
            }
        }

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(newName) || oldName == newName)
            {
                title = oldName;
                return;
            }

            base.OnGroupRenamed(oldName, newName);
            GroupData.groupName = newName;
            EditorUtility.SetDirty(GraphView.GraphData);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            BringToFront();
            foreach (GraphElement element in containedElements)
            {
                element.BringToFront();
            }
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            base.OnElementsAdded(elements);
            if (!GraphView.Inited)
                return;
            foreach (BaseNode node in elements.OfType<BaseNode>())
            {
                if (!GroupData.nodes.Contains(node.NodeData))
                    GroupData.nodes.Add(node.NodeData);
            }
            EditorUtility.SetDirty(GraphView.GraphData);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            base.OnElementsRemoved(elements);
            if (!GraphView.Inited)
                return;
            foreach (BaseNode node in elements.OfType<BaseNode>())
            {
                if (GroupData.nodes.Contains(node.NodeData))
                    GroupData.nodes.Remove(node.NodeData);
            }

            if (GroupData.nodes.Count == 0)
            {
                GraphView.GraphData.RemoveGroup(GroupData);
                GraphView.RemoveElement(this);
            }

            EditorUtility.SetDirty(GraphView.GraphData);
        }
    }
}