using System;
using System.Drawing;

using CSharpMath.Display;
using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using CSharpMath.Atoms;
using CSharpMath.Interfaces;
using TFont = CSharpMath.SkiaSharp.SkiaMathFont;

using Glyph = Typography.OpenFont.Glyph;

using SkiaSharp;
using NColor = SkiaSharp.SKColor;
using NColors = SkiaSharp.SKColors;

namespace CSharpMath.SkiaSharp {
  public readonly struct Thickness {
    public Thickness(float uniformSize) { Left = Right = Top = Bottom = uniformSize; }
    public Thickness(float horizontalSize, float verticalSize) { Left = Right = horizontalSize; Top = Bottom = verticalSize; }
    public Thickness(float left, float top, float right, float bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }

    public float Top { get; }
    public float Bottom { get; }
    public float Left { get; }
    public float Right { get; }

    public void Deconstruct(out float left, out float top, out float right, out float bottom) =>
      (left, top, right, bottom) = (Left, Top, Right, Bottom);
  }
  public class SkiaLatexPainter {
    public SkiaLatexPainter(Action invalidate, SKSize bounds, float fontSize = 20f) {
      Invalidate = invalidate ?? throw new ArgumentNullException(nameof(invalidate));
      Bounds = bounds;
      FontSize = fontSize;
    }
    public SkiaLatexPainter(Action invalidate, float width, float height, float fontSize = 20f) {
      Invalidate = invalidate ?? throw new ArgumentNullException(nameof(invalidate));
      Bounds = new SKSize(width, height);
      FontSize = fontSize;
    }

    protected static readonly TypesettingContext<TFont, Glyph> _typesettingContext = SkiaTypesetters.LatinMath;
    protected MathListDisplay<TFont, Glyph> _displayList;

    public Action Invalidate { get; }
    public SKSize Bounds { get; set; }
    public Thickness Padding { get; set; } = new Thickness();
    public string ErrorMessage { get; private set; }
    public bool DisplayErrorInline { get; set; } = true;
    /// <summary>
    /// Unit of measure: points
    /// </summary>
    public float FontSize { get; set; } = 20f;
    /// <summary>
    /// Unit of measure: points;
    /// Defaults to <see cref="FontSize"/>.
    /// </summary>
    public float? ErrorFontSize { get; set; } = null;
    public NColor TextColor { get; set; } = NColors.Black;
    public NColor BackgroundColor { get; set; } = new NColor();
    public NColor ErrorColor { get; set; } = NColors.Red;
    public ColumnAlignment TextAlignment { get; set; } = ColumnAlignment.Left;
    public SKPaintStyle PaintStyle { get; set; } = SKPaintStyle.StrokeAndFill;
    public bool DrawGlyphBoxes { get; set; }

    private SKSize ToSKSize(SizeF size) => new SKSize(size.Width, size.Height);
    public SKSize DrawingSize => _displayList == null ? Bounds :
      SKSize.Add(ToSKSize(_displayList.ComputeDisplayBounds().Size), new SKSize(Padding.Left + Padding.Right, Padding.Top + Padding.Bottom));

    private IMathList _mathList;
    public IMathList MathList {
      get => _mathList;
      set {
        _mathList = value ?? new MathList();
        _latex = MathListBuilder.MathListToString(_mathList);
        Invalidate();
      }
    }

    private string _latex;
    public string LaTeX {
      get => _latex;
      set {
        _latex = value ?? "";
        var buildResult = MathLists.BuildResultFromString(_latex);
        _mathList = buildResult.MathList;
        ErrorMessage = buildResult.Error;
        Invalidate();
      }
    }

    public void InitPositions() {
      if (_mathList != null) {
        var fontSize = FontSize;
        var skiaFont = SkiaFontManager.LatinMath(fontSize);
        _displayList = _typesettingContext.CreateLine(_mathList, skiaFont, LineStyle.Display);
        float displayWidth = _displayList.Width;
        float textX = 0;
        switch (TextAlignment) {
          case ColumnAlignment.Left:
            textX = Padding.Left;
            break;
          case ColumnAlignment.Center:
            textX = Padding.Left + (Bounds.Width - Padding.Left - Padding.Right - displayWidth) / 2;
            break;
          case ColumnAlignment.Right:
            textX = Bounds.Width - Padding.Right - displayWidth;
            break;
        }
        float availableHeight = Bounds.Height - Padding.Top - Padding.Bottom;
        float contentHeight = _displayList.Ascent + _displayList.Descent;
        if (contentHeight < FontSize / 2) {
          contentHeight = FontSize / 2;
        }
        float textY = ((availableHeight - contentHeight) / 2) + Padding.Bottom + _displayList.Descent;
        _displayList.Position = new PointF(textX, textY);
      }
    }

    public void Draw(SKCanvas canvas) {
      if (_mathList != null) {
        InitPositions();
        var skiaContext = new SkiaGraphicsContext() {
          Canvas = canvas,
          Color = TextColor,
          PaintStyle = PaintStyle,
          DrawGlyphBoxes = DrawGlyphBoxes
        };
        canvas.Save();
        //invert the canvas vertically
        canvas.Scale(1, -1);
        canvas.Translate(0, -Bounds.Height);
        canvas.DrawColor(BackgroundColor);
        _displayList.Draw(skiaContext);
        canvas.Restore();
      } else if (ErrorMessage.IsNonEmpty()) {
        canvas.Save();
        canvas.DrawColor(BackgroundColor);
        canvas.DrawText(ErrorMessage, new SKPoint(0, Bounds.Height - ErrorFontSize ?? FontSize), new SKPaint { Color = ErrorColor, Typeface = SKFontManager.Default.MatchCharacter('A') });
        canvas.Restore();
      }
    }
  }
}
