using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(TargetingStrategy), true)]
public class TargetingStrategyDrawer : PropertyDrawer {
    static Dictionary<string, Type> typeMap;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (typeMap == null)
            BuildTypeMap();

        EditorGUI.BeginProperty(position, label, property);

        // Label handling
        Rect fieldRect = EditorGUI.PrefixLabel(position, label);

        Rect typeRect = new Rect(
            fieldRect.x,
            fieldRect.y,
            fieldRect.width,
            EditorGUIUtility.singleLineHeight
        );

        Rect contentRect = new Rect(
            position.x,
            position.y + EditorGUIUtility.singleLineHeight + 2,
            position.width,
            fieldRect.height - EditorGUIUtility.singleLineHeight - 2
        );

        string typeName = property.managedReferenceFullTypename;
        string displayName = GetShortTypeName(typeName) ?? "Select Targeting Strategy";

        if (EditorGUI.DropdownButton(typeRect, new GUIContent(displayName), FocusType.Keyboard)) {
            GenericMenu menu = new GenericMenu();

            if (typeMap.Count == 0) {
                menu.AddDisabledItem(new GUIContent("No Targeting Strategies Found"));
            }
            else {
                foreach (var kvp in typeMap) {
                    var name = kvp.Key;
                    var type = kvp.Value;

                    menu.AddItem(
                        new GUIContent(name),
                        type.FullName == typeName,
                        () => {
                            property.managedReferenceValue = Activator.CreateInstance(type);
                            property.serializedObject.ApplyModifiedProperties();
                        });
                }
            }

            menu.ShowAsContext();
        }

        if (property.managedReferenceValue != null) {
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.PropertyField(contentRect, property, GUIContent.none, true);
            EditorGUI.indentLevel = oldIndent;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        float height = EditorGUIUtility.singleLineHeight;

        if (property.managedReferenceValue != null) {
            height += EditorGUI.GetPropertyHeight(property, true) + 2;
        }

        return height;
    }

    static void BuildTypeMap() {
        var baseType = typeof(TargetingStrategy);

        typeMap = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => {
                try { return asm.GetTypes(); }
                catch { return Type.EmptyTypes; }
            })
            .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t))
            .ToDictionary(
                t => ObjectNames.NicifyVariableName(t.Name),
                t => t);
    }

    static string GetShortTypeName(string fullTypeName) {
        if (string.IsNullOrEmpty(fullTypeName))
            return null;

        var parts = fullTypeName.Split(' ');
        return parts.Length > 1 ? parts[1].Split('.').Last() : fullTypeName;
    }
}