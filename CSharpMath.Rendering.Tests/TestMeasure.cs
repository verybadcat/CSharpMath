namespace CSharpMath.Rendering.Tests {
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Linq;
  using BackEnd;
  using CSharpMath.Display.FrontEnd;
  using CSharpMath.Display.Displays;
  using CSharpMath.Display;
  using CSharpMath.Editor;
  using CSharpMath.Rendering.FrontEnd;
  using Xunit;

  public class TestMeasure {
    class D : Display.IDisplay<Fonts, Glyph> {
      public float Ascent => 12;
      public float Descent => 3;
      public float Width => 10;

      public PointF Position { get => PointF.Empty; set => throw new NotImplementedException(); }
      public Atom.Range Range => throw new NotImplementedException();
      public Color? TextColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public Color? BackColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public bool HasScript { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public void Draw(IGraphicsContext<Fonts, Glyph> context) => throw new NotImplementedException();
      public void SetTextColorRecursive(Color? textColor) => throw new NotImplementedException();
    }
    class DKeyboard : Editor.MathKeyboard<Fonts, Glyph> {
      public DKeyboard() : base(TypesettingContext.Instance, new Fonts(Enumerable.Empty<Typography.OpenFont.Typeface>(), 0.0f)) =>
        Display = new Display.Displays.ListDisplay<Fonts, Glyph>(new[] { new D() });
    }
    class DRenderingMath : SkiaSharp.MathPainter {
      public DRenderingMath() =>
        Display = new Display.Displays.ListDisplay<Fonts, Glyph>(new[] { new D() });
      protected override void UpdateDisplayCore(float unused) { }
    }
    class DRenderingText : SkiaSharp.TextPainter {
      public DRenderingText() =>
        Display = new Display.Displays.ListDisplay<Fonts, Glyph>(new[] { new D() });
      protected override void UpdateDisplayCore(float canvasWidth) { }
    }
    class DRenderingKeyboard : FrontEnd.MathKeyboard {
      public DRenderingKeyboard() =>
        Display = new Display.Displays.ListDisplay<Fonts, Glyph>(new[] { new D() });
    }

    sealed class GlyphDisplay : Display.IGlyphDisplay<Fonts, Glyph> {
      public GlyphDisplay(int location, float ascent, float descent) {
        Range = new Atom.Range(location, 1);
        Ascent = ascent;
        Descent = descent;
      }
      public float Ascent { get; }
      public float Descent { get; }
      public float Width => 10;
      public Atom.Range Range { get; }
      public PointF Position { get; set; }
      public Color? TextColor { get; set; }
      public Color? BackColor { get; set; }
      public bool HasScript { get; set; }
      public float ShiftDown { get; set; }
      public Fonts Font => throw new NotImplementedException();
      public void Draw(IGraphicsContext<Fonts, Glyph> context) => throw new NotImplementedException();
      public void SetTextColorRecursive(Color? textColor) => TextColor = textColor;
    }

    sealed class RecordingPath : Path {
      public readonly List<PointF> Points = new();
      public override Color? Foreground { get; set; }
      public override void MoveTo(float x0, float y0) => Points.Add(new(x0, y0));
      public override void LineTo(float x1, float y1) => Points.Add(new(x1, y1));
      public override void Curve3(float x1, float y1, float x2, float y2) { }
      public override void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) { }
      public override void CloseContour() { }
      public override void Dispose() { }
    }

    sealed class RecordingCanvas : ICanvas {
      public RecordingPath? LastPath { get; private set; }
      public float Width => 100;
      public float Height => 100;
      public Color DefaultColor { get; set; }
      public Color? CurrentColor { get; set; }
      public PaintStyle CurrentStyle { get; set; }
      public Path StartNewPath() => LastPath = new RecordingPath();
      public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness) { }
      public void StrokeRect(float left, float top, float width, float height) { }
      public void FillRect(float left, float top, float width, float height) { }
      public void Save() { }
      public void Translate(float dx, float dy) { }
      public void Scale(float sx, float sy) { }
      public void Restore() { }
    }

    sealed class CaretKeyboard : FrontEnd.MathKeyboard {
      public void SetDisplay(Display.Displays.ListDisplay<Fonts, Glyph> display) => Display = display;
    }

    static (CaretKeyboard keyboard, RecordingCanvas canvas) MakeCaretKeyboard(
      Display.Displays.ListDisplay<Fonts, Glyph> display, MathListIndex index) {
      var keyboard = new CaretKeyboard { InsertionIndex = index };
      keyboard.SetDisplay(display);
      var canvas = new RecordingCanvas();
      keyboard.DrawCaret(canvas, Color.Black, CaretShape.IBeam);
      return (keyboard, canvas);
    }

    [Fact]
    public void CaretUsesLevelZeroDisplayBounds() {
      var display = new ListDisplay<Fonts, Glyph>(new Display.IDisplay<Fonts, Glyph>[] {
        new GlyphDisplay(0, 8, 2)
      });
      var result = MakeCaretKeyboard(display, new MathListIndex(0));
      Assert.Equal(new[] { 0f, 3f, -9f, -9f, 3f }, result.canvas.LastPath!.Points.Select(p => p.Y));
    }

    [Fact]
    public void EmptyEditorUsesFontFallbackBounds() {
      var display = new ListDisplay<Fonts, Glyph>(Array.Empty<Display.IDisplay<Fonts, Glyph>>());
      var result = MakeCaretKeyboard(display, new MathListIndex(0));
      var ascent = result.keyboard.Font.PointSize * 2 / 3;
      Assert.Equal(new[] { 1f, -ascent - 1, -ascent - 1, 1f },
        result.canvas.LastPath!.Points.Skip(1).Select(p => p.Y));
    }

    [Fact]
    public void TerminalCaretUsesNearestRegularChild() {
      var tall = new GlyphDisplay(0, 14, 4);
      var trailingScript = new ListDisplay<Fonts, Glyph>(new Display.IDisplay<Fonts, Glyph>[] {
        new GlyphDisplay(0, 3, 1)
      }) {
        LinePosition = LinePosition.Superscript, IndexInParent = 0, Position = new PointF(10, 8)
      };
      var display = new ListDisplay<Fonts, Glyph>(new Display.IDisplay<Fonts, Glyph>[] { tall, trailingScript });
      var result = MakeCaretKeyboard(display, new MathListIndex(1));
      Assert.Equal(new[] { 5f, -15f, -15f, 5f },
        result.canvas.LastPath!.Points.Skip(1).Select(p => p.Y));
    }

    [Theory]
    [InlineData(MathListSubIndexType.Superscript, 4f, 1f)]
    [InlineData(MathListSubIndexType.Subscript, 3f, 2f)]
    public void CaretUsesScriptDisplayBounds(MathListSubIndexType type, float ascent, float descent) {
      var scriptGlyph = new GlyphDisplay(0, ascent, descent);
      var script = new ListDisplay<Fonts, Glyph>(new Display.IDisplay<Fonts, Glyph>[] { scriptGlyph }) {
        LinePosition = type == MathListSubIndexType.Superscript ? LinePosition.Superscript : LinePosition.Subscript,
        IndexInParent = 0,
        Position = new PointF(0, type == MathListSubIndexType.Superscript ? 5 : -5)
      };
      var display = new ListDisplay<Fonts, Glyph>(new Display.IDisplay<Fonts, Glyph>[] {
        new GlyphDisplay(0, 8, 2), script
      });
      var index = new MathListIndex(0, (type, new MathListIndex(0)));
      var result = MakeCaretKeyboard(display, index);
      var baseline = -script.Position.Y;
      Assert.Equal(new[] { baseline, baseline + descent + 1, baseline - ascent - 1, baseline - ascent - 1, baseline + descent + 1 },
        result.canvas.LastPath!.Points.Select(p => p.Y));
    }

    [Fact]
    public void CaretUsesInnermostNestedScriptBounds() {
      var inner = new ListDisplay<Fonts, Glyph>(new Display.IDisplay<Fonts, Glyph>[] { new GlyphDisplay(0, 2, 1) }) {
        LinePosition = LinePosition.Subscript, IndexInParent = 0, Position = new PointF(0, -3)
      };
      var outer = new ListDisplay<Fonts, Glyph>(new Display.IDisplay<Fonts, Glyph>[] { new GlyphDisplay(0, 5, 2), inner }) {
        LinePosition = LinePosition.Superscript, IndexInParent = 0, Position = new PointF(0, 6)
      };
      var display = new ListDisplay<Fonts, Glyph>(new Display.IDisplay<Fonts, Glyph>[] { new GlyphDisplay(0, 8, 2), outer });
      var index = new MathListIndex(0, (MathListSubIndexType.Superscript,
        new MathListIndex(0, (MathListSubIndexType.Subscript, new MathListIndex(0)))));
      var result = MakeCaretKeyboard(display, index);
      var baseline = -(outer.Position.Y + inner.Position.Y);
      Assert.Equal(new[] { baseline, baseline + 2, baseline - 3, baseline - 3, baseline + 2 },
        result.canvas.LastPath!.Points.Select(p => p.Y));
    }

    [Fact]
    public void UpArrowGeometryIsUnchanged() {
      var display = new ListDisplay<Fonts, Glyph>(new Display.IDisplay<Fonts, Glyph>[] { new GlyphDisplay(0, 8, 2) });
      var keyboard = new CaretKeyboard { InsertionIndex = new MathListIndex(0) };
      keyboard.SetDisplay(display);
      var canvas = new RecordingCanvas();
      keyboard.DrawCaret(canvas, Color.Black, CaretShape.UpArrow);
      var arrowHeight = keyboard.Font.PointSize * 2 / 3;
      Assert.Equal(new[] { arrowHeight / 4, arrowHeight, arrowHeight, arrowHeight / 4 },
        canvas.LastPath!.Points.Skip(1).Select(p => p.Y));
    }
    /// <summary>
    /// CSharpMath uses the mathematical coordinate system,
    /// i.e. the rectangle position is at the bottom-left.
    /// </summary>
    [Fact]
    public void CoreMeasure_YIsNegDescent() {
      Assert.Equal(new RectangleF(0, -3, 10, 15), new D().DisplayBounds());
      Assert.Equal(new RectangleF(0, -3, 10, 15), new DKeyboard().Measure);
    }
    /// <summary>
    /// CSharpMath.Rendering and descendants use the graphical coordinate system,
    /// i.e. the rectangle position is at the top-left.
    /// </summary>
    [Fact]
    public void RenderingMeasure_YIsNegAscent() {
      Assert.Equal(new RectangleF(0, -12, 10, 15), new DRenderingMath().Measure());
      Assert.Equal(new RectangleF(0, -12, 10, 15), new DRenderingText().Measure(float.NaN));
      Assert.Equal(new RectangleF(0, -12, 10, 15), new DRenderingKeyboard().Measure);
    }
  }
}
