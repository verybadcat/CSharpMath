using System.Drawing;
using CSharpMath.Display;
using CSharpMath.FrontEnd;
using CSharpMath.Interfaces;

namespace CSharpMath.Rendering {
  public abstract class MathPainter<TCanvas, TColor> : Painter<TCanvas, MathSource, TColor> {
    public MathPainter(float fontSize = DefaultFontSize) : base(fontSize) { }

    protected IDisplay<Fonts, Glyph> _display;
    protected bool _displayChanged = true;

    public IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }

    public RectangleF? Measure => MeasureCore(ref _display, float.NaN);

    protected override void SetRedisplay() => _displayChanged = true;
    protected override void UpdateDisplay(ref IDisplay<Fonts, Glyph> display, float canvasWidth = float.NaN) {
      if (_displayChanged && MathList != null) {
        _display = TypesettingContext.Instance.CreateLine(MathList, Fonts, LineStyle);
        _displayChanged = false;
      }
    }

    public override void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      var c = WrapCanvas(canvas);
      if (!Source.IsValid) DrawError(c);
      else {
        UpdateDisplay(ref _display, c.Width);
        Draw(c, _display, IPainterExtensions.GetDisplayPosition(_display.Width, _display.Ascent, _display.Descent, FontSize, CoordinatesFromBottomLeftInsteadOfTopLeft, c.Width, c.Height, alignment, padding, offsetX, offsetY));
      }
    }

    public void Draw(TCanvas canvas, float x, float y) {
      var c = WrapCanvas(canvas);
      UpdateDisplay(ref _display, c.Width);
      Draw(c, _display, new PointF(x, CoordinatesFromBottomLeftInsteadOfTopLeft ? y : -y));
    }

    public void Draw(TCanvas canvas, PointF position) {
      var c = WrapCanvas(canvas);
      if (CoordinatesFromBottomLeftInsteadOfTopLeft) position.Y *= -1;
      UpdateDisplay(ref _display, c.Width);
      Draw(c, _display, position);
    }
  }
}
