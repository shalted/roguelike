using UnityEngine;
using UnityEngine.UIElements;
using System.Globalization;

namespace Editor
{
    public class AnimationLineClass:TimeLineBaseClass
    {
        private int LineCount { get; set; }
        private Label EndTime { get; set; }
        private float CommonWidth { get; set; }
        private bool IsDragging { get; set; }
        private VisualElement CurElement { get; set; }
        public AnimationClip AnimationClip { get; private set; }
        public Label StartTime { get; private set; }
        
        public void CreateAnimationLine(VisualElement root, AnimationClip animation, int lineCount)
        {
            var animationLine = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart,
                    backgroundColor = LineCount % 2 == 0 ? new StyleColor(HexToColor("#4F4F4F")) : new StyleColor(HexToColor("#5c5c5c")),
                    height = 40, // 设置固定高,
                }
            };
            LineCount = lineCount;
            AnimationClip = animation;
            CreateAnimationShowEle(animationLine, animation);
            CreateAnimationTimeLineEle(animationLine, animation);
            root.Add(animationLine);
        }
        
        // 创建动作按钮节点
        private void CreateAnimationShowEle(VisualElement parentElement, AnimationClip animation)
        {
            var timeButtonRoot = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center,  // 水平居中
                    alignItems = Align.Center, 
                    backgroundColor = LineCount % 2 == 0 ? new StyleColor(HexToColor("#4F4F4F")) : new StyleColor(HexToColor("#5c5c5c")),
                    height = 40, // 设置固定高度
                    width = 300,
                }
            };
            CreateAnimationNameEle(timeButtonRoot, animation);
            CreateAnimationLabelEle(timeButtonRoot, animation);
            parentElement.Add(timeButtonRoot);
        }
        
        // 动作名称
        private void CreateAnimationNameEle(VisualElement parentElement, AnimationClip animation)
        {
            var temp = new Label($"动作");
            parentElement.Add(temp);
            var gameObjectText = new Label
            {
                style =
                {
                    width = 100,
                    height = 20,
                    backgroundColor = new StyleColor(Color.gray),  // 设置背景颜色
                    unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                    justifyContent = Justify.Center,  // 垂直居中
                    marginLeft = 2,
                },
                text = animation.name,
            };
            parentElement.Add(gameObjectText);
        }
        
        // 创建动作开始与结束
        private void CreateAnimationLabelEle(VisualElement parentElement, AnimationClip animation)
        {
            CreateStartTimeLabelElement(parentElement);
            CreateEndTimeLabelElement(parentElement, animation);
        }
        
        private void CreateStartTimeLabelElement(VisualElement parentElement)
        {
            var temp = new Label
            {
                style =
                {
                    marginLeft = 10,
                },
                text = ($"开始"),
            };
            parentElement.Add(temp);
            var gameObjectText = new Label
            {
                style =
                {
                    width = 30,
                    height = 20,
                    backgroundColor = new StyleColor(Color.gray),  // 设置背景颜色
                    unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                    justifyContent = Justify.Center,  // 垂直居中
                    marginLeft = 2,
                },
                text = "0",
            };
            parentElement.Add(gameObjectText);
            StartTime = gameObjectText;
        }
        
        private void CreateEndTimeLabelElement(VisualElement parentElement, AnimationClip animation)
        {
            var temp = new Label
            {
                style =
                {
                    marginLeft = 10,
                },
                text = ($"结束"),
            };
            parentElement.Add(temp);
            var gameObjectText = new Label
            {
                style =
                {
                    width = 30,
                    height = 20,
                    backgroundColor = new StyleColor(Color.gray),  // 设置背景颜色
                    unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                    justifyContent = Justify.Center,  // 垂直居中
                    marginLeft = 2,
                },
                text = animation.length.ToString(CultureInfo.CurrentCulture),
            };
            parentElement.Add(gameObjectText);
            EndTime = gameObjectText;
        }
        
        private void CreateAnimationTimeLineEle(VisualElement parentElement, AnimationClip animation)
        {
            var animationTimeLine = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    justifyContent = Justify.FlexStart,
                    backgroundColor = new StyleColor(HexToColor("#8B8B83")),
                    height = 40, // 设置固定高度
                    width = 900, // 设置固定高度
                }
            };
            parentElement.Add(animationTimeLine);
            CreateAnimationCursor(animationTimeLine, animation);
        }
        
        // 创建动作游标节点
        private void CreateAnimationCursor(VisualElement parentElement, AnimationClip animation)
        {
            CommonWidth = (900 / TimelineEditorWindow.TotalTimeInSeconds * animation.length);
            CurElement = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    width  = CommonWidth,
                    height = 40,
                    unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                    justifyContent = Justify.Center,  // 垂直居中
                    backgroundColor = new StyleColor(HexToColor("#40e0d0")),
                }
            };
            parentElement.Add(CurElement);
            // CreateTickMarkOutLine(CurElement, 0);
            // CreateTickMarkOutLine(CurElement, CommonWidth - 5);
            parentElement.RegisterCallback<MouseDownEvent>(OnMouseDown);
            parentElement.RegisterCallback<MouseUpEvent>(OnMouseUp);
            parentElement.RegisterCallback<MouseMoveEvent>((evt) => OnMouseMove(evt, CurElement));
        }
        
        private void CreateTickMarkOutLine(VisualElement parentElement, float marginLeft)
        {
            var element = new VisualElement
            {
                style =
                {
                    width  = 5,  // 每秒的宽度
                    height = 40,
                    unityTextAlign = TextAnchor.MiddleLeft,  // 水平居右
                    justifyContent = Justify.Center,  // 垂直居中
                    backgroundColor = new StyleColor(Color.white),
                    marginLeft = marginLeft,
                }
            };
            parentElement.Add(element);
        }
        
        public void Update()
        {
            CommonWidth = (900 / TimelineEditorWindow.TotalTimeInSeconds * AnimationClip.length);
            if (CurElement != null)
            {
                CurElement.style.width = CommonWidth;
            }
        }
        
        // 鼠标监听事件
        private void OnMouseDown(MouseDownEvent mouseDownEvent)
        {
            IsDragging = true;
        }
    
        private void OnMouseUp(MouseUpEvent evt)
        {
            IsDragging = false;
        }

        private void OnMouseMove(MouseMoveEvent evt, VisualElement element)
        {
            if (!IsDragging) return;
            var distance = evt.mousePosition.x - TimelineEditorWindow.TimelineTitleWidth;
            var oriPos = Mathf.Clamp(distance - CommonWidth / 2, 0, TimelineEditorWindow.TimelineWidth - CommonWidth);
            Debug.Log(oriPos);
            element.style.marginLeft = Mathf.Clamp(distance - CommonWidth / 2, 0, TimelineEditorWindow.TimelineWidth - CommonWidth);
            var oneSecWidth = TimelineEditorWindow.TimelineWidth / TimelineEditorWindow.TotalTimeInSeconds;
            StartTime.text = (Mathf.Round(oriPos / oneSecWidth * 100) / 100f).ToString(CultureInfo.CurrentCulture);
            EndTime.text = (Mathf.Round((oriPos + CommonWidth) / oneSecWidth * 100) / 100).ToString(CultureInfo.CurrentCulture);
        }
    }
}