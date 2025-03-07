using UnityEngine;
using UnityEngine.UIElements;
using System.Globalization;
using UnityEditor;
using UnityEngine.VFX;

namespace Editor
{
    public class EffectLineClass:TimeLineBaseClass
    {
        private int LineCount { get; set; }
        private float EffectLength { get; set; }
        private float CommonWidth { get; set; }
        private bool IsDragging { get; set; }
        private VisualElement CurElement { get; set; }
        private GameObject EffectClip { get; set; }
        private ParticleSystem ParticleSystem { get; set; }
        private VisualEffect VisualEffect { get; set; }
        private Label EndTime { get; set; }
        private Label StartTime { get; set; }
        private GameObject EffectObj { get; set; }
        private VisualElement _curMoveElement;
        private Vector2 dragOffset;
        private float oriX;
        
        
        public bool CreateEffectLine(VisualElement root, GameObject effectObj, int lineCount, GameObject parentObj)
        {
            if (parentObj == null)
            {
                return false;
            }
            EffectObj = PrefabUtility.InstantiatePrefab(effectObj) as GameObject;
            if (EffectObj == null)
            {
                return false;
            }
            FormatEffectComp(EffectObj);
            EffectObj.transform.position = Vector3.zero;
            EffectObj.transform.parent = parentObj.transform;
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
            EffectClip = effectObj;
            CreateEffectShowEle(animationLine, EffectClip);
            CreateEffectTimeLineEle(animationLine);
            root.Add(animationLine);
            return true;
        }
        
        private void FormatEffectComp(GameObject effectObj)
        {
            ParticleSystem = effectObj.GetComponentInChildren<ParticleSystem>(true);
            VisualEffect = effectObj.GetComponentInChildren<VisualEffect>(true);
            if (ParticleSystem)
            {
                EffectLength = ParticleSystem.main.loop ? Mathf.Infinity : ParticleSystem.main.duration;
            }
            else
            {
                if (VisualEffect)
                {
                    EffectLength = VisualEffect.GetFloat("Duration");
                }
            }
            ParticleSystem.Simulate(EffectLength / 2, true);
            ParticleSystem.Play();
            AnimationMode.StartAnimationMode();
            SceneView.RepaintAll();
        }
        
        private void CreateEffectShowEle(VisualElement parentElement, GameObject effectObj)
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
            CreateEffectNameEle(timeButtonRoot, effectObj);
            CreateEffectLabelEle(timeButtonRoot);
            parentElement.Add(timeButtonRoot);
        }
        
        // 动作名称
        private void CreateEffectNameEle(VisualElement parentElement, GameObject effectObj)
        {
            var temp = new Label($"特效");
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
                text = effectObj.name,
            };
            parentElement.Add(gameObjectText);
        }
        
        // 创建动作开始与结束
        private void CreateEffectLabelEle(VisualElement parentElement)
        {
            CreateStartTimeLabelElement(parentElement);
            CreateEndTimeLabelElement(parentElement);
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
        
        private void CreateEndTimeLabelElement(VisualElement parentElement)
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
                text = EffectLength.ToString(CultureInfo.CurrentCulture),
            };
            parentElement.Add(gameObjectText);
            EndTime = gameObjectText;
        }
        
        private void CreateEffectTimeLineEle(VisualElement parentElement)
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
            CreateEffectCursor(animationTimeLine);
        }
        
        // 创建动作游标节点
        private void CreateEffectCursor(VisualElement parentElement)
        {
            CommonWidth = (900 / TimelineEditorWindow.TotalTimeInSeconds * EffectLength);
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
            parentElement.RegisterCallback<MouseDownEvent>((evt) => OnMouseDown(evt, CurElement));
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
            CommonWidth = (900 / TimelineEditorWindow.TotalTimeInSeconds * EffectLength);
            if (CurElement != null)
            {
                CurElement.style.width = CommonWidth;
            }
        }
        
        // 鼠标监听事件
        private void OnMouseDown(MouseDownEvent mouseDownEvent, VisualElement element)
        {
            Debug.Log("鼠标点击按下");
            IsDragging = true;
            dragOffset = TimelineEditorWindow.Evt.mousePosition;
            _curMoveElement = element;
            EditorApplication.update += OnDragUpdate;
        }

        private void OnDragUpdate()
        {
            var evt = TimelineEditorWindow.Evt;
            if (evt is { type: EventType.MouseUp })
            {
                IsDragging = false;
                EditorApplication.update -= OnDragUpdate;
                return;
            }

            if (evt is not { type: EventType.MouseDrag } || !IsDragging) return;
            var mousePos = evt.mousePosition;
            var moveX = mousePos.x - dragOffset.x;
            dragOffset = evt.mousePosition;
            var oriPos = Mathf.Clamp(oriX + moveX, 0, TimelineEditorWindow.TimelineWidth - CommonWidth);
            oriX = oriPos;
            _curMoveElement.style.marginLeft = oriPos;
            var oneSecWidth = TimelineEditorWindow.TimelineWidth / TimelineEditorWindow.TotalTimeInSeconds;
            StartTime.text = (Mathf.Round(oriPos / oneSecWidth * 100) / 100f).ToString(CultureInfo.CurrentCulture);
            EndTime.text = (Mathf.Round((oriPos + CommonWidth) / oneSecWidth * 100) / 100).ToString(CultureInfo.CurrentCulture);
        }
        
        // 外部方法
        public void Play(float currentTime)
        {
            var realTime = currentTime - float.Parse(StartTime.text);
            if (!(realTime > 0) || !(realTime < EffectLength)) return;
            ParticleSystem.Simulate(realTime, true);
            ParticleSystem.Play();
            SceneView.RepaintAll();
            EditorApplication.QueuePlayerLoopUpdate();
            EditorUtility.SetDirty(ParticleSystem);
        }

        ~EffectLineClass()
        {
            Debug.Log("特效资源被回收");
        }
    }
}