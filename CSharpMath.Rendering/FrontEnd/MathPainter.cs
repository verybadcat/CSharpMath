using System.Drawing;

namespace CSharpMath.Rendering.FrontEnd {
  using Display;
  using BackEnd;
  using Structures;
  public abstract class MathPainter<TCanvas, TColor> : Painter<TCanvas, Atom.MathList, TColor> {
    protected IDisplay<Fonts, Glyph>? _display;
    protected bool _displayChanged = true;
    public override IDisplay<Fonts, Glyph>? Display => _display;
    public override string? LaTeX {
      get => Content is null ? "" : Atom.LaTeXParser.MathListToLaTeX(Content).ToString();
      set => (Content, ErrorMessage) = Atom.LaTeXParser.TryMathListFromLaTeX(value ?? "");
    }
    public override RectangleF? Measure(float unused = float.NaN) {
      UpdateDisplay();
      return _display?.DisplayBounds();
    }
    protected override void SetRedisplay() => _displayChanged = true;
    protected void UpdateDisplay() {
      if (_displayChanged && Content != null) {
        _display = Typesetter.CreateLine(Content, Fonts, TypesettingContext.Instance, LineStyle);
        _displayChanged = false;
      }
    }
    public override void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      var c = WrapCanvas(canvas);
      if (ErrorMessage is { }) DrawError(c);
      else {
        UpdateDisplay();
        DrawCore(c, _display, _display == null ? new PointF?() : IPainterExtensions.GetDisplayPosition(_display.Width, _display.Ascent, _display.Descent, FontSize, CoordinatesFromBottomLeftInsteadOfTopLeft, c.Width, c.Height, alignment, padding, offsetX, offsetY));
      }
    }
    public void Draw(TCanvas canvas, float x, float y) {
      var c = WrapCanvas(canvas);
      UpdateDisplay();
      DrawCore(c, _display, new PointF(x, CoordinatesFromBottomLeftInsteadOfTopLeft ? y : -y));
    }
    public void Draw(TCanvas canvas, PointF position) {
      var c = WrapCanvas(canvas);
      if (CoordinatesFromBottomLeftInsteadOfTopLeft) position.Y *= -1;
      UpdateDisplay();
      DrawCore(c, _display, position);
    }
  }
}