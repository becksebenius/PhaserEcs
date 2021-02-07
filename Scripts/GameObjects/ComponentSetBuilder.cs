using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Phaser
{
    public static class ComponentSetBuilder<TComponentSet>
        where TComponentSet : struct
    {
        private struct ComponentField
        {
            public FieldInfo field;
            public bool optional;
            public Type type;
        }

        private static List<ComponentField> fields;
        private static Component[] componentBuffer;
        private static object componentSetAsObject;

        static ComponentSetBuilder()
        {
            fields = new List<ComponentField>();

            foreach (var fieldInfo in typeof(TComponentSet).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var type = fieldInfo.FieldType;

                if (!typeof(Component).IsAssignableFrom(type))
                {
                    Debug.LogError($"Invalid type in component set {typeof(TComponentSet).Name}: {fieldInfo.FieldType.Name} {fieldInfo.Name}");
                    continue;
                }

                bool isOptional = fieldInfo.GetCustomAttribute<OptionalAttribute>() != null;

                fields.Add(new ComponentField()
                {
                    field = fieldInfo,
                    optional = isOptional,
                    type = type
                });
            }

            componentBuffer = new Component[fields.Count];
            componentSetAsObject = new TComponentSet();
        }

        public static bool TryCreateComponentSet(GameObject gameObject, out TComponentSet componentSet)
        {
            for (int i = 0; i < fields.Count; ++i)
            {
                var field = fields[i];
                var component = gameObject.GetComponent(field.type);
                if (!field.optional && component == null)
                {
                    componentSet = default;
                    return false;
                }

                componentBuffer[i] = component;
            }

            UpdateComponentSetFields();
            var result = (TComponentSet)componentSetAsObject;

            ClearComponentBuffer();

            // pay an additional reflection cost to avoid having static pointers
            // to components from last run
            UpdateComponentSetFields();

            componentSet = result;
            return true;
        }

        private static void ClearComponentBuffer()
        {
            for (int i = 0; i < componentBuffer.Length; ++i)
            {
                componentBuffer[i] = null;
            }
        }

        private static void UpdateComponentSetFields()
        {
            for (int i = 0; i < fields.Count; ++i)
            {
                var field = fields[i];
                field.field.SetValue(componentSetAsObject, componentBuffer[i]);
            }
        }
    }
}