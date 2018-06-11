using System;
using System.Drawing;

using CSharpMath.Display;
using CSharpMath.Enumerations;
using CSharpMath.Rendering;
using CSharpMath.Atoms;
using CSharpMath.Interfaces;
using TFonts = CSharpMath.Rendering.MathFonts;
using CSharpMath.FrontEnd;
using CSharpMath.Structures;

using Glyph = Typography.OpenFont.Glyph;
using Typography.OpenFont;
using System.Collections.Generic;

namespace CSharpMath.Rendering {
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
  public class MathPainter {
    public MathPainter(SizeF bounds, float fontSize = 20f) {
      Bounds = bounds;
      FontSize = fontSize;
    }
    public MathPainter(float width, float height, float fontSize = 20f) {
      Bounds = new SizeF(width, height);
      FontSize = fontSize;
    }

    //_field == private field, __field == property-only field
    protected void Redisplay<T>(T assignment) => _displayChanged = true;
    protected bool _displayChanged = false;
    protected MathListDisplay<TFonts, Glyph> _displayList;
    protected GraphicsContext _skiaContext;

    public Action Invalidate { get; }

    public string ErrorMessage { get; private set; }
    public SizeF Bounds { get; set; }

    public TextAlignment TextAlignment { get; set; } = TextAlignment.Centre;

    /// <summary>
    /// Unit of measure: points;
    /// Defaults to <see cref="FontSize"/>.
    /// </summary>
    public float? ErrorFontSize { get; set; }
    public bool DisplayErrorInline { get; set; } = true;
    public Color ErrorColor { get; set; } = new Color(255, 0, 0);
    public Thickness Padding { get; set; }
    public Color TextColor { get; set; } = new Color(0, 0, 0);
    public Color BackgroundColor { get; set; } = new Color(0, 0, 0, 0);
    public PaintStyle PaintStyle { get; set; } = PaintStyle.Fill;
    public float ScrollX { get; set; }
    public float ScrollY { get; set; }
    public float Magnification { get; set; } = 1;

    public SizeF? DrawingSize {
      get {
        if (MathList != null && _displayList == null) UpdateDisplay();
        return _displayList?.ComputeDisplayBounds().Size + new SizeF(Padding.Left + Padding.Right, Padding.Top + Padding.Bottom);
      }
    }

    /// <summary>
    /// Unit of measure: points
    /// </summary>
    public float FontSize { get => __size; set => Redisplay(__size = value); } float __size = 20f;
    List<Typeface> __typefaces = new List<Typeface>(); public List<Typeface> LocalTypefaces { get => __typefaces; set => Redisplay(__typefaces = value); }
    LineStyle __style = LineStyle.Display; public LineStyle LineStyle { get => __style; set => Redisplay(__style = value); }
    (Color glyph, Color textRun)? __box; public (Color glyph, Color textRun)? GlyphBoxColor { get => __box; set => Redisplay(__box = value); }

    MathSource __source = new MathSource();
    public MathSource Source { get => __source; set { __source = value; _displayChanged = true; } }
    public IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }

    private PointF GetDisplayPosition() {
      float x, y;
      float displayWidth = _displayList.Width;
      if ((TextAlignment & TextAlignment.Left) != 0)
        x = Padding.Left;
      else if ((TextAlignment & TextAlignment.Right) != 0)
        x = Bounds.Width - Padding.Right - displayWidth;
      else
        x = Padding.Left + (Bounds.Width - Padding.Left - Padding.Right - displayWidth) / 2;
      float contentHeight = _displayList.Ascent + _displayList.Descent;
      if (contentHeight < FontSize / 2) {
        contentHeight = FontSize / 2;
      }
      //Canvas is inverted!
      if ((TextAlignment & TextAlignment.Top) != 0)
        y = Bounds.Height - Padding.Bottom - _displayList.Ascent;
      else if ((TextAlignment & TextAlignment.Bottom) != 0)
        y = Padding.Top + _displayList.Descent;
      else {
        float availableHeight = Bounds.Height - Padding.Top - Padding.Bottom;
        y = ((availableHeight - contentHeight) / 2) + Padding.Top + _displayList.Descent;
      }
      return new PointF(x, y);
    }

    public void UpdateDisplay() {
      var fonts = new TFonts(LocalTypefaces, FontSize);
      _displayList = fonts.TypesettingContext.CreateLine(MathList, fonts, LineStyle);
      _displayList.Position = GetDisplayPosition();
      _displayChanged = false;
    }

    public void Draw(ICanvas canvas) {
      if (MathList != null) {
        canvas.Save();
        //invert the canvas vertically
        canvas.Scale(1, -1);
        canvas.Translate(0, -Bounds.Height);
        canvas.Scale(Magnification, Magnification);
        canvas.DefaultColor = TextColor;
        canvas.CurrentColor = BackgroundColor;
        canvas.FillColor();
        if (_displayChanged) UpdateDisplay();
        canvas.Translate(ScrollX, ScrollY);
        var _context = new GraphicsContext() {
          Canvas = canvas,
          GlyphBoxColor = GlyphBoxColor
        };
        _displayList.Draw(_context);
        canvas.Restore();
      } else if (ErrorMessage.IsNonEmpty()) {
        canvas.Save();
        canvas.CurrentColor = BackgroundColor;
        canvas.FillColor();
        var size = ErrorFontSize ?? FontSize;
        if (DisplayErrorInline) {
          canvas.CurrentColor = ErrorColor;
          canvas.FillText(ErrorMessage, 0, size, size);
        }
        canvas.Restore();
      }
    }
  }
}