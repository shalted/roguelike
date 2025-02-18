using System.Collections.Generic;
using Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TimelineEditorWindow : EditorWindow
{
    public const float TimelineWidth = 900;
    public const float TimelineTitleWidth = 300;
    private const float FrameRate = 60;  // 每秒帧数
    private const float UpdateInterval = 1f / FrameRate; // 更新间隔（30fps）
    
    public static float TotalTimeInSeconds = 10;
    
    private VisualElement root;
    private VisualElement _cursor;
    private VisualElement timeLineParentRoot;
    private ScrollView timelineScrollView;
    private bool _isPlaying;
    private float _currentTime;
    private Vector2 _scrollPosition;
    private float _lastUpdateTime; // 上次更新时间
 
    private Button _playPauseButton;
    private Label _timeDisplay;
    private Label _gameObjectText;
    private TextField _timesTextField;
    private AnimationClip _animation;
    private Animator _animator;
    private GameObject _selectedObject;
    private bool isDragging;
    private Vector2 dragStartPos;

    private List<AnimationLineClass> animationLineList = new List<AnimationLineClass>(); 
    
    [MenuItem("Window/SkillTimeLine")]
    public static void ShowWindow()
    {
        TimelineEditorWindow wnd = GetWindow<TimelineEditorWindow>();
        wnd.titleContent = new GUIContent("Timeline Editor");
        wnd.minSize = new Vector2(1200, 800); // 设置固定的最小大小
        wnd.maxSize = new Vector2(1200, 800); // 设置固定的最大大小
    }

    public void CreateGUI()
    {
        root = CreateRoot();
        var parameterInputBox = CreateParameterInputBoxRoot(root);
        CreateSelectGameObject(parameterInputBox);
        CreateTimesInputTextField(parameterInputBox);
        CreateTimesRefreshBtn(parameterInputBox);
        CreateDropArea(parameterInputBox);
        var timeLineRoot = CreateTimeLine(root);
        var timeButtonRoot = CreateTimeBtnEle(timeLineRoot);
        timeLineParentRoot = CreateTimeLineParent(timeLineRoot);
        CreateTimeText(timeButtonRoot);
        CreatePrevBtn(timeButtonRoot);
        CreatePlayBtn(timeButtonRoot);
        CreateNextBtn(timeButtonRoot);
        CreateTimeLineScroll(timeLineParentRoot);
        var timeLineLongitudinalRoot = CreateScrollViewContent();
        var timeLineHorizontalRoot = CreateScrollViewCursorContent(timeLineLongitudinalRoot);
        CreateTimeLineCursor(timeLineHorizontalRoot);
        CreateTimeLineTimeText(timeLineLongitudinalRoot);
        EditorApplication.update += OnEditorUpdate;
    }
    
    // 创建时间轴节点
    private VisualElement CreateRoot()
    {
        var timeLineRoot = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                justifyContent = Justify.FlexStart,
                height = 40, // 设置固定高度
                flexGrow = 1f,
            }
        };
        rootVisualElement.Add(timeLineRoot);
        return timeLineRoot;
    }
    
    // 创建输入文本框相关内容
    private static VisualElement CreateParameterInputBoxRoot(VisualElement parentElement)
    {
        var tempRoot = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.Center,  // 水平居中
                alignItems = Align.Center,
                height = 40, // 设置固定高度
                backgroundColor = new StyleColor(Color.black),
            }
        };
        parentElement.Add(tempRoot);
        return tempRoot;
    }
    
    // 创建当前选中gameObject
    private void CreateSelectGameObject(VisualElement parentElement)
    {
        var temp = new Label($"当前选中预制体:");
        parentElement.Add(temp);
        _gameObjectText = new Label
        {
            style =
            {
                width = 100,
                height = 30,
                backgroundColor = new StyleColor(Color.gray),  // 设置背景颜色
                unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                justifyContent = Justify.Center,  // 垂直居中
                marginLeft = 10,
            },
            text = $"null",
        };
        parentElement.Add(_gameObjectText);
    }
    
    // 创建文本输入框
    private void CreateTimesInputTextField(VisualElement parentElement)
    {
        var temp = new Label
        {
            style =
            {
                marginLeft = 20,
            },
            text = ($"时间轴长度:"),
        };
        parentElement.Add(temp);
        _timesTextField = new TextField
        {
            style =
            {
                width = 50,
                height = 20,
                justifyContent = Justify.FlexStart,
                backgroundColor = new StyleColor(Color.gray),  // 设置背景颜色
                unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                marginLeft = 10,
            }
        };
        parentElement.Add(_timesTextField);
    }
    
    // 创建刷新按钮
    private void CreateTimesRefreshBtn(VisualElement parentElement)
    {
        var tempButton = new Button();
        tempButton.clicked += OnclickRefreshBtn;
        var temp = new Label($"刷新");
        tempButton.Add(temp);
        parentElement.Add(tempButton);
    }
    
    // 按钮点击回调
    private void OnclickRefreshBtn()
    {
        Debug.Log(_timesTextField.text);
        TotalTimeInSeconds = int.Parse(_timesTextField.text);
        OnRefreshBtn();
    }
    
    // 点击刷新按钮
    private void OnRefreshBtn()
    {
        timeLineParentRoot.Clear();
        CreateTimeLineScroll(timeLineParentRoot);
        var timeLineLongitudinalRoot = CreateScrollViewContent();
        var timeLineHorizontalRoot = CreateScrollViewCursorContent(timeLineLongitudinalRoot);
        CreateTimeLineCursor(timeLineHorizontalRoot);
        CreateTimeLineTimeText(timeLineLongitudinalRoot);
        foreach (var animationLine in animationLineList)
        {
            animationLine.Update();
        }
    }
    
    // 创建拖拽区域
    private void CreateDropArea(VisualElement parentElement)
    {
        var dragArea = new VisualElement
        {
            style =
            {
                width = 150,
                height = 20,
                backgroundColor = new StyleColor(Color.gray),
                unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                justifyContent = Justify.Center,  // 垂直居中
                marginLeft = 10,
            }
        };
        dragArea.Add(new Label("Drag Resources Here"));
        parentElement.Add(dragArea);
        dragArea.RegisterCallback<DragUpdatedEvent>(evt =>
        {
            // 当鼠标悬停在拖拽区域时，改变拖拽指示符
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            evt.StopPropagation();
        });
        dragArea.RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
    }
    
    private void OnDragPerformEvent(DragPerformEvent evt)
    {
        // 接受拖拽并处理
        DragAndDrop.AcceptDrag(); // 这行代码接受拖拽
        // 当拖拽完成时，获取拖拽的资源
        if (DragAndDrop.objectReferences.Length > 0)
        {
            var draggedResources = "Dragged Resources: ";
            foreach (var draggedObject in DragAndDrop.objectReferences)
            {
                draggedResources += draggedObject.name + "\n";
                switch (draggedObject)
                {
                    case AnimationClip clip:
                    {
                        _animation = clip;
                        CreateAnimationLine(_animation);
                        break;
                    }
                    case GameObject gameObject:
                    {
                        _gameObjectText.text = $"{gameObject.transform.name}";
                        _animator = gameObject.GetComponent<Animator>();
                        _selectedObject = gameObject;
                        Debug.Log(gameObject.transform.name);
                        break;
                    }
                    case Effector2D effect:
                    {
                        CreateEffectLine(effect);
                        break;
                    }
                }
            }
            Debug.Log(draggedResources);
        }
        evt.StopPropagation();
    }

    // 创建时间轴节点
    private static VisualElement CreateTimeLine(VisualElement parentElement)
    {
        var timeLineRoot = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.FlexStart,
                backgroundColor = new StyleColor(Color.black),
                height = 60, // 设置固定高度
            }
        };
        parentElement.Add(timeLineRoot);
        return timeLineRoot;
    }
    
    // 创建按钮节点
    private static VisualElement CreateTimeBtnEle(VisualElement parentElement)
    {
        var timeButtonRoot = new VisualElement
        {
            style = { 
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.Center,  // 水平居中
                alignItems = Align.Center,
                backgroundColor = new StyleColor(Color.black),
                height = 60, // 设置固定高度
                width = 300, // 设置固定高度
            }
        };
        parentElement.Add(timeButtonRoot);
        return timeButtonRoot;
    }
    
    // 创建timeline父节点
    private static VisualElement CreateTimeLineParent(VisualElement parentElement)
    {
        var lineRoot = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                justifyContent = Justify.FlexStart,
                backgroundColor = new StyleColor(HexToColor("#8B8B83")),
                height = 60, // 设置固定高度
                width = TimelineWidth, // 设置固定高度
            }
        };
        parentElement.Add(lineRoot);
        return lineRoot;
    }
    
    // 添加时间显示文本
    private void CreateTimeText(VisualElement parentElement)
    {
        _timeDisplay = new Label
        {
            text = $"Time: {_currentTime:F2}s",
            style =
            {
                width = 100,
                height = 30,
                backgroundColor = new StyleColor(Color.gray),  // 设置背景颜色
                unityTextAlign = TextAnchor.MiddleCenter,  // 水平居中
                justifyContent = Justify.Center,  // 垂直居中
                marginLeft = 10,
            }
        };
        parentElement.Add(_timeDisplay);
    }
    
    // 创建上一帧按钮
    private void CreatePrevBtn(VisualElement parentElement)
    {
        var previousFrameButton = new Button(PreviousFrame)
        {
            text = "Prev",
            style =
            {
                width = 40,
                height = 30,
                marginLeft = 10,
            }
        };
        parentElement.Add(previousFrameButton);
    }
    
    // 创建播放、暂停按钮
    private void CreatePlayBtn(VisualElement parentElement)
    {
        _playPauseButton = new Button(TogglePlayPause)
        {
            text = "Play",
            style =
            {
                width = 40,
                height = 30,
                marginLeft = 10,
            }
        };
        parentElement.Add(_playPauseButton);
    }
    
    // 创建下一帧按钮
    private void CreateNextBtn(VisualElement parentElement)
    {
        var nextFrameButton = new Button(NextFrame)
        {
            text = "Next",
            style =
            {
                width = 40,
                height = 30,
                marginLeft = 10,
            }
        };
        parentElement.Add(nextFrameButton);
    }

    // 创建滚动条
    private void CreateTimeLineScroll(VisualElement parentElement)
    {
        timelineScrollView = new ScrollView(ScrollViewMode.Horizontal)
        {
            style =
            {
                height = 60,
                backgroundColor = new StyleColor(Color.black),
            },
            horizontalScroller =
            {
                style ={display = DisplayStyle.None},
            }
        };
        parentElement.Add(timelineScrollView);
    }
    
    private VisualElement CreateScrollViewContent()
    {
        var lineRoot = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                justifyContent = Justify.FlexStart,
                backgroundColor = new StyleColor(HexToColor("#8B8B83")),
                height = 80, // 设置固定高度
                width = TimelineWidth, // 设置固定高度
            }
        };
        timelineScrollView.Add(lineRoot);
        return lineRoot;
    }
    
    private static VisualElement CreateScrollViewCursorContent(VisualElement parentElement)
    {
        var lineRoot = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.FlexStart,
                backgroundColor = new StyleColor(HexToColor("#8B8B83")),
                height = 20,// 设置固定高度
                width = TimelineWidth, // 设置固定高度
            }
        };
        parentElement.Add(lineRoot);
        return lineRoot;
    }
    
    // 创建刻度显示
    private static void CreateTimeLineCursor(VisualElement parentElement)
    {
        for (var i = 0; i < TotalTimeInSeconds; i++)
        {
            var tickMark = new Label
            {
                text = (i + 1).ToString(),
                style =
                {
                    width  = (TimelineWidth / TotalTimeInSeconds),  // 每秒的宽度
                    height = 20,
                    flexDirection = FlexDirection.Column,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    backgroundColor = i % 2 == 0 ? new StyleColor(HexToColor("#4F4F4F")) : new StyleColor(HexToColor("#5c5c5c")),
                }
            };
            parentElement.Add(tickMark);
        }
    }
    
    // 创建游标
    private void CreateTimeLineTimeText(VisualElement parentElement)
    {
        _cursor = new VisualElement
        {
            style =
            {
                width = 2,
                height = 20,
                flexDirection = FlexDirection.Column,
                backgroundColor = Color.white,
                alignItems = Align.Center,
            }
        };
        parentElement.Add(_cursor);
    }
    
    private void OnDisable()
    {
        Debug.Log("Window is being disabled or closed.");
        _isPlaying = false;
    }
    
    // 播放与暂停状态值
    private void TogglePlayPause()
    {
        _isPlaying = !_isPlaying;
        if (_isPlaying)
        {
            AnimationMode.StartAnimationMode();
        }
        else
        {
            AnimationMode.StopAnimationMode();
        }
        _playPauseButton.text = _isPlaying ? "Pause" : "Play";
    }
    
    // 编辑器更新
    private void OnEditorUpdate()
    {
        if (!_isPlaying) return;
        PlayTime();
        PlayAnimation();
    }
    
    // 游标与时间显示更新
    private void PlayTime()
    {
        var currentTimeInEditor = Time.realtimeSinceStartup;
        // 检查是否需要更新
        if (currentTimeInEditor - _lastUpdateTime >= UpdateInterval)
        {
            _lastUpdateTime = currentTimeInEditor;

            // 更新当前时间
            _currentTime += UpdateInterval;
            if (_currentTime >= TotalTimeInSeconds)
            {
                _currentTime = 0; // 循环播放
            }

            // 更新游标位置
            var normalizedTime = _currentTime / TotalTimeInSeconds;
            var newLeft = normalizedTime * TimelineWidth; // 时间轴宽度
            _cursor.style.left = newLeft;
        }
        _timeDisplay.text = $"Time: {_currentTime:F2}s";
    }
    
    private void PlayAnimation()
    {
        if (_animator == null) return;
        foreach (var animationLine in animationLineList)
        {
            var realTime = _currentTime - float.Parse(animationLine.StartTime.text);
            if (realTime > 0 && realTime < animationLine.AnimationClip.length)
            {
                AnimationMode.SampleAnimationClip(_selectedObject, animationLine.AnimationClip, _currentTime - float.Parse(animationLine.StartTime.text));
            }
        }
    }

    // 更新当前游标位置
    private void UpdateCursor()
    {
        var normalizedTime = _currentTime / TotalTimeInSeconds;
        var newLeft = normalizedTime * TimelineWidth;
        _cursor.style.left = newLeft; 
        _timeDisplay.text = $"Time: {_currentTime:F2}s";
    }

    // 游标移动到上一帧
    private void PreviousFrame()
    {
        _currentTime = Mathf.Max(0, _currentTime - UpdateInterval);
        AnimationMode.StartAnimationMode();
        UpdateCursor();
        PlayAnimation();
    }

    // 游标移动到下一帧
    private void NextFrame()
    {
        _currentTime = Mathf.Min(TotalTimeInSeconds, _currentTime + UpdateInterval);
        AnimationMode.StartAnimationMode();
        UpdateCursor();
        PlayAnimation();
    }

    // 色码转色号
    private static Color HexToColor(string hex)
    {
        // 移除开头的 #
        hex = hex.Replace("#", "");

        // 如果长度不是 6 或 8，抛出异常
        if (hex.Length != 6 && hex.Length != 8)
            throw new System.ArgumentException("Invalid hex color code");

        // 解析颜色分量
        var r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        var g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        var b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        var a = hex.Length == 8 ? byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) : (byte)255;

        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
    
    // 新item添加，除了gameObject绑定的都在这里，做好内容区分，不同的内容实现也在这里，后续考虑拆分
    // 统一使用update调用过来
    
    // animation 动作执行模块
    // 创建animation line
    private void CreateAnimationLine(AnimationClip animation)
    {
        var animationLine = new AnimationLineClass();
        animationLine.CreateAnimationLine(root, animation, animationLineList.Count);
        animationLineList.Add(animationLine);
    }
    
    // effect 特效播放模块
    private static void CreateEffectLine(Effector2D effect)
    {
        Debug.Log(effect.name);
    }
    
}
