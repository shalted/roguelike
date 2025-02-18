using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class TimelineEditor : EditorWindow
{
    private Label timeLabel;
    private ScrollView timelineScrollView;
    private Button playPauseButton;
    private Button previousFrameButton;
    private Button nextFrameButton;
    private float currentTime = 0f;
    private float maxTime = 100f; // 假设最大时间为100秒
    private bool isPlaying = false;
    private float scaleFactor = 1.0f; // 初始缩放因子

    [MenuItem("Window/Timeline Editor")]
    public static void ShowWindow()
    {
        GetWindow<TimelineEditor>("Timeline Editor");
    }

    private void OnEnable()
    {
        var root = rootVisualElement;

        // 时间显示区域
        timeLabel = new Label($"Time: {currentTime}");
        root.Add(timeLabel);

        // 播放/暂停按钮
        playPauseButton = new Button(TogglePlayPause) { text = "Play" };
        root.Add(playPauseButton);

        // 上一帧按钮
        previousFrameButton = new Button(PreviousFrame) { text = "Previous Frame" };
        root.Add(previousFrameButton);

        // 下一帧按钮
        nextFrameButton = new Button(NextFrame) { text = "Next Frame" };
        root.Add(nextFrameButton);

        // 时间轴滚动视图
        timelineScrollView = new ScrollView(ScrollViewMode.Horizontal);
        timelineScrollView.style.height = 50;
        timelineScrollView.style.flexGrow = 1;
        timelineScrollView.style.paddingBottom = 10;
        timelineScrollView.style.paddingLeft = 10;
        timelineScrollView.style.paddingRight = 10;
        timelineScrollView.style.paddingTop = 10;
        root.Add(timelineScrollView);

        // 创建时间轴内容
        CreateTimelineContent();

        // 添加滚轮缩放
        timelineScrollView.RegisterCallback<WheelEvent>(OnScrollWheel);

        // 设置布局
        root.style.flexDirection = FlexDirection.Column;
        root.style.flexGrow = 1;
    }

    private void TogglePlayPause()
    {
        isPlaying = !isPlaying;
        playPauseButton.text = isPlaying ? "Pause" : "Play";

        // 实际播放逻辑的实现（这里可以使用协程或计时器）
    }

    private void PreviousFrame()
    {
        currentTime = Mathf.Max(currentTime - 1f, 0f);
        UpdateTimeDisplay();
    }

    private void NextFrame()
    {
        currentTime = Mathf.Min(currentTime + 1f, maxTime);
        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay()
    {
        timeLabel.text = $"Time: {currentTime:F2}";
    }

    private void CreateTimelineContent()
    {
        timelineScrollView.Clear(); // 清除旧的时间轴内容

        // 创建时间轴条
        var timelineBar = new VisualElement();
        timelineBar.style.height = 50;
        timelineBar.style.flexDirection = FlexDirection.Row;
        timelineBar.style.flexGrow = 1;
        timelineBar.style.flexShrink = 0;
        timelineScrollView.Add(timelineBar);

        // 绘制时间轴刻度
        for (int i = 0; i <= maxTime; i += 10)  // 每10秒一个刻度
        {
            var tick = new VisualElement();
            tick.style.width = 20;
            tick.style.borderLeftColor = Color.gray;
            tick.style.borderLeftWidth = 1;
            tick.style.marginLeft = 10;
            tick.style.marginRight = 10;

            var label = new Label(i.ToString());
            label.style.marginTop = 5;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            tick.Add(label);

            timelineBar.Add(tick);
        }
    }

    private void OnScrollWheel(WheelEvent evt)
    {
        Debug.Log(evt.delta.y);
        scaleFactor += evt.delta.y * -0.1f; // 缩放因子
        scaleFactor = Mathf.Clamp(scaleFactor, 0.1f, 10f); // 限制缩放范围

        // 获取时间轴条（确保滚动视图的第一个子元素是时间轴条）
        var timelineBar = GetFirstChild();
        Debug.Log(timelineBar);
        if (timelineBar != null)
        {
            timelineBar.style.flexBasis = new Length(1000 * scaleFactor, LengthUnit.Pixel); // 设置缩放后的宽度
            timelineBar.style.width = new Length(1000 * scaleFactor, LengthUnit.Pixel); // 确保宽度更新
        }

        CreateTimelineContent();
    }
    
    private VisualElement GetFirstChild()
    {
        foreach (var child in timelineScrollView.Children())
        {
            return child;
        }
        return null;
    }
}
