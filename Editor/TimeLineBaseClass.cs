using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class TimeLineBaseClass:VisualElement
    {
        private float _value;
        private Vector2 _dragStartPos;
        private float _dragStartValue;
        private bool _isDragging;
        
        public event System.Action<float> OnValueChanged;

        protected static Color HexToColor(string hex)
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
    }
}