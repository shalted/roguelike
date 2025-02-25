using UnityEditor;
using UnityEngine;

public class ElementControlEditor : EditorWindow
{
    private float elementValue = 0f;
    private bool isDragging = false;
    private Vector2 dragStartPos;

    [MenuItem("Window/Element Control")]
    public static void ShowWindow()
    {
        GetWindow<ElementControlEditor>("Element Control");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Drag to Control Element Value", EditorStyles.boldLabel);

        // 绘制一个拖拽的元素
        GUILayout.Label($"Element Value: {elementValue:F2}");

        // 当鼠标按下并开始拖动时，开始拖拽
        if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            isDragging = true;
            dragStartPos = Event.current.mousePosition;
            Event.current.Use(); // 消耗事件，防止传递给其他控件
        }

        // 当鼠标拖动时，改变元素的值
        if (isDragging && Event.current.type == EventType.MouseDrag)
        {
            float delta = Event.current.mousePosition.y - dragStartPos.y;
            elementValue += delta * 0.01f; // 根据鼠标垂直移动距离更新值
            dragStartPos = Event.current.mousePosition; // 更新起始位置
            Repaint(); // 强制重绘，实时更新界面
            Event.current.Use(); // 消耗事件，防止传递给其他控件
        }

        // 当鼠标松开时，停止拖拽
        if (isDragging && Event.current.type == EventType.MouseUp)
        {
            isDragging = false;
            Event.current.Use(); // 消耗事件，防止传递给其他控件
        }
    }
}