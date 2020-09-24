using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CZFramework.CZNode
{
    public static class AttributeCache
    {
        /// <summary> 保存类的特性，在编译时重载 </summary>
        private static readonly Dictionary<Type, Attribute[]> TypeAttributes = new Dictionary<Type, Attribute[]>();

        /// <summary> 保存类的字段的特性，在编译时重载 </summary>
        private static readonly Dictionary<Type, Dictionary<string, Dictionary<Type, Attribute>>> TypeFieldAttributes =
            new Dictionary<Type, Dictionary<string, Dictionary<Type, Attribute>>>();

        /// <summary> 尝试获取目标类型的目标特性 </summary>
        public static bool TryGetTypeAttribute<AttributeType>(Type classType, out AttributeType attribute)
            where AttributeType : Attribute
        {
            if (TryGetTypeAttributes(classType, out Attribute[] attributes))
            {
                foreach (var tempAttribute in attributes)
                {
                    attribute = tempAttribute as AttributeType;
                    if (attribute != null)
                        return true;
                }
            }

            attribute = null;
            return false;
        }

        /// <summary> 尝试获取目标类型的所有特性 </summary>
        public static bool TryGetTypeAttributes(Type classType, out Attribute[] attributes)
        {
            if (TypeAttributes.TryGetValue(classType, out attributes))
                return attributes == null || attributes.Length > 0;

            attributes = classType.GetCustomAttributes() as Attribute[];
            TypeAttributes[classType] = attributes;
            return attributes == null || attributes.Length > 0;
        }

        /// <summary> 尝试获取目标类型的目标字段的目标特性 </summary>
        public static bool TryGetFieldAttribute<AttributeType>(Type classType, string fieldName,
            out AttributeType attribute)
            where AttributeType : Attribute
        {
            if (TryGetFieldAttributes(classType, fieldName, out Attribute[] attributes))
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    attribute = attributes[i] as AttributeType;
                    if (attribute != null)
                        return true;
                }
            }

            attribute = null;
            return false;
        }

        /// <summary> 尝试获取目标类型的目标字段的所有特性 </summary>
        public static bool TryGetFieldAttributes(Type classType, string fieldName,
            out Attribute[] attributes)
        {
            Dictionary<string, Dictionary<Type, Attribute>> fieldTypes;
            Dictionary<Type, Attribute> types;
            if (TypeFieldAttributes.TryGetValue(classType, out fieldTypes))
            {
                if (fieldTypes.TryGetValue(fieldName, out types))
                {
                    attributes = types.Values.ToArray();
                    if (attributes != null && attributes.Length > 0)
                        return true;
                    return false;
                }
            }
            else
                fieldTypes = new Dictionary<string, Dictionary<Type, Attribute>>();

            FieldInfo field = GetFieldInfo(classType, fieldName);
            attributes = field.GetCustomAttributes(typeof(Attribute), true) as Attribute[];
            types = new Dictionary<Type, Attribute>();
            for (int i = 0; i < attributes.Length; i++)
            {
                types[attributes[i].GetType()] = attributes[i];
            }

            fieldTypes[fieldName] = types;
            TypeFieldAttributes[classType] = fieldTypes;
            if (attributes.Length > 0)
                return true;
            return false;
        }

        

        /// <summary> 尝试获取目标类型的目标字段的目标特性 </summary>
        public static bool TryGetFieldInfoAttribute<AttributeType>(Type classType, FieldInfo fieldInfo,
            out AttributeType attribute)
            where AttributeType : Attribute
        {
            if (TryGetFieldAttributes(classType, fieldInfo.Name, out Attribute[] attributes))
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    attribute = attributes[i] as AttributeType;
                    if (attribute != null)
                        return true;
                }
            }

            attribute = null;
            return false;
        }

        /// <summary> 尝试获取目标类型的目标字段的所有特性 </summary>
        public static bool TryGetFieldInfoAttributes(Type classType, FieldInfo fieldInfo,
            out Attribute[] attributes)
        {
            Dictionary<string, Dictionary<Type, Attribute>> fieldTypes;
            Dictionary<Type, Attribute> types;
            if (TypeFieldAttributes.TryGetValue(classType, out fieldTypes))
            {
                if (fieldTypes.TryGetValue(fieldInfo.Name, out types))
                {
                    attributes = types.Values.ToArray();
                    if (attributes != null && attributes.Length > 0)
                        return true;
                    return false;
                }
            }
            else
                fieldTypes = new Dictionary<string, Dictionary<Type, Attribute>>();

            attributes = fieldInfo.GetCustomAttributes(typeof(Attribute), true) as Attribute[];
            types = new Dictionary<Type, Attribute>();
            for (int i = 0; i < attributes.Length; i++)
            {
                types[attributes[i].GetType()] = attributes[i];
            }

            fieldTypes[fieldInfo.Name] = types;
            TypeFieldAttributes[classType] = fieldTypes;
            if (attributes.Length > 0)
                return true;
            return false;
        }


        /// <summary> 获取字段，包括基类的私有字段 </summary>
        public static FieldInfo GetFieldInfo(Type type, string fieldName)
        {
            // 如果第一次没有找到，那么这个变量可能是基类的私有字段
            FieldInfo field = type.GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            // 只搜索基类的私有字段
            while (field == null && (type = type.BaseType) != null)
            {
                field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return field;
        }
    }
}