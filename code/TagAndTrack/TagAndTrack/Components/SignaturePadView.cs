using System.Text;
using System.Text.Json;
using Microsoft.Maui.Graphics;

namespace TagAndTrack.Components
{
    /// <summary>
    /// A handwritten signature pad built on MAUI's GraphicsView.
    /// Captures touch/drag strokes and can export them as JSON byte[].
    /// Also provides a static helper to render stored signatures for display.
    /// </summary>
    public class SignaturePadView : GraphicsView, IDrawable
    {
        private readonly List<List<PointF>> _strokes = new();
        private List<PointF>? _currentStroke;

        public Color StrokeColor { get; set; } = Colors.Black;
        public float StrokeWidth { get; set; } = 3f;

        /// <summary>True when no strokes have been drawn.</summary>
        public bool IsBlank => _strokes.Count == 0 && _currentStroke == null;

        public SignaturePadView()
        {
            Drawable = this;
            BackgroundColor = Colors.White;

            // GraphicsView interaction events handle both touch (mobile)
            // and mouse (desktop) input
            StartInteraction += OnStartInteraction;
            DragInteraction += OnDragInteraction;
            EndInteraction += OnEndInteraction;
            CancelInteraction += OnCancelInteraction;
        }

        /// <summary>
        /// Walks up the visual tree and toggles ScrollView scrolling.
        /// Disabling scroll while drawing prevents the page from moving under the finger.
        /// </summary>
        private void SetParentScrollEnabled(bool enabled)
        {
            Element? current = this.Parent;
            while (current != null)
            {
                if (current is ScrollView sv)
                {
                    sv.InputTransparent = !enabled;
                    // On iOS/Android the most reliable way is to toggle orientation
                    sv.Orientation = enabled
                        ? ScrollOrientation.Vertical
                        : ScrollOrientation.Neither;
                    break;
                }
                current = (current as VisualElement)?.Parent ?? (current as Element)?.Parent;
            }
        }

        private void OnStartInteraction(object? sender, TouchEventArgs e)
        {
            if (e.Touches.Length == 0) return;
            SetParentScrollEnabled(false);
            _currentStroke = new List<PointF> { e.Touches[0] };
            Invalidate();
        }

        private void OnDragInteraction(object? sender, TouchEventArgs e)
        {
            if (_currentStroke == null || e.Touches.Length == 0) return;
            _currentStroke.Add(e.Touches[0]);
            Invalidate();
        }

        private void OnEndInteraction(object? sender, TouchEventArgs e)
        {
            if (_currentStroke != null && _currentStroke.Count > 0)
            {
                _strokes.Add(_currentStroke);
            }
            _currentStroke = null;
            SetParentScrollEnabled(true);
            Invalidate();
        }

        private void OnCancelInteraction(object? sender, EventArgs e)
        {
            // Touch was cancelled (e.g. system gesture) — commit partial stroke and re-enable scroll
            if (_currentStroke != null && _currentStroke.Count > 0)
            {
                _strokes.Add(_currentStroke);
            }
            _currentStroke = null;
            SetParentScrollEnabled(true);
            Invalidate();
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Background
            canvas.FillColor = BackgroundColor ?? Colors.White;
            canvas.FillRectangle(dirtyRect);

            canvas.StrokeColor = StrokeColor;
            canvas.StrokeSize = StrokeWidth;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;

            // Draw completed strokes
            foreach (var stroke in _strokes)
                DrawStroke(canvas, stroke);

            // Draw in-progress stroke
            if (_currentStroke != null)
                DrawStroke(canvas, _currentStroke);
        }

        private static void DrawStroke(ICanvas canvas, List<PointF> points)
        {
            if (points.Count < 2) return;
            for (int i = 1; i < points.Count; i++)
            {
                canvas.DrawLine(points[i - 1], points[i]);
            }
        }

        /// <summary>Clears all strokes.</summary>
        public void Clear()
        {
            _strokes.Clear();
            _currentStroke = null;
            Invalidate();
        }

        /// <summary>
        /// Serializes all strokes to a JSON byte array.
        /// Format: float[][][] — array of strokes, each stroke is array of [X,Y] points.
        /// </summary>
        public byte[] GetSignatureBytes()
        {
            var data = _strokes.Select(stroke =>
                stroke.Select(p => new float[] { p.X, p.Y }).ToArray()
            ).ToArray();

            var json = JsonSerializer.Serialize(data);
            return Encoding.UTF8.GetBytes(json);
        }

        // =====================================================================
        //  Static helper: render stored signature data for display-only views
        // =====================================================================

        /// <summary>
        /// Creates a non-interactive GraphicsView that renders the stored signature.
        /// Returns null if signatureData is null/empty or cannot be parsed.
        /// </summary>
        public static GraphicsView? CreateSignatureDisplay(byte[]? signatureData,
            double width = 300, double height = 120)
        {
            if (signatureData == null || signatureData.Length == 0)
                return null;

            try
            {
                var json = Encoding.UTF8.GetString(signatureData);
                var strokes = JsonSerializer.Deserialize<float[][][]>(json);
                if (strokes == null || strokes.Length == 0)
                    return null;

                var view = new GraphicsView
                {
                    WidthRequest = width,
                    HeightRequest = height,
                    BackgroundColor = Colors.White,
                    Drawable = new SignatureDisplayDrawable(strokes),
                    InputTransparent = true
                };
                return view;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Drawable that renders stored stroke data read-only.</summary>
        private class SignatureDisplayDrawable : IDrawable
        {
            private readonly float[][][] _strokes;

            public SignatureDisplayDrawable(float[][][] strokes) => _strokes = strokes;

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                canvas.FillColor = Colors.White;
                canvas.FillRectangle(dirtyRect);

                if (_strokes.Length == 0) return;

                // Find bounding box
                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;
                foreach (var stroke in _strokes)
                    foreach (var pt in stroke)
                    {
                        if (pt[0] < minX) minX = pt[0];
                        if (pt[1] < minY) minY = pt[1];
                        if (pt[0] > maxX) maxX = pt[0];
                        if (pt[1] > maxY) maxY = pt[1];
                    }

                float dataW = maxX - minX;
                float dataH = maxY - minY;
                if (dataW < 1) dataW = 1;
                if (dataH < 1) dataH = 1;

                float pad = 10f;
                float scaleX = (dirtyRect.Width - pad * 2) / dataW;
                float scaleY = (dirtyRect.Height - pad * 2) / dataH;
                float scale = Math.Min(scaleX, scaleY);

                float offX = pad + (dirtyRect.Width - pad * 2 - dataW * scale) / 2;
                float offY = pad + (dirtyRect.Height - pad * 2 - dataH * scale) / 2;

                canvas.StrokeColor = Colors.Black;
                canvas.StrokeSize = 2;
                canvas.StrokeLineCap = LineCap.Round;
                canvas.StrokeLineJoin = LineJoin.Round;

                foreach (var stroke in _strokes)
                {
                    if (stroke.Length < 2) continue;
                    for (int i = 1; i < stroke.Length; i++)
                    {
                        float x1 = (stroke[i - 1][0] - minX) * scale + offX;
                        float y1 = (stroke[i - 1][1] - minY) * scale + offY;
                        float x2 = (stroke[i][0] - minX) * scale + offX;
                        float y2 = (stroke[i][1] - minY) * scale + offY;
                        canvas.DrawLine(x1, y1, x2, y2);
                    }
                }
            }
        }
    }
}
