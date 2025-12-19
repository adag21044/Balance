using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MethodAttribute))]
public class MethotAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //MethotAttribute buttonAttribute = attribute as MethotAttribute;
//
        //if (GUILayout.Button("buttonAttribute"))
        //{
        //    object targetObject = property.serializedObject.targetObject;
        //    MethodInfo methodInfo = targetObject.GetType().GetMethod("MethotNoParameters");
        //    methodInfo.Invoke(targetObject, null);
        //}
        
        
        //MethotAttribute methotAttribute = attribute as MethotAttribute;
        //Debug.Log(methotAttribute.GetType());
        //
        //Object target = property.serializedObject.targetObject;
        //System.Type type = target.GetType();
        //System.Reflection.MethodInfo method = type.GetMethod("MethotNoParameters");
        //
        //EditorGUI.IntField(position,1);
        //base.OnGUI(position, property, label);
        //string methodName = (attribute as MethotAttribute).MethodName;
        //Object target = property.serializedObject.targetObject;
        //System.Type type = target.GetType();
        //System.Reflection.MethodInfo method = type.GetMethod(methodName);
        //property.met
    }
}

[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
public class MethodAttribute : PropertyAttribute
{
    public bool onlyPlayMode;
    public bool showWarnings;
    public MethodAttribute(bool onlyPlayMode = false, bool showWarnings = false)
    {
        this.onlyPlayMode = onlyPlayMode;
        this.showWarnings = showWarnings;
    }
}