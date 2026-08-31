using System.Drawing;
using CSharpMath;

namespace CSharpMath.Rendering.FrontEnd {
  using System.Runtime;
  using BackEnd;
  using Display;

  public abstract class MathPainter<TCanvas, TColor> : Painter<TCanvas, Atom.MathList, TColor> {
    protected bool _displayChanged = true;
    public override IDisplay<Fonts, Glyph>? Display { get; protected set; }
    protected override Atom.Result<Atom.MathList> LaTeXToContent(string latex) =>
      Atom.LaTeXParser.MathListFromLaTeX(latex);
    protected override string ContentToLaTeX(Atom.MathList mathList) =>
      Atom.LaTeXParser.MathListToLaTeX(mathList).ToString();
    public override RectangleF Measure(float unused = float.NaN) => base.Measure(unused);
    protected override void SetRedisplay() => _displayChanged = true;
    protected override void UpdateDisplayCore(float unused) {
      if (_displayChanged) {
        Display = Content == null
          ? null
          : Typesetter.CreateLine(Content, Fonts, TypesettingContext.Instance, LineStyle);
        _displayChanged = false;
      }
    }
    public override void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      var c = WrapCanvas(canvas);
      UpdateDisplay(float.NaN);
      var position = Display == null ? new PointF?() : Display.HasJoinRel()
        ? GetAlignedPosition(c, alignment, padding, offsetX, offsetY)
        : IPainterExtensions.GetDisplayPosition(Display.Width, Display.Ascent, Display.Descent,
          FontSize, c.Width, c.Height, alignment, padding, offsetX, offsetY);
      DrawCore(c, Display, position);
    }
    public void Draw(TCanvas canvas, float x, float y) {
      var c = WrapCanvas(canvas);
      UpdateDisplay(float.NaN);
      var inkLeft = Display?.HasJoinRel() == true ? Display.InkBounds().Left : 0;
      DrawCore(c, Display, new PointF(x - inkLeft, -y)); // x is the ink bounding-box origin; invert the canvas
    }
    private PointF GetAlignedPosition(ICanvas canvas, TextAlignment alignment, Thickness padding,
      float offsetX, float offsetY) {
      var bounds = Display!.InkBounds();
      var aligned = IPainterExtensions.GetDisplayPosition(
        bounds.Right - bounds.Left, Display.Ascent, Display.Descent, FontSize,
        canvas.Width, canvas.Height, alignment, padding, offsetX, offsetY);
      return new PointF(aligned.X - bounds.Left, aligned.Y);
    }
    /// <summary>
    /// Directly draw the given <paramref name="display"/>. Repositions the <paramref name="display"/>.
    /// </summary>
    public void DrawDisplay(IDisplay<Fonts, Glyph>? display, TCanvas canvas, float x, float y) {
      if (display is null) return;
      var original = (Display, _displayChanged);
      (Display, _displayChanged) = (display, false);
      Draw(canvas, x, y);
      (Display, _displayChanged) = original;
    }
    /// <summary>
    /// Directly draw the given <paramref name="display"/>. Repositions the <paramref name="display"/>.
    /// </summary>
    public void DrawDisplay(IDisplay<Fonts, Glyph>? display, TCanvas canvas,
      TextAlignment textAlignment = TextAlignment.Center,
      Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      if (display is null) return;
      var original = (Display, _displayChanged);
      (Display, _displayChanged) = (display, false);
      Draw(canvas, textAlignment, padding, offsetX, offsetY);
      (Display, _displayChanged) = original;
    }
    public new MathPainter<TCanvas, TColor> ShallowClone() => (MathPainter<TCanvas, TColor>)MemberwiseClone();
  }
}
