using System.Drawing;
using CSharpMath.Display;
using CSharpMath.FrontEnd;
using CSharpMath.Interfaces;

namespace CSharpMath.Rendering {
  public abstract class MathPainter<TCanvas, TPathWrapper, TColor> : Painter<TCanvas, TPathWrapper, MathSource, TColor> where TPathWrapper : IPath {
    public MathPainter(float fontSize = DefaultFontSize) : base(fontSize) { }

    protected IDisplay<Fonts, Glyph> _display;
    protected bool _displayChanged = true;

    public IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }

    protected override RectangleF? MeasureCore(float canvasWidth = float.NaN) =>
      _display?.ComputeDisplayBounds(CoordinatesFromBottomLeftInsteadOfTopLeft);
    public RectangleF? Measure {
      get {
        UpdateDisplay();
        return MeasureCore();
      }
    }

    protected override void SetRedisplay() => _displayChanged = true;
    protected override void UpdateDisplay(float canvasWidth = float.NaN) {
      if (_displayChanged && MathList != null) {
        _display = TypesettingContext.Instance.CreateLine(MathList, Fonts, LineStyle);
        _displayChanged = false;
      }
    }

    public override void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      var c = WrapCanvas(canvas);
      if (!Source.IsValid) DrawError(c);
      else {
        UpdateDisplay(c.Width);
        DrawCore(c, _display, IPainterExtensions.GetDisplayPosition(_display.Width, _display.Ascent, _display.Descent, FontSize, CoordinatesFromBottomLeftInsteadOfTopLeft, c.Width, c.Height, alignment, padding, offsetX, offsetY));
      }
    }

    public void Draw(TCanvas canvas, float x, float y) {
      var c = WrapCanvas(canvas);
      UpdateDisplay(c.Width);
      DrawCore(c, _display, new PointF(x, CoordinatesFromBottomLeftInsteadOfTopLeft ? y : -y));
    }

    public void Draw(TCanvas canvas, PointF position) {
      var c = WrapCanvas(canvas);
      if (CoordinatesFromBottomLeftInsteadOfTopLeft) position.Y *= -1;
      UpdateDisplay(c.Width);
      DrawCore(c, _display, position);
    }
  }
}