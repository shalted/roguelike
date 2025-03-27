using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class PrefabReferenceFinder : EditorWindow
{
    private string targetFolderPath = "Assets/Things/Textures/UI"; // 要查找的目标资源路径
    private string searchFolder = "Assets/Things/Prefabs"; // 要扫描的预制体文件夹
    private string changeTime = "2025/1/1"; // 要扫描的预制体文件夹
    private readonly Dictionary<string, int> guidList = new Dictionary<string, int>();
    private List<string> resGuidList;

    [MenuItem("Window/PrefabReferenceFinder")]
    public static void ShowWindow()
    {
        GetWindow<PrefabReferenceFinder>("Prefab Reference Finder");
    }

    private void OnGUI()
    {
        GUILayout.Label("未引用资源查找", EditorStyles.boldLabel);
        targetFolderPath = EditorGUILayout.TextField("资源目标文件夹", targetFolderPath);
        searchFolder = EditorGUILayout.TextField("预制体目标文件夹", searchFolder);
        changeTime = EditorGUILayout.TextField("时间", changeTime);

        if (GUILayout.Button("Start Search"))
        {
            FindReferences();
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void FindReferences()
    {
        var targetGuid = AssetDatabase.AssetPathToGUID(targetFolderPath);
        if (string.IsNullOrEmpty(targetGuid))
        {
            Debug.LogError("Target asset not found: " + targetFolderPath);
            return;
        }

        // 获取所有UI预制体
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { searchFolder });
        var total = prefabGuids.Length;
        
        for (var i = 0; i < prefabGuids.Length; i++)
        {
            //获取路径
            var guid = prefabGuids[i];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            
            // 显示进度条
            EditorUtility.DisplayProgressBar("Scanning Prefabs", $"Processing {i+1}/{total} ({path})", (float)i / total);
            
            //仅展示预制体
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) continue;

            // 递归检查预制体
            var referencePaths = new List<string>();
            CheckPrefabRecursive(prefab.transform, "", ref referencePaths);
        }

        CheckUnUseUIResources();
        RemoveUselessResources();
        EditorUtility.ClearProgressBar();
        Debug.Log("Search completed");
    }

    private void CheckPrefabRecursive(Transform transform, string currentPath, ref List<string> results)
    {
        currentPath += (currentPath == "" ? "" : "/") + transform.name;
        
        // 检查组件引用
        foreach (var component in transform.GetComponents<Component>())
        {
            if (component == null) continue;
            if(component.GetType() != typeof(UnityEngine.UI.Image)) continue;
            
            var so = new SerializedObject(component);
            var prop = so.GetIterator();
            var componentType = component.GetType().Name;
            results.Add($"{currentPath} ({componentType}.{prop.name})");
            while (prop.NextVisible(true))
            {
                var component2 = prop.serializedObject.targetObject as Component;
                if (component2 == null) continue;
                if(component2.GetType() != typeof(UnityEngine.UI.Image)) continue;
                if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;
                var obj = prop.objectReferenceValue;
                if (obj == null) continue;
                var objGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
                guidList.TryAdd(objGuid, 1);
            }
        }

        // 递归检查子对象
        foreach (Transform child in transform)
        {
            CheckPrefabRecursive(child, currentPath, ref results);
        }
    }
    
    private void CheckUnUseUIResources()
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { targetFolderPath });
        resGuidList = new List<string>(prefabGuids);
        var tileList = changeTime.Split("/");
        foreach (var guid in guidList)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid.Key);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var assetPath = AssetDatabase.GetAssetPath(texture);
            var fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../", assetPath));
            if (!File.Exists(fullPath)) continue;
            var lastWriteTime = File.GetLastWriteTime(fullPath);
            if (tileList.Length >= 3)
            {
                if (int.Parse(tileList[0]) > lastWriteTime.Year)
                {
                    var fileName = Path.GetFileName(path);
                    var newPath = Path.Combine("Assets/Things/Textures/UI/OldUI", fileName);
                    AssetDatabase.MoveAsset(path, newPath);
                    Debug.Log("移动老路径" + path + "  " + "新路径为：" + newPath + "，修改时间：" + lastWriteTime + "的资源");
                }
                else if(int.Parse(tileList[0]) == lastWriteTime.Year && int.Parse(tileList[1]) >= lastWriteTime.Month)
                {
                    if (int.Parse(tileList[1]) > lastWriteTime.Month)
                    {
                        var fileName = Path.GetFileName(path);
                        var newPath = Path.Combine("Assets/Things/Textures/UI/OldUI", fileName);
                        AssetDatabase.MoveAsset(path, newPath);
                        Debug.Log("移动老路径" + path + "  " + "新路径为：" + newPath + "，修改时间：" + lastWriteTime + "的资源");
                    }
                    else if (int.Parse(tileList[1]) == lastWriteTime.Month && int.Parse(tileList[2]) >= lastWriteTime.Day)
                    {
                        var fileName = Path.GetFileName(path);
                        var newPath = Path.Combine("Assets/Things/Textures/UI/OldUI", fileName);
                        AssetDatabase.MoveAsset(path, newPath);
                        Debug.Log("移动老路径" + path + "  " + "新路径为：" + newPath + "，修改时间：" + lastWriteTime + "的资源");
                    }
                }
            }
            resGuidList.Remove(guid.Key);
        }
    }

    private void RemoveUselessResources()
    {
        var tileList = changeTime.Split("/");
        foreach (var guid in resGuidList)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var assetPath = AssetDatabase.GetAssetPath(texture);
            var fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../", assetPath));
            if (!File.Exists(fullPath)) continue;
            var lastWriteTime = File.GetLastWriteTime(fullPath);
            if (tileList.Length < 3)
            {
                Debug.Log("时间格式错误，请检查输入");
                return;
            }
            if (int.Parse(tileList[0]) > lastWriteTime.Year)
            {
                Debug.Log("没有找到guid为" + guid + "  " + "路径为：" + fullPath + "，修改时间：" + lastWriteTime + "的资源");
                AssetDatabase.DeleteAsset(path);
            }
            else if(int.Parse(tileList[0]) == lastWriteTime.Year && int.Parse(tileList[1]) >= lastWriteTime.Month)
            {
                if (int.Parse(tileList[1]) > lastWriteTime.Month)
                {
                    Debug.Log("没有找到guid为" + guid + "  " + "路径为：" + fullPath + "，修改时间：" + lastWriteTime + "的资源");
                    AssetDatabase.DeleteAsset(path);
                }
                else if (int.Parse(tileList[1]) == lastWriteTime.Month && int.Parse(tileList[2]) >= lastWriteTime.Day)
                {
                    Debug.Log("没有找到guid为" + guid + "  " + "路径为：" + fullPath + "，修改时间：" + lastWriteTime + "的资源");
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }
    }
}