using System;
using UnityEngine;

namespace CZFramework.CZNode
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeTitleTintAttribute : Attribute
    {
        public readonly Color Color;

        public NodeTitleTintAttribute(float r, float g, float b, float a = 1f)
        {
            Color = new Color(r, g, b, a);
        }
    }
}
