using System;

namespace CZFramework.CZNode.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CustomNodeAttribute : Attribute, ICustomEditorAttribute
    {
        public readonly Type TargetType;

        public CustomNodeAttribute(Type targetType)
        {
            this.TargetType = targetType;
        }

        public Type GetInspectedType()
        {
            return TargetType;
        }
    }

    public interface ICustomEditorAttribute
    {
        Type GetInspectedType();
    }
}