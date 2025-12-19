using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(MonoBehaviour), true)]
public class MasearyInspector : Editor
{
    private IEnumerable<MethodInfo> methods;
    
    Sprite sprite;
    private object[] objectParameters;
    private int parameterCount = 0;
    
    private Object[] unityObjectParameters;
    private int unityObjectParameterCount = 0;
    
    public static List<Object> tempUnityObjectParameters;
    
    Color newBackgroundColor = new Color(1f, 1f, 1f, 0.1f);
    private void OnEnable()
    {
        parameterCount = 0;
        unityObjectParameterCount = 0;
        
        methods = target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => true);
        
        foreach (MethodInfo method in methods)
        {
            if (Attribute.GetCustomAttribute(method, typeof(MethodAttribute)) is MethodAttribute buttonAttribute)
            {
                for (int i = 0; i < method.GetParameters().Count(); i++)
                {
                    Type type = method.GetParameters()[i].ParameterType;
                    if (type == typeof(int) || type == typeof(float) || type == typeof(string))
                    {
                        parameterCount++;
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    {
                        unityObjectParameterCount++;
                    }
                }
            }
        } 

        tempUnityObjectParameters ??= new();
        
     
        objectParameters = new object[parameterCount];
        unityObjectParameters = new Object[unityObjectParameterCount]; 

        for (int i = 0; i < tempUnityObjectParameters.Count; i++)
        {
            if (unityObjectParameters != null && i < unityObjectParameters.Length)
            {
                unityObjectParameters[i] = tempUnityObjectParameters[i];
            }
        }
    }


    [Obsolete("Obsolete")]
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();
        
        DrawParameterFields();

        if (unityObjectParameters != null)
        {
            tempUnityObjectParameters = unityObjectParameters.ToList();
        } 
    } 

    [Obsolete("Obsolete")]
    private void DrawParameterFields()
    {
        int counter = 0;
        int unityCounter = 0;

        foreach (MethodInfo method in methods)
        {
            if (Attribute.GetCustomAttribute(method, typeof(MethodAttribute)) is MethodAttribute buttonAttribute)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                var r = EditorGUILayout.BeginVertical();

                // EditorGUI.DrawRect(r, newBackgroundColor);
                List<object> parametersList = new List<object>();
                List<Object> objectList = new List<Object>();

                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < method.GetParameters().Count(); i++)
                {
                    EditorGUILayout.LabelField(method.GetParameters()[i].Name + ":", GUILayout.MaxWidth(50));

                    if (method.GetParameters()[i].ParameterType == typeof(int))
                    {
                        objectParameters[counter] =
                            EditorGUILayout.IntField(
                                objectParameters[counter] == null ? 0 : (int) objectParameters[counter]);
                        parametersList.Add(objectParameters[counter]);
                        counter++;
                    }
                    else if (method.GetParameters()[i].ParameterType == typeof(float))
                    {
                        objectParameters[counter] =
                            EditorGUILayout.FloatField(objectParameters[counter] == null
                                ? 0
                                : (float) objectParameters[counter]);
                        parametersList.Add(objectParameters[counter]);
                        counter++;
                    }
                    else if (method.GetParameters()[i].ParameterType == typeof(string))
                    {
                        objectParameters[counter] = EditorGUILayout.TextField((string) objectParameters[counter]);
                        parametersList.Add(objectParameters[counter]);
                        counter++;
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(method.GetParameters()[i].ParameterType))
                    {
                        EditorGUILayout.BeginVertical();
                        
                        unityObjectParameters[unityCounter] =
                            EditorGUILayout.ObjectField(unityObjectParameters[unityCounter],
                                method.GetParameters()[i].ParameterType);
                        parametersList.Add(unityObjectParameters[unityCounter]);
                        
                        if (buttonAttribute.showWarnings && unityObjectParameters[unityCounter] == null)
                        {
                            EditorGUILayout.HelpBox( "Field is empty. This may cause an error.", UnityEditor.MessageType.Warning);
                        }
                        
                        unityCounter++; 
                        
                        EditorGUILayout.EndVertical();
                    }
                }

                EditorGUILayout.EndHorizontal();

                var isPressed = DrawButtonAndCheckIsPressed(buttonAttribute, method);

                if (isPressed)
                {
                    foreach (object methodTarget in targets)
                    {
                        method.Invoke(methodTarget, parametersList.ToArray());
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }
    }

    private static bool DrawButtonAndCheckIsPressed(MethodAttribute buttonAttribute, MethodInfo method)
    {
        bool isPressed = false;

        if (buttonAttribute.onlyPlayMode)
        {
            if (EditorApplication.isPlaying)
            {
                isPressed = GUILayout.Button(method.Name);
            }
            else
            {
                GUI.enabled = false;
                isPressed = GUILayout.Button(method.Name + " (Only Play Mode)");
                GUI.enabled = true;
            }
        }
        else
        {
            isPressed = GUILayout.Button(method.Name);
        }

        return isPressed;
    }
}