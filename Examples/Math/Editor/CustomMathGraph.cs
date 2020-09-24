using System;
using System.Collections.Generic;
using CZFramework.CZNode.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZFramework.CZNode.Example.Math.Editor
{
    public class CustomMathGraph : BaseGraphView
    {
        public override void Init(GraphData graphData)
        {
            base.Init(graphData);
            Label label = new Label("Math Example");
            label.style.fontSize = 30;
            label.style.width = 0;
            label.style.color = new StyleColor(Color.magenta);
            Add(label);
        }

        public override NodeSearchWindow BuildSearchWindow()
        {
            SearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            List<Type> nodeTypes = ChildrenTypeCache.GetChildrenTypes<NormalNode>();
            nodeTypes.Add(typeof(DisplayValueNode));
            SearchWindow.Init(this, nodeTypes);
            return SearchWindow;
        }
    }
}