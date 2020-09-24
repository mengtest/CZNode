using System;

namespace CZFramework.CZNode
{
    /// <summary> 接口路径特性，通过此特性设置接口在节点列表中的路径 </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TitleAttribute : Attribute
    {
        /// <summary> 最后一个将作为节点的名字 </summary>
        public readonly string[] Title;

        public bool ShowInList = true;

        public TitleAttribute(params string[] title)
        {
            Title = title;

#if UNITY_EDITOR
            for (int i = 0; i < title.Length; i++)
            {
                title[i] = UnityEditor.ObjectNames.NicifyVariableName(title[i]);
            }
#endif
        }
    }
}
