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

namespace CSharpMath.SkiaSharp
{
  public readonly struct Thickness {
    public Thickness(float uniformSize) { Left = Right = Top = Bottom = uniformSize; }
    public Thickness(float horizontalSize, float verticalSize) { Left = Right = horizontalSize; Top = Bottom = verticalSize; }
    public Thickness(float left, float top, float right, float bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }

    public float Top { get; }
    public float Bottom { get; }
    public float Left { get; }
    public float Right { get; }
  }
  public class SkiaLatexPainter {
    public SkiaLatexPainter(SKSizeI bounds, float fontSize = 20f) : this(new SizeF(bounds.Width, bounds.Height), fontSize) { }
    public SkiaLatexPainter(SizeF bounds, float fontSize = 20f) {
      Bounds = bounds;
      FontSize = fontSize;
    }

    protected MathListDisplay<TFont, Glyph> _displayList;
    protected SKCanvas _canvas;
    protected readonly TypesettingContext<TFont, Glyph> _typesettingContext = SkiaTypesetters.LatinMath;

    public SizeF Bounds { get; set; }
    public SKSizeI BoundsSK { set => Bounds = new SizeF(value.Width, value.Height); }
    public Thickness Margin { get; set; } = new Thickness();
    public string ErrorMessage { get; private set; }
    public bool DisplayErrorInline { get; set; } = true;
    public float FontSize { get; set; } = 20f;
    public float? ErrorFontSize { get; set; } = null;
    public NColor TextColor { get; set; } = NColors.Black;
    public NColor BackgroundColor { get; set; } = new NColor(230, 230, 230);
    public NColor ErrorColor { get; set; } = NColors.Red;
    public ColumnAlignment TextAlignment { get; set; } = ColumnAlignment.Left;
    
    public SizeF DrawingSize => _displayList == null ? Bounds :
      SizeF.Add(_displayList.ComputeDisplayBounds().Size, new SizeF(Margin.Left + Margin.Right, Margin.Top + Margin.Bottom));

    private IMathList _mathList;
    public IMathList MathList {
      get => _mathList;
      set {
        _mathList = value;
        _latex = MathListBuilder.MathListToString(value);
        Redraw();
      }
    }

    private string _latex;
    public string LaTeX {
      get => _latex;
      set {
        _latex = value;
        var buildResult = MathLists.BuildResultFromString(value);
        _mathList = buildResult.MathList;
        ErrorMessage = buildResult.Error;
        if (_mathList != null) {
          var fontSize = FontSize;
          var skiaFont = SkiaFontManager.LatinMath(fontSize);
          _displayList = _typesettingContext.CreateLine(_mathList, skiaFont, LineStyle.Display);
        }
        Redraw();
      }
    }

    public void InitPositions() {
      if (_mathList != null) {
        float displayWidth = _displayList.Width;
        float textX = 0;
        switch (TextAlignment) {
          case ColumnAlignment.Left:
            textX = Margin.Left;
            break;
          case ColumnAlignment.Center:
            textX = Margin.Left + (Bounds.Width - Margin.Left - Margin.Right - displayWidth) / 2;
            break;
          case ColumnAlignment.Right:
            textX = Bounds.Width - Margin.Right - displayWidth;
            break;
        }
        float availableHeight = Bounds.Height - Margin.Top - Margin.Bottom;
        float contentHeight = _displayList.Ascent + _displayList.Descent;
        if (contentHeight < FontSize / 2) {
          contentHeight = FontSize / 2;
        }
        float textY = ((availableHeight - contentHeight) / 2) + Margin.Bottom + _displayList.Descent;
        _displayList.Position = new PointF(textX, textY);
      }
    }

    public void Redraw() => Draw(_canvas);
    public void Draw(SKCanvas canvas) {
      _canvas = canvas;
      if (_mathList != null) {
        InitPositions();
        var skiaContext = new SkiaGraphicsContext() {
          Canvas = canvas
        };
        canvas.Save();
        canvas.DrawColor(BackgroundColor);
        skiaContext.Color = TextColor;
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
