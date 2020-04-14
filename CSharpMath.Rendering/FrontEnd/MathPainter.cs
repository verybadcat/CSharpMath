using System.Drawing;

namespace CSharpMath.Rendering.FrontEnd {
  using Display;
  using BackEnd;
  using Structures;
  public abstract class MathPainter<TCanvas, TColor> : Painter<TCanvas, Atom.MathList, TColor> {
    protected bool _displayChanged = true;
    public override IDisplay<Fonts, Glyph>? Display { get; protected set; }
    protected override Result<Atom.MathList> LaTeXToContent(string latex) =>
      Atom.LaTeXParser.MathListFromLaTeX(latex);
    protected override string ContentToLaTeX(Atom.MathList mathList) =>
      Atom.LaTeXParser.MathListToLaTeX(mathList).ToString();
    public override RectangleF Measure(float unused = float.NaN) => base.Measure(unused);
    protected override void SetRedisplay() => _displayChanged = true;
    protected override void UpdateDisplayCore(float unused) {
      if (_displayChanged)
      {
        Display = Content == null
          ? null
          : Typesetter.CreateLine(Content, Fonts, TypesettingContext.Instance, LineStyle);
        _displayChanged = false;
      }
    }
    public override void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      var c = WrapCanvas(canvas);
      UpdateDisplay(float.NaN);
      DrawCore(c, Display, Display == null ? new PointF?() : IPainterExtensions.GetDisplayPosition(Display.Width, Display.Ascent, Display.Descent, FontSize, c.Width, c.Height, alignment, padding, offsetX, offsetY));
    }
    public void Draw(TCanvas canvas, float x, float y) {
      var c = WrapCanvas(canvas);
      UpdateDisplay(float.NaN);
      DrawCore(c, Display, new PointF(x, -y));
    }
    public void Draw(TCanvas canvas, PointF position) {
      var c = WrapCanvas(canvas);
      // Invert the canvas
      position.Y *= -1;
      UpdateDisplay(float.NaN);
      DrawCore(c, Display, position);
    }
  }
}