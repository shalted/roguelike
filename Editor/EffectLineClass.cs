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
        public float EffectLength { get; private set; }
        private float CommonWidth { get; set; }
        private bool IsDragging { get; set; }
        private VisualElement CurElement { get; set; }
        private GameObject EffectClip { get; set; }
        private ParticleSystem ParticleSystem { get; set; }
        public VisualEffect VisualEffect { get; private set; }
        private Label EndTime { get; set; }
        public Label StartTime { get; private set; }
        private GameObject EffectObj { get; set; }
        
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
            CommonWidth = (900 / TimelineEditorWindow.TotalTimeInSeconds * EffectLength);
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
        
        
        // 外部方法
        public void Play(float currentTime)
        {
            var realTime = currentTime - float.Parse(StartTime.text);
            Debug.Log("当前的时间：" + realTime + " " + EffectLength);
            if (realTime > 0 && realTime < EffectLength)
            {
                Debug.Log("播放特效：" + realTime);
                ParticleSystem.Simulate(realTime, true);
                ParticleSystem.Play();
                SceneView.RepaintAll();
            }
            else
            {
                //ParticleSystem.Stop();
                //Object.DestroyImmediate(EffectObj);
            }
        }

        ~EffectLineClass()
        {
            Debug.Log("特效资源被回收");
        }
    }
}