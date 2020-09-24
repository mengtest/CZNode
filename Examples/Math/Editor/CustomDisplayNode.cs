using CZFramework.CZNode.Editor;
using UnityEngine;

namespace CZFramework.CZNode.Example.Math.Editor
{
    [CustomNode(typeof(DisplayValueNode))]
    public class CustomDisplayNode : BaseNode
    {
        private DisplayValueNode node;

        public override void Init(NodeData nodeData)
        {
            base.Init(nodeData);
            node = nodeData as DisplayValueNode;
        }

        public override void OnBodyGUI()
        {
            base.OnBodyGUI();
            GUILayout.Label(node.GetValue(null).ToString());
        }
    }
}
