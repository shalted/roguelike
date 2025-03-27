using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Object = System.Object;

public class BatchComponentReplacement : EditorWindow
{
    private string searchFolder = "Assets/Things/Prefabs"; // 要扫描的预制体文件夹
    private UnityEngine.Object oldAsset;
    private UnityEngine.Object newAsset;
    private Object asset1 = new Object();
    private Object asset2 = new Object();
    private GameObject curPrefab;
    private string asset1Guid;
    private string asset2Guid;
    private static int _modifiedCount = 0;
    private UnityEngine.Object[] droppedObjects;

    [MenuItem("Tool/ResourceReplacement")]
    public static void ShowWindow()
    {
        GetWindow<BatchComponentReplacement>("Prefab Reference Finder");
    }

    private void OnGUI()
    {
        GUILayout.Label("资源引用批量替换", EditorStyles.boldLabel);
        searchFolder = EditorGUILayout.TextField("预制体目标文件夹", searchFolder);
        oldAsset = EditorGUILayout.ObjectField("原资源", oldAsset, typeof(UnityEngine.Object), false);
        newAsset = EditorGUILayout.ObjectField("新资源", newAsset, typeof(UnityEngine.Object), false);
        if (!GUILayout.Button("Start Search")) return;
        if (oldAsset.GetType() != newAsset.GetType())
        {
            Debug.Log("替换资源类型不一致，请检查~~");
        }
        else
        {
            var path = AssetDatabase.GetAssetPath(oldAsset);
            asset1Guid = AssetDatabase.AssetPathToGUID(path);
            path = AssetDatabase.GetAssetPath(newAsset);
            asset2Guid = AssetDatabase.AssetPathToGUID(path);
            FindReferences();
        }
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private void FindReferences()
    {
        // 获取所有UI预制体
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { searchFolder });
        var total = prefabGuids.Length;
        _modifiedCount = 0;
        for (var i = 0; i < prefabGuids.Length; i++)
        {
            //获取路径
            var guid = prefabGuids[i];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            
            // 显示进度条
            EditorUtility.DisplayProgressBar("Scanning Prefabs", $"Processing {i+1}/{total} ({path})", (float)i / total);
            
            //仅展示预制体
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            curPrefab = prefab;
            if (!prefab) continue;
            
            // 检查组件引用
            CheckPrefabRecursive(prefab.transform, "");
            PrefabUtility.SavePrefabAsset(prefab);
            AssetDatabase.SaveAssets();
        }
        Debug.Log($"成功修改 {_modifiedCount} 个预制体");
        
        EditorUtility.ClearProgressBar();
    }
    
    private void CheckPrefabRecursive(Transform transform, string currentPath)
    {
        currentPath += (currentPath == "" ? "" : "/") + transform.name;
        
        // 检查组件引用
        foreach (var component in transform.GetComponents<Component>())
        {
            var modified = false;
            var so = new SerializedObject(component);
            var prop = so.GetIterator();
            while (prop.NextVisible(true))
            {
                if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;
                var objGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prop.objectReferenceValue));
                if (objGuid != asset1Guid) continue;
                prop.objectReferenceValue = newAsset;
                prop.serializedObject.ApplyModifiedProperties();
                modified = true;
            }

            if (!modified) continue;
            Debug.Log(_modifiedCount);
            _modifiedCount++;
        }

        // 递归检查子对象
        foreach (Transform child in transform)
        {
            CheckPrefabRecursive(child, currentPath);
        }
    }
}