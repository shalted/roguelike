using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class EventLineClass:TimeLineBaseClass
    {
        private TextField _timesTextField;
        private Label _eventLabel;
        private VisualElement CurElement { get; set; }
        private bool IsDragging { get; set; }
        private bool IsFire { get; set; }
        
        private VisualElement _curMoveElement;
        private Vector2 dragOffset;
        private float oriX;
        private Matrix4x4 originalGUIMatrix;

        public void CreateEventLine(VisualElement root)
        {
            var animationLine = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart,
                    backgroundColor = new StyleColor(HexToColor("#4F4F4F")),
                    height = 40 // 设置固定高,
                }
            };
            CreateEventShowEle(animationLine);
            CreateEventTimeLineEle(animationLine);
            root.Add(animationLine);
        }
        
        // 创建动作按钮节点
        private void CreateEventShowEle(VisualElement parentElement)
        {
            var timeButtonRoot = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center,  // 水平居中
                    alignItems = Align.Center, 
                    backgroundColor = new StyleColor(HexToColor("#4F4F4F")),
                    height = 40, // 设置固定高度
                    width = 300
                }
            };
            CreateEventNameEle(timeButtonRoot);
            CreateEventTimeEle(timeButtonRoot);
            parentElement.Add(timeButtonRoot);
        }

        private void CreateEventNameEle(VisualElement parentElement)
        {
            var temp = new Label
            {
                style =
                {
                    marginLeft = 10,
                },
                text = "事件名称:"
            };
            parentElement.Add(temp);
            _timesTextField = new TextField
            {
                style =
                {
                    width = 100,
                    height = 20,
                    justifyContent = Justify.FlexStart,
                    backgroundColor = new StyleColor(Color.gray),  // 设置背景颜色
                    unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                    marginLeft = 10
                }
            };
            parentElement.Add(_timesTextField);
        }
        
        private void CreateEventTimeEle(VisualElement parentElement)
        {
            var temp = new Label
            {
                style =
                {
                    marginLeft = 10,
                },
                text = ($"时间:"),
            };
            parentElement.Add(temp);
            _eventLabel = new Label
            {
                style =
                {
                    width = 50,
                    height = 20,
                    justifyContent = Justify.FlexStart,
                    backgroundColor = new StyleColor(Color.gray),  // 设置背景颜色
                    unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                    marginLeft = 10,
                },
                text = ($"0.0"),
            };
            _eventLabel.RegisterCallback<PointerDownEvent>((evt) => OnMouseDown(evt, CurElement));
            parentElement.Add(_eventLabel);
        }
        
        private void CreateEventTimeLineEle(VisualElement parentElement)
        {
            var animationTimeLine = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    justifyContent = Justify.FlexStart,
                    backgroundColor = new StyleColor(HexToColor("#8B8B83")),
                    height = 40, // 设置固定高度
                    width = 900 // 设置固定高度
                }
            };
            parentElement.Add(animationTimeLine);
            CreateEventCursor(animationTimeLine);
        }
        
        // 创建事件游标
        private void CreateEventCursor(VisualElement parentElement)
        {
            CurElement = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    width  = 10,
                    height = 40,
                    unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                    justifyContent = Justify.Center,  // 垂直居中
                    backgroundColor = new StyleColor(HexToColor("#000000")),
                }
            };
            CurElement.RegisterCallback<PointerDownEvent>((evt) => OnMouseDown(evt, CurElement));
            parentElement.Add(CurElement);
        }
        
        // 鼠标监听事件
        private void OnMouseDown(PointerDownEvent mouseDownEvent, VisualElement element)
        {
            Debug.Log("鼠标点击按下");
            IsDragging = true;
            dragOffset = mouseDownEvent.position;
            _curMoveElement = CurElement;
            EditorApplication.update += OnDragUpdate;
            this.CapturePointer(mouseDownEvent.pointerId); // 捕获指针
            mouseDownEvent.StopPropagation();
            style.cursor = new StyleCursor((StyleKeyword)MouseCursor.Link);
        }

        private void OnDragUpdate()
        {
            var evt = TimelineEditorWindow.Evt;
            if (evt is { type: EventType.MouseUp })
            {
                IsDragging = false;
                EditorApplication.update -= OnDragUpdate;
                style.cursor = StyleKeyword.Null;
                return;
            }
            if (evt is not { type: EventType.MouseDrag } || !IsDragging) return;
            var mousePos = evt.mousePosition;
            var moveX = mousePos.x - dragOffset.x;
            dragOffset = evt.mousePosition;
            var oriPos = Mathf.Clamp(oriX + moveX, 0, TimelineEditorWindow.TimelineWidth);
            oriX = oriPos;
            _curMoveElement.style.marginLeft = oriPos - 5;
            var oneSecWidth = TimelineEditorWindow.TimelineWidth / TimelineEditorWindow.TotalTimeInSeconds;
            _eventLabel.text = (Mathf.Round(oriPos / oneSecWidth * 100) / 100f).ToString(CultureInfo.CurrentCulture);
        }
        
        public void Play(float currentTime)
        {
            if (IsFire && currentTime > float.Parse(_eventLabel.text))
            {
                IsFire = false;
                Debug.Log("发送事件：" + _timesTextField.text);
            }
            else if (currentTime < float.Parse(_eventLabel.text))
            {
                IsFire = true;
            }
        }
    }
}