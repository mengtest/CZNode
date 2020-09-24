using System;
using CZFramework.CZNode.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZFramework.CZNode.Example.Math.Editor
{
    [CustomGraphEditorWindow(typeof(MathGraph))]
    public class CustomMathGraphEditorWindow : BaseGraphEditorWindow
    {
        protected override VisualElement BuildWindow()
        {
            titleContent = new GUIContent("Math");
            return base.BuildWindow();
        }

        protected override Type GetGraphViewType()
        {
            return typeof(CustomMathGraph);
        }
    }
}