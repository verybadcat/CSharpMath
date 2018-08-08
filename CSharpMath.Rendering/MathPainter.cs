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

    protected override RectangleF? MeasureCore(float canvasWidth = float.NaN) =>
      _display?.ComputeDisplayBounds(!CoordinatesFromBottomLeftInsteadOfTopLeft);
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
    public void Draw(TCanvas canvas, float x, float y) => Draw(canvas, TextAlignment.TopLeft, default, x, y);
  }
}
