using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CZFramework.CZNode.Editor
{
    public class BaseGraphEditorWindow : EditorWindow
    {
        /// <summary> Key is GraphDataType, Value is EditorWindowType </summary>
        private static Dictionary<Type, Type> _editorWindowDataCache;

        public static Type GetGraphEditorWindow(Type graphDataType, Type fallback)
        {
            if (_editorWindowDataCache == null)
            {
                _editorWindowDataCache = new Dictionary<Type, Type>();
                Type[] graphEditorWindowTypes = GetDerivedTypes(typeof(BaseGraphEditorWindow));
                foreach (Type graphEditorWindowType in graphEditorWindowTypes)
                {
                    var attribs = graphEditorWindowType.GetCustomAttributes(typeof(CustomGraphEditorWindowAttribute), false);
                    if (attribs == null || attribs.Length == 0) continue;
                    CustomGraphEditorWindowAttribute attrib = attribs[0] as CustomGraphEditorWindowAttribute;
                    _editorWindowDataCache.Add(attrib.TargetType, graphEditorWindowType);
                }
            }

            if (_editorWindowDataCache.TryGetValue(graphDataType, out Type type))
                return type;
            return fallback;
        }


        public static EditorWindow GetWindowPrivate(Type t, bool utility, string title, bool focus)
        {
            UnityEngine.Object[] objectsOfTypeAll = Resources.FindObjectsOfTypeAll(t);
            EditorWindow editorWindow = null;
            for (int i = 0; i < objectsOfTypeAll.Length; i++)
            {
                if (objectsOfTypeAll[i].GetType() == t)
                {
                    editorWindow = (EditorWindow)objectsOfTypeAll[i];
                    break;
                }
            }

            if (!(bool)(UnityEngine.Object)editorWindow)
            {
                editorWindow = ScriptableObject.CreateInstance(t) as EditorWindow;
                if (title != null)
                    editorWindow.titleContent = new GUIContent(title);
                if (utility)
                    editorWindow.ShowUtility();
                else
                    editorWindow.Show();
            }
            else if (focus)
            {
                editorWindow.Show();
                editorWindow.Focus();
            }
            return editorWindow;
        }

        public static Type[] GetDerivedTypes(Type baseType)
        {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray());
                }
                catch (ReflectionTypeLoadException) { }
            }
            return types.ToArray();
        }

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceId, int line)
        {
            GraphData graphData = null;
            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is GraphData)
            {
                graphData = obj as GraphData;
            }
            else if (obj is NodeData)
            {
                graphData = (obj as NodeData).graph;
                instanceId = graphData.GetInstanceID();
            }

            if (graphData != null)
            {
                if (Instance != null &&
                    GetGraphEditorWindow(graphData.GetType(), typeof(BaseGraphEditorWindow)) == Instance.GetType())
                {
                    if (Instance.GraphData != graphData)
                        Instance.OpenGraph(graphData);
                    Instance.Focus();
                }
                else
                {
                    InstanceId = instanceId;
                    Type t = GetGraphEditorWindow(graphData.GetType(), typeof(BaseGraphEditorWindow));
                    Instance = GetWindowPrivate(t, false, (string)null, true) as BaseGraphEditorWindow;
                }
                if (obj is NodeData)
                {
                    BaseNode node = Instance.GraphView.NodeDic[obj as NodeData];
                    Instance.GraphView.AddToSelection(node);
                    Instance.GraphView.FrameSelection();
                }
                return true;
            }

            return false;
        }

        public static BaseGraphEditorWindow Instance { get; private set; }
        private static int InstanceId;

        protected BaseGraphView GraphView;
        public GraphData GraphData;
        public int instanceID;

        private void Awake()
        {
            instanceID = InstanceId;
        }

        protected virtual void OnEnable()
        {
            Instance = this;
            wantsMouseMove = true;
            this.SetAntiAliasing(4);

            if (GraphData == null &&
                (GraphData = EditorUtility.InstanceIDToObject(instanceID) as GraphData) == null)
                return;
            OpenGraph(GraphData);
        }

        protected void OpenGraph(GraphData graphData)
        {
            this.GraphData = graphData;
            this.instanceID = graphData.GetInstanceID();
            titleContent = new GUIContent("CZNode");
            rootVisualElement.Add(BuildWindow());
        }

        protected virtual VisualElement BuildWindow()
        {
            // GraphView
            VisualElement visualElement = new VisualElement();

            GraphView = Activator.CreateInstance(GetGraphViewType()) as BaseGraphView;
            GraphView.SetWindow(this);
            GraphView.Init(GraphData);
            GraphView.StretchToParentSize();

            visualElement.Add(GraphView);
            visualElement.StretchToParentSize();
            return visualElement;
        }

        protected virtual Type GetGraphViewType()
        {
            return typeof(BaseGraphView);
        }
    }
}