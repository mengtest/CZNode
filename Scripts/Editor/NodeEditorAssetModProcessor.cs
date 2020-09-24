using System.IO;
using UnityEditor;
using UnityEngine;

namespace CZFramework.CZNode.Editor
{
    public class NodeEditorAssetModProcessor : UnityEditor.AssetModificationProcessor
    {
        /// <summary> 改名Bug补救方案 </summary>
        static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            GraphData graphData = AssetDatabase.LoadMainAssetAtPath(sourcePath) as GraphData;
            if (graphData == null)
            {
                return AssetMoveResult.DidNotMove;
            }

            var srcDir = Path.GetDirectoryName(sourcePath);
            string dstDir = Path.GetDirectoryName(destinationPath);
            if (srcDir != dstDir)
            {
                return AssetMoveResult.DidNotMove;
            }

            string fileName = Path.GetFileNameWithoutExtension(destinationPath);
            graphData.name = fileName;

            return AssetMoveResult.DidNotMove;
        }

        /// <summary> 删除节点脚本之前自动删除节点 </summary> 
        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            // 即将被删除的资源路径
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

            // 如果不是删除脚本则返回
            if (!(obj is MonoScript)) return AssetDeleteResult.DidNotDelete;

            // 检查脚本类型，如果不是节点类则返回
            MonoScript script = obj as MonoScript;
            System.Type scriptType = script.GetClass();
            if (scriptType == null || (scriptType != typeof(NodeData) && !scriptType.IsSubclassOf(typeof(NodeData)))) return AssetDeleteResult.DidNotDelete;

            // 查找所有使用此类的ScriptableObjects
            string[] guids = AssetDatabase.FindAssets("t:" + scriptType);
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
                for (int k = 0; k < objs.Length; k++)
                {
                    NodeData node = objs[k] as NodeData;
                    if (node != null && node.GetType() == scriptType)
                    {
                        if (node != null && node.graph != null)
                        {
                            // Delete the node and notify the user
                            Debug.LogWarning(node.name + " of " + node.graph + " depended on deleted script and has been removed automatically.", node.graph);
                            node.graph.RemoveNode(node);
                            AssetDatabase.RemoveObjectFromAsset(node);
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            // 继续让unity删除应该删除的脚本
            return AssetDeleteResult.DidNotDelete;
        }

        [InitializeOnLoadMethod]
        private static void OnReloadEditor()
        {
            // Find all NodeGraph assets
            // 查找所有的Graph文件
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(GraphData));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetpath = AssetDatabase.GUIDToAssetPath(guids[i]);
                GraphData graph = AssetDatabase.LoadAssetAtPath(assetpath, typeof(GraphData)) as GraphData;
                if (graph != null)
                {
                    graph.nodes.RemoveAll(x => x == null); // 移除掉空对象
                    UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetpath);
                    // 将子对象中没添加进列表的添加进去
                    for (int u = 0; u < objs.Length; u++)
                    {
                        NodeData nodeData = objs[u] as NodeData;
                        if (!graph.nodes.Contains(nodeData)) graph.nodes.Add(nodeData);
                        if (nodeData != null)
                        {
                            nodeData.graph = graph;

                            foreach (NodePort nodePort in nodeData.Ports)
                            {
                                foreach (PortConnection connection in nodePort.connections)
                                {
                                    if (connection.node == null)
                                        connection.node = EditorUtility.InstanceIDToObject(connection.NodeInstanceID) as NodeData;
                                }
                            }

                            nodeData.UpdateStaticPorts();
                        }
                    }
                }
            }
        }
    }
}