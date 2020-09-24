using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CZFramework.CZNode.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CustomGraphEditorWindowAttribute : Attribute
    {
        /// <summary> Target GraphDataType </summary>
        public readonly Type TargetType;

        public CustomGraphEditorWindowAttribute(Type targetType)
        {
            TargetType = targetType;
        }
    }
}