using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace CZFramework.CZNode.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private BaseGraphView graph;
        private List<Type> nodeTypes;
        List<SearchTreeEntry> tree;

        public void Init(BaseGraphView graph, List<Type> nodeTypes)
        {
            this.graph = graph;
            this.nodeTypes = nodeTypes;

            this.nodeTypes.Sort((a, b) =>
            {
                int aCount = 1, bCount = 1;

                if (AttributeCache.TryGetTypeAttribute(a, out TitleAttribute aAttribute))
                    aCount = aAttribute.Title.Length;

                if (AttributeCache.TryGetTypeAttribute(b, out TitleAttribute bAttribute))
                    bCount = bAttribute.Title.Length;
                if (aCount > bCount)
                    return 1;
                if (aCount == bCount)
                    return 0;

                return -1;
            });

            tree = CreateSearchTree();
        }

        private List<SearchTreeEntry> CreateSearchTree()
        {
            List<SearchTreeEntry> tempTree = new List<SearchTreeEntry>()
                {new SearchTreeGroupEntry(new GUIContent("Create Elements"))};
            foreach (Type type in nodeTypes)
            {
                if (AttributeCache.TryGetTypeAttribute(type, out TitleAttribute attribute))
                {
                    if (attribute.ShowInList)
                    {
                        GUIContent content = new GUIContent(attribute.Title.Last());
                        //if (AttributeCache.TryGetTypeAttribute(type, out NodeTooltipAttribute tooltipAttribute))
                        //    content.tooltip = tooltipAttribute.Tooltip;
                        if (attribute.Title.Length > 1)
                        {
                            SearchTreeGroupEntry groupTemp = null;
                            for (int i = 1; i < attribute.Title.Length; i++)
                            {
                                SearchTreeGroupEntry group = tempTree.Find(item =>
                                        (item.content.text == attribute.Title[i - 1] && item.level == i)) as
                                    SearchTreeGroupEntry;
                                if (group == null)
                                {
                                    group = new SearchTreeGroupEntry(new GUIContent(attribute.Title[i - 1]), i);
                                    int index = groupTemp == null ? 0 : tempTree.IndexOf(groupTemp);
                                    tempTree.Insert(index + 1, group);
                                }

                                groupTemp = group;
                            }
                            tempTree.Insert(tempTree.IndexOf(groupTemp) + 1,
                                new SearchTreeEntry(content)
                                { userData = type, level = attribute.Title.Length });
                        }
                        else
                        {
                            tempTree.Add(new SearchTreeEntry(content)
                            { userData = type, level = 1 });
                        }
                    }
                }
                else
                {
                    GUIContent content = new GUIContent(ObjectNames.NicifyVariableName(type.Name));
                    //if (AttributeCache.TryGetTypeAttribute(type, out NodeTooltipAttribute tooltipAttribute))
                    //    content.tooltip = tooltipAttribute.Tooltip;
                    tempTree.Add(new SearchTreeEntry(content) { userData = type, level = 1 });
                }
            }

            return tempTree;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            Vector2 worldPosition = graph.TargetWindow.rootVisualElement.ChangeCoordinatesTo(
                graph.TargetWindow.rootVisualElement.parent,
                context.screenMousePosition - graph.TargetWindow.position.position);
            Vector2 localPosition = graph.contentViewContainer.WorldToLocal(worldPosition);

            graph.AddNode(searchTreeEntry.userData as Type, localPosition);
            return true;
        }
    }
}