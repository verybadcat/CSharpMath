using System;
using System.Drawing;

using CSharpMath.Display;
using CSharpMath.Enumerations;
using CSharpMath.Rendering;
using CSharpMath.Atoms;
using CSharpMath.Interfaces;
using TFont = CSharpMath.Rendering.MathFont;

using Glyph = Typography.OpenFont.Glyph;

using SkiaSharp;
using NColor = SkiaSharp.SKColor;
using NColors = SkiaSharp.SKColors;
using CSharpMath.FrontEnd;
using CSharpMath.Structures;

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
    public SkiaLatexPainter(SKSize bounds, float fontSize = 20f) {
      Bounds = bounds;
      FontSize = fontSize;
    }
    public SkiaLatexPainter(float width, float height, float fontSize = 20f) {
      Bounds = new SKSize(width, height);
      FontSize = fontSize;
    }

    //_field == private field, __field == property-only field
    protected void Redisplay<T>(T assignment) => _displayChanged = true;
    protected bool _displayChanged = false;
    protected MathListDisplay<TFont, Glyph> _displayList;
    protected GraphicsContext<SkiaPath> _skiaContext;
    protected static readonly TypesettingContext<TFont, Glyph> _typesettingContext = Typesetters.LatinMath;
    
    public Action Invalidate { get; }

    public string ErrorMessage { get; private set; }
    public SKSize Bounds { get; set; }

    TextAlignment __align = TextAlignment.Centre; public TextAlignment TextAlignment {
      get => __align;
      set {
        __align = value;
        OriginX = OriginY = null; //Reset origin using TextAlignment
      }
    }

    /// <summary>
    /// Unit of measure: points;
    /// Defaults to <see cref="FontSize"/>.
    /// </summary>
    public float? ErrorFontSize { get; set; }
    public bool DisplayErrorInline { get; set; } = true;
    public NColor ErrorColor { get; set; } = NColors.Red; 
    public Thickness Padding { get; set; }
    public NColor TextColor { get; set; } = NColors.Black; 
    public NColor BackgroundColor { get; set; } = NColors.Transparent; 
    public SKPaintStyle PaintStyle { get; set; } = SKPaintStyle.StrokeAndFill;
    /// <summary>
    /// Defults to <see cref="null"/>, which signals <see cref="UpdateOrigin"/> to update its value by calculating the text alignment.
    /// </summary>
    public float? OriginX { get; set; }
    /// <summary>
    /// Defults to <see cref="null"/>, which signals <see cref="UpdateOrigin"/> to update its value by calculating the text alignment.
    /// </summary>
    public float? OriginY { get; set; }
    public float Magnification { get; set; } = 1;

    private SKSize ToSKSize(SizeF size) => new SKSize(size.Width, size.Height);
    public SKSize? DrawingSize => _displayList == null ? default(SKSize?) :
      SKSize.Add(ToSKSize(_displayList.ComputeDisplayBounds().Size), new SKSize(Padding.Left + Padding.Right, Padding.Top + Padding.Bottom));

    /// <summary>
    /// Unit of measure: points
    /// </summary>
    public float FontSize { get => __size; set => Redisplay(__size = value); }
    float __size = 20f;
    LineStyle __style = LineStyle.Display; public LineStyle LineStyle { get => __style; set => Redisplay(__style = value); }
    (Color glyph, Color textRun)? __box; public (Color glyph, Color textRun)? GlyphBoxColor { get => __box; set => Redisplay(__box = value); }

    MathSource __source = new MathSource();
    public MathSource Source { get => __source; set { __source = value; OriginX = OriginY = null; _displayChanged = true; } }
    public IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }

    public void UpdateOrigin() {
      if (_displayList != null) {
        if (OriginX == null) {
          float displayWidth = _displayList.Width;
          if ((TextAlignment & TextAlignment.Left) != 0)
            OriginX = Padding.Left;
          else if ((TextAlignment & TextAlignment.Right) != 0)
            OriginX = Bounds.Width - Padding.Right - displayWidth;
          else
            OriginX = Padding.Left + (Bounds.Width - Padding.Left - Padding.Right - displayWidth) / 2;
        }
        if (OriginY == null) {
          float contentHeight = _displayList.Ascent + _displayList.Descent;
          if (contentHeight < FontSize / 2) {
            contentHeight = FontSize / 2;
          }
          //Canvas is inverted!
          if ((TextAlignment & TextAlignment.Top) != 0)
            OriginY = Bounds.Height - Padding.Bottom - _displayList.Ascent;
          else if ((TextAlignment & TextAlignment.Bottom) != 0)
            OriginY = Padding.Top + _displayList.Descent;
          else {
            float availableHeight = Bounds.Height - Padding.Top - Padding.Bottom;
            OriginY = ((availableHeight - contentHeight) / 2) + Padding.Top + _displayList.Descent;
          }
        }
      }
    }

    public void Draw(SKCanvas canvas) {
      if (MathList != null) {
        canvas.Save();
        //invert the canvas vertically
        canvas.Scale(1, -1);
        canvas.Translate(0, -Bounds.Height);
        canvas.Scale(Magnification);
        if (_displayChanged) {
          var fontSize = FontSize;
          var skiaFont = FontManager.LatinMath(fontSize);
          _displayList = _typesettingContext.CreateLine(MathList, skiaFont, LineStyle);
          _displayList.Position = new PointF(0, 0);
          _skiaContext = new GraphicsContext<SkiaPath>() {
            GlyphBoxColor = GlyphBoxColor
          };
          _displayList.Draw(_skiaContext);
          _displayChanged = false;
        }
        UpdateOrigin();
        canvas.Translate(OriginX.Value, OriginY.Value);
        canvas.DrawColor(BackgroundColor);
        var paths = _skiaContext.Paths;
        var paint = new SKPaint { IsStroke = false, StrokeCap = SKStrokeCap.Butt, Style = PaintStyle, IsAntialias = true };
        foreach (var (path, pos, color) in paths) {
          paint.Color = color.ToNative() ?? TextColor;
          canvas.Save();
          canvas.Translate(pos.X, pos.Y);
          canvas.DrawPath(path.Path, paint);
          canvas.Restore();
        }
        paint.Style = SKPaintStyle.Stroke;
        var boxPaths = _skiaContext.BoxPaths;
        foreach (var (path, color) in boxPaths) {
          paint.Color = color.ToNative();
          canvas.DrawPath(path.Path, paint);
        }
        var lines = _skiaContext.LinePaths;
        foreach (var (from, to, thickness, color) in lines) {
          paint.Color = color.ToNative() ?? TextColor;
          paint.StrokeWidth = thickness;
          canvas.DrawLine(from.X, from.Y, to.X, to.Y, paint);
        }
        canvas.Restore();
      } else if (ErrorMessage.IsNonEmpty()) {
        canvas.Save();
        canvas.DrawColor(BackgroundColor);
        var size = ErrorFontSize ?? FontSize;
        if(DisplayErrorInline) canvas.DrawText(ErrorMessage, 0, size,
          new SKPaint { Color = ErrorColor, Typeface = SKFontManager.Default.MatchCharacter('A'), TextSize = size });
        canvas.Restore();
      }
    }
  }
}