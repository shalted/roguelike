using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class StateLineClass:TimeLineBaseClass
    {
        private TextField _timesTextField;
        private Label _startStateLabel;
        private Label _endStateLabel;
        private Label _curtateLabel;
        private VisualElement CurElement { get; set; }
        private VisualElement LeftCursorElement { get; set; }
        private VisualElement RightCursorElement { get; set; }
        private bool IsDragging { get; set; }
        private float CommonWidth { get; set; }
        private bool IsFire { get; set; }
        
        private VisualElement _curMoveElement;
        private Vector2 dragOffset;
        private float oriX;
        private Matrix4x4 originalGUIMatrix;

        public void CreateStateLine(VisualElement root)
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
            CreateStateShowEle(animationLine);
            CreateStateTimeLineEle(animationLine);
            root.Add(animationLine);
        }
        
        // 创建动作按钮节点
        private void CreateStateShowEle(VisualElement parentElement)
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
            CreateStateNameEle(timeButtonRoot);
            CreateStateStartTimeEle(timeButtonRoot);
            CreateStateEndTimeEle(timeButtonRoot);
            parentElement.Add(timeButtonRoot);
        }

        private void CreateStateNameEle(VisualElement parentElement)
        {
            var temp = new Label
            {
                style =
                {
                    marginLeft = 5,
                },
                text = "状态:"
            };
            parentElement.Add(temp);
            _timesTextField = new TextField
            {
                style =
                {
                    width = 60,
                    height = 20,
                    justifyContent = Justify.FlexStart,
                    backgroundColor = new StyleColor(Color.gray),  // 设置背景颜色
                    unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                    marginLeft = 10
                }
            };
            parentElement.Add(_timesTextField);
        }
        
        private void CreateStateStartTimeEle(VisualElement parentElement)
        {
            var temp = new Label
            {
                style =
                {
                    marginLeft = 5,
                },
                text = ($"开始:"),
            };
            parentElement.Add(temp);
            _startStateLabel = new Label
            {
                style =
                {
                    width = 50,
                    height = 20,
                    justifyContent = Justify.FlexStart,
                    backgroundColor = new StyleColor(Color.gray),  // 设置背景颜色
                    unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                    marginLeft = 5,
                },
                text = ($"0"),
            };
            _startStateLabel.RegisterCallback<PointerDownEvent>((evt) => OnMouseDown(evt, CurElement, _startStateLabel));
            parentElement.Add(_startStateLabel);
        }
        
        private void CreateStateEndTimeEle(VisualElement parentElement)
        {
            var temp = new Label
            {
                style =
                {
                    marginLeft = 5,
                },
                text = ($"结束:"),
            };
            parentElement.Add(temp);
            _endStateLabel = new Label
            {
                style =
                {
                    width = 50,
                    height = 20,
                    justifyContent = Justify.FlexStart,
                    backgroundColor = new StyleColor(Color.gray),  // 设置背景颜色
                    unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                    marginLeft = 5,
                },
                text = ($"1"),
            };
            _endStateLabel.RegisterCallback<PointerDownEvent>((evt) => OnMouseDown(evt, CurElement, _endStateLabel));
            parentElement.Add(_endStateLabel);
        }
        
        private void CreateStateTimeLineEle(VisualElement parentElement)
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
            CreateStateCursor(animationTimeLine);
        }
        
        private void CreateStateCursor(VisualElement parentElement)
        {
            CommonWidth = (900 / TimelineEditorWindow.TotalTimeInSeconds * (float.Parse(_endStateLabel.text) - float.Parse(_startStateLabel.text)));
            CurElement = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    width  = CommonWidth,
                    height = 40,
                    alignItems = Align.Center,
                    justifyContent = Justify.FlexStart,  // 垂直居中
                    backgroundColor = new StyleColor(HexToColor("#40e0d0")),
                }
            };
            parentElement.Add(CurElement);
            CreateStateStartCursor(CurElement, 0);
            CreateStateEndCursor(CurElement, CommonWidth - 10f);
            parentElement.RegisterCallback<PointerDownEvent>((evt) => OnMouseDown(evt, CurElement));
        }
        
        // 创建事件游标
        private void CreateStateStartCursor(VisualElement parentElement, float marginLeft)
        {
            LeftCursorElement = new VisualElement
            {
                style =
                {
                    width  = 5,
                    height = 40,
                    unityTextAlign = TextAnchor.MiddleLeft,  // 水平居中
                    justifyContent = Justify.FlexStart,  // 垂直居中
                    backgroundColor = new StyleColor(HexToColor("#000000")),
                    marginLeft = marginLeft,
                }
            };
            LeftCursorElement.RegisterCallback<PointerDownEvent>((evt) => OnMouseDown(evt, LeftCursorElement, _startStateLabel));
            parentElement.Add(LeftCursorElement);
        }
        
        // 创建事件游标
        private void CreateStateEndCursor(VisualElement parentElement, float marginLeft)
        {
            RightCursorElement = new VisualElement
            {
                style =
                {
                    width  = 5,
                    height = 40,
                    unityTextAlign = TextAnchor.MiddleLeft,  // 水平居中
                    justifyContent = Justify.FlexStart,  // 垂直居中
                    backgroundColor = new StyleColor(HexToColor("#000000")),
                    marginLeft = marginLeft,
                }
            };
            RightCursorElement.RegisterCallback<PointerDownEvent>((evt) => OnMouseDown(evt, RightCursorElement, _endStateLabel));
            parentElement.Add(RightCursorElement);
        }
        
        // 鼠标监听事件
        private void OnMouseDown(PointerDownEvent mouseDownState, VisualElement element, Label label = null)
        {
            Debug.Log("鼠标点击按下");
            IsDragging = true;
            dragOffset = mouseDownState.position;
            _curMoveElement = CurElement;
            EditorApplication.update += OnDragUpdate;
            this.CapturePointer(mouseDownState.pointerId); // 捕获指针
            mouseDownState.StopPropagation();
            style.cursor = new StyleCursor((StyleKeyword)MouseCursor.Link);
            _curtateLabel = label;
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
            var min = 0.0f;
            var max = TimelineEditorWindow.TimelineWidth - CommonWidth;
            var oneSecWidth = TimelineEditorWindow.TimelineWidth / TimelineEditorWindow.TotalTimeInSeconds;
            if (_curtateLabel == _startStateLabel)
            {
                max = float.Parse(_endStateLabel.text) * oneSecWidth;
                ChangeStartLabel(min, max, evt);
            }

            else if (_curtateLabel == _endStateLabel)
            {
                min = float.Parse(_startStateLabel.text) * oneSecWidth;
                ChangeEndLabel(min, max, evt);
            }
            else
            {
                var mousePos = evt.mousePosition;
                var moveX = mousePos.x - dragOffset.x;
                dragOffset = evt.mousePosition;
                var oriPos = Mathf.Clamp(oriX + moveX, min, max);
                oriX = oriPos;
                Debug.Log(oriPos - 5);
                Debug.Log(mousePos);
                Debug.Log(moveX);
                _curMoveElement.style.marginLeft = oriPos - 5;
                // var oneSecWidth = TimelineEditorWindow.TimelineWidth / TimelineEditorWindow.TotalTimeInSeconds;
                _startStateLabel.text = (Mathf.Round(oriPos / oneSecWidth * 100) / 100f).ToString(CultureInfo.CurrentCulture);
                _endStateLabel.text = (Mathf.Round((oriPos + CommonWidth) / oneSecWidth * 100) / 100f).ToString(CultureInfo.CurrentCulture);
            }
        }
        
        private void ChangeStartLabel(float min, float max, Event evt)
        {
            var mousePos = evt.mousePosition;
            var moveX = mousePos.x - dragOffset.x;
            dragOffset = evt.mousePosition;
            var oneSecWidth = TimelineEditorWindow.TimelineWidth / TimelineEditorWindow.TotalTimeInSeconds;
            var oriPos = Mathf.Clamp(oriX + moveX, min, max);
            var differenceDis = oriX - oriPos;
            _curMoveElement.style.width = CommonWidth + differenceDis;
            CommonWidth += differenceDis;
            oriX = oriPos;
            _curMoveElement.style.marginLeft = oriPos - 5;
            _curtateLabel.text = (Mathf.Round(oriPos / oneSecWidth * 100) / 100f).ToString(CultureInfo.CurrentCulture);
            LeftCursorElement.style.marginLeft = 0;
            RightCursorElement.style.marginLeft = CommonWidth - 5;
        }
        
        private void ChangeEndLabel(float min, float max, Event evt)
        {
            var mousePos = evt.mousePosition;
            var moveX = mousePos.x - dragOffset.x;
            dragOffset = evt.mousePosition;
            var oneSecWidth = TimelineEditorWindow.TimelineWidth / TimelineEditorWindow.TotalTimeInSeconds;
            var oriPos = Mathf.Clamp(oriX + CommonWidth + moveX, min, max);
            var differenceDis = oriPos - oriX;
            Debug.Log(differenceDis);
            _curMoveElement.style.width = differenceDis;
            CommonWidth = differenceDis;
            _curtateLabel.text = (Mathf.Round(oriPos / oneSecWidth * 100) / 100f).ToString(CultureInfo.CurrentCulture);
            LeftCursorElement.style.marginLeft = 0;
            RightCursorElement.style.marginLeft = CommonWidth - 5;
        }
        
        public void Play(float currentTime)
        {
            if (IsFire && currentTime > float.Parse(_startStateLabel.text) && currentTime < float.Parse(_endStateLabel.text))
            {
                IsFire = false;
                Debug.Log("进入状态：" + _timesTextField.text);
            }
            else if (!IsFire && currentTime >= float.Parse(_endStateLabel.text) || currentTime < float.Parse(_startStateLabel.text))
            {
                IsFire = true;
                Debug.Log("离开状态：" + _timesTextField.text);
            }
        }
    }
}