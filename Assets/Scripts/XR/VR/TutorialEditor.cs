#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.UIElements;
using System.Linq;

[CustomPropertyDrawer(typeof(Tutorial.TutorialStep))]
public class TutorialStepPropertyDrawer : PropertyDrawer
{
    private float lastPropertyPosition = 0;
    private float padding = 5;
    SerializedProperty nextStepEventProp;
    SerializedProperty buttonNextStepProp;
    SerializedProperty nextStepObjectHierarchyNameProp;
    SerializedProperty arrowTargetGameObjectProp;
    SerializedProperty textProp;
    SerializedProperty arrowTargetHierarchyNameProp;
    SerializedProperty teleportBoolProp;
    SerializedProperty teleportPositionProp;
    SerializedProperty arrowTargetTypeProp;
    SerializedProperty stepObjectsHiddenProp;
    SerializedProperty stepObjectsShownProp;
    SerializedProperty stepSApiObjectsInstantiatedProp;



    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUIStyle boldStyle = new GUIStyle
        {
            fontStyle = FontStyle.Bold,
            fontSize = 13
        };

        boldStyle.normal.textColor = Color.white;
        EditorGUI.BeginProperty(position, label, property);


        EditorGUI.indentLevel = 0;
        lastPropertyPosition = 20;
        //bool foldout = true;
        //foldout = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, padding + EditorGUIUtility.singleLineHeight), foldout, "test");
        nextStepEventProp = property.FindPropertyRelative("nextStepEvent");
        buttonNextStepProp = property.FindPropertyRelative("buttonNextStep");
        nextStepObjectHierarchyNameProp = property.FindPropertyRelative("nextStepObjectHierarchyName");
        arrowTargetGameObjectProp = property.FindPropertyRelative("arrowTargetGameObject");
        textProp = property.FindPropertyRelative("text");
        arrowTargetHierarchyNameProp = property.FindPropertyRelative("arrowTargetHierarchyName");
        teleportBoolProp = property.FindPropertyRelative("teleportPlayer");
        teleportPositionProp = property.FindPropertyRelative("teleportPosition");
        arrowTargetTypeProp = property.FindPropertyRelative("arrowTargetType");

        stepObjectsHiddenProp = property.FindPropertyRelative("stepObjectsHidden");
        stepObjectsShownProp = property.FindPropertyRelative("stepObjectsShown");
        stepSApiObjectsInstantiatedProp = property.FindPropertyRelative("stepSApiObjectsInstantiated");

        PlaceProperty(textProp, position);

        string text = (string)GetTargetObjectOfProperty(textProp);
        if (text.Length > 60)
        {
            label.text = text.Substring(0, 60);
            int i = 60;
            while (text[i] != ' ' && i < text.Length)
            {
                label.text += text[i];
                i++;
            }
            label.text += "...";
        }
        else
            label.text = text;

        EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label, boldStyle);
        PlaceProperty(arrowTargetTypeProp, position);

        if ((Tutorial.TutorialStep.ArrowTargetType)GetTargetObjectOfProperty(arrowTargetTypeProp) == Tutorial.TutorialStep.ArrowTargetType.GameObject)
        {
            SetTargetObjectOfProperty(arrowTargetHierarchyNameProp, null);
            PlaceProperty(arrowTargetGameObjectProp, position);
        }
        else
        {
            SetTargetObjectOfProperty(arrowTargetGameObjectProp, null);
            PlaceProperty(arrowTargetHierarchyNameProp, position);
        }

        PlaceProperty(teleportBoolProp, position);

        if ((bool)GetTargetObjectOfProperty(teleportBoolProp))
            PlaceProperty(teleportPositionProp, position);
        else
            DrawBlank(teleportBoolProp, position);


        PlaceProperty(nextStepEventProp, position);

        if ((Tutorial.TutorialStep.NextStepEvent)GetTargetObjectOfProperty(nextStepEventProp) == Tutorial.TutorialStep.NextStepEvent.ButtonPress)
        {
            SetTargetObjectOfProperty(nextStepObjectHierarchyNameProp, null);
            PlaceProperty(buttonNextStepProp, position);
        }
        else
        {
            SetTargetObjectOfProperty(buttonNextStepProp, null);
            PlaceProperty(nextStepObjectHierarchyNameProp, position);
        }

        PlaceProperty(stepObjectsShownProp, position);
        PlaceProperty(stepObjectsHiddenProp, position);
        PlaceProperty(stepSApiObjectsInstantiatedProp, position);
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {

        nextStepEventProp = property.FindPropertyRelative("nextStepEvent");
        buttonNextStepProp = property.FindPropertyRelative("buttonNextStep");
        nextStepObjectHierarchyNameProp = property.FindPropertyRelative("nextStepObjectHierarchyName");
        arrowTargetGameObjectProp = property.FindPropertyRelative("arrowTargetGameObject");
        textProp = property.FindPropertyRelative("text");
        arrowTargetHierarchyNameProp = property.FindPropertyRelative("arrowTargetHierarchyName");
        teleportBoolProp = property.FindPropertyRelative("teleportPlayer");
        teleportPositionProp = property.FindPropertyRelative("teleportPosition");
        arrowTargetTypeProp = property.FindPropertyRelative("arrowTargetType");

        stepObjectsHiddenProp = property.FindPropertyRelative("stepObjectsHidden");
        stepObjectsShownProp = property.FindPropertyRelative("stepObjectsShown");
        stepSApiObjectsInstantiatedProp = property.FindPropertyRelative("stepSApiObjectsInstantiated");

        //Debug.Log(20 + padding * 10 + EditorGUI.GetPropertyHeight(textProp) + EditorGUI.GetPropertyHeight(arrowTargetTypeProp) + EditorGUI.GetPropertyHeight(arrowTargetGameObjectProp) + EditorGUI.GetPropertyHeight(teleportBoolProp) + EditorGUI.GetPropertyHeight(teleportPositionProp) + EditorGUI.GetPropertyHeight(nextStepEventProp) + EditorGUI.GetPropertyHeight(nextStepObjectHierarchyNameProp) + EditorGUI.GetPropertyHeight(stepObjectsShownProp, true) + EditorGUI.GetPropertyHeight(stepObjectsHiddenProp, true) + EditorGUI.GetPropertyHeight(stepSApiObjectsInstantiatedProp, true));
        return 20 + padding * 10 + EditorGUI.GetPropertyHeight(textProp) + EditorGUI.GetPropertyHeight(arrowTargetTypeProp) + EditorGUI.GetPropertyHeight(arrowTargetGameObjectProp) + EditorGUI.GetPropertyHeight(teleportBoolProp) + EditorGUI.GetPropertyHeight(teleportPositionProp) + EditorGUI.GetPropertyHeight(nextStepEventProp) + EditorGUI.GetPropertyHeight(nextStepObjectHierarchyNameProp) + EditorGUI.GetPropertyHeight(stepObjectsShownProp, true) + EditorGUI.GetPropertyHeight(stepObjectsHiddenProp, true) + EditorGUI.GetPropertyHeight(stepSApiObjectsInstantiatedProp, true);
    }

    private void PlaceProperty(SerializedProperty property, Rect position)
    {
        EditorGUI.PropertyField(new Rect(position.x, position.y + lastPropertyPosition, position.width, padding + EditorGUI.GetPropertyHeight(property)), property, true);
        lastPropertyPosition += EditorGUI.GetPropertyHeight(property, true) + padding;
    }

    private void DrawBlank(SerializedProperty property, Rect position)
    {
        EditorGUI.DrawRect(new Rect(position.x, position.y + lastPropertyPosition, position.width, padding + EditorGUI.GetPropertyHeight(property)), Color.gray);
        lastPropertyPosition += EditorGUI.GetPropertyHeight(property, true) + padding;
    }

    ///
    ///https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs
    ///

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

    public static void SetTargetObjectOfProperty(SerializedProperty prop, object value)
    {
        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements.Take(elements.Length - 1))
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

        if (Object.ReferenceEquals(obj, null)) return;

        try
        {
            var element = elements.Last();

            if (element.Contains("["))
            {
                var tp = obj.GetType();
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                var field = tp.GetField(elementName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var arr = field.GetValue(obj) as System.Collections.IList;
                arr[index] = value;

                //var elementName = element.Substring(0, element.IndexOf("["));
                //var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                //var arr = DynamicUtil.GetValue(element, elementName) as System.Collections.IList;
                //if (arr != null) arr[index] = value;
            }
            else
            {
                var tp = obj.GetType();
                var field = tp.GetField(element, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(obj, value);
                }
                //DynamicUtil.SetValue(obj, element, value);
            }

        }
        catch
        {
            return;
        }
    }
}




#endif