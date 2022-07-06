using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.UIElements;

[CustomEditor(typeof(Tutorial))]
public class TutorialEditor : Editor
{
    SerializedProperty tutorialStepsProp;

    [CustomPropertyDrawer(typeof(Tutorial.TutorialStep))]
    public class TutorialStepPropertyDrawer : PropertyDrawer
    {
        SerializedProperty nextStepEventProp;
        SerializedProperty buttonNextStepProp;
        SerializedProperty nextStepObjectIdProp;
        SerializedProperty arrowTargetGameObjectProp;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            nextStepEventProp = property.FindPropertyRelative("nextStepEvent");
            buttonNextStepProp = property.FindPropertyRelative("buttonNextStep");
            nextStepObjectIdProp = property.FindPropertyRelative("nextStepObjectId");
            Tutorial.TutorialStep.NextStepEvent nextStepEvent = (Tutorial.TutorialStep.NextStepEvent)GetTargetObjectOfProperty(nextStepEventProp);
            EditorGUI.PropertyField(position, nextStepEventProp);
            if (nextStepEvent == Tutorial.TutorialStep.NextStepEvent.ButtonPress)
                EditorGUI.PropertyField(new Rect(position.x,position.y+200f,position.width,position.height),buttonNextStepProp);
            else
                EditorGUI.PropertyField(new Rect(position.x, position.y + 200f, position.width, position.height),nextStepObjectIdProp);

            EditorGUI.EndProperty();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        tutorialStepsProp = serializedObject.FindProperty("tutorialSteps");
        EditorGUILayout.PropertyField(tutorialStepsProp);
        serializedObject.ApplyModifiedProperties();
    }


    /// <summary>
    /// Gets the object the property represents.
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    public static object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        if (prop == null) return null;

        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }
        }
        return obj;
    }
    private static object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }
    private static object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();
        //while (index-- >= 0)
        //    enm.MoveNext();
        //return enm.Current;

        for (int i = 0; i <= index; i++)
        {
            if (!enm.MoveNext()) return null;
        }
        return enm.Current;
    }
}

