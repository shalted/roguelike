using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

[InitializeOnLoad]
public static class ScrollViewZoomEditor
{
    static ScrollViewZoomEditor()
    {
        // 注册在编辑器 UI 上的回调
        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate()
    {
        // 获取当前的 VisualElement 根
        var root = EditorWindow.focusedWindow?.rootVisualElement;

        if (root == null) return;

        // 查找 ScrollView
        var scrollView = root.Q<ScrollView>("myScrollView");

        if (scrollView != null)
        {
            root.RegisterCallback<WheelEvent>(evt =>
            {
                // 计算缩放因子
                float scaleFactor = 1 + (evt.delta.y * 0.01f); // 你可以调整缩放比例

                // 限制缩放范围
                float newScale = Mathf.Clamp(scaleFactor, 0.5f, 2.0f);

                // 应用缩放
                scrollView.transform.scale = new Vector3(newScale, newScale, 1);

                // 防止事件传播
                evt.StopImmediatePropagation();
            });
        }
    }
}
