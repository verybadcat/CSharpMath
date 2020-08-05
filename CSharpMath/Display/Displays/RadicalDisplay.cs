using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  public class RadicalDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    ///<summary>A display representing the radicand of the radical.
    ///Its position is relative to the parent and it is not treated as a sub-display.</summary>
    public ListDisplay<TFont, TGlyph> Radicand { get; }
    ///<summary>A display representing the degree of the radical.
    ///Its position is relative to the parent and it is not treated as a sub-display.</summary>
    public ListDisplay<TFont, TGlyph>? Degree { get; private set; }
    public float Width { get; set; }
    public float TopKern { get; set; }
    ///<summary>The thickness of the top bar of the radical.</summary>
    public float LineThickness { get; set; }
    private float _radicalShift;
    private readonly IDisplay<TFont, TGlyph> _radicalGlyph;
    public RadicalDisplay(ListDisplay<TFont, TGlyph> innerDisplay, IGlyphDisplay<TFont, TGlyph> glyph, PointF position, Range range) {
      Radicand = innerDisplay;
      _radicalGlyph = glyph;
      Position = position;
      Range = range;
    }
    public void SetDegree(ListDisplay<TFont, TGlyph> degree, TFont degreeFont, FontMathTable<TFont, TGlyph> degreeFontMathTable) {
      var kernBefore = degreeFontMathTable.RadicalKernBeforeDegree(degreeFont);
      Degree = degree;
      _radicalShift = kernBefore + degree.Width +
        degreeFontMathTable.RadicalKernAfterDegree(degreeFont);
      if (_radicalShift < 0) {
        kernBefore -= _radicalShift;
        _radicalShift = 0;
      }

      // Position of degree is relative to parent.
      Degree.Position =
        new PointF(Position.X + kernBefore, Position.Y +
          degreeFontMathTable.RadicalDegreeBottomRaise(degreeFont) * (Ascent - Descent));
      // update the width by the _radicalShift
      Width = _radicalShift + _radicalGlyph.Width + Radicand.Width;
      UpdateRadicandPosition();
    }
    private void UpdateRadicandPosition() =>
      Radicand.Position =
        new PointF(Position.X + _radicalShift + _radicalGlyph.Width, Position.Y);
    
    public float Ascent { get; set; }
    public float Descent { get; set; }
    public Range Range { get; }
    PointF _position;
    public PointF Position { get => _position; set { _position = value; UpdateRadicandPosition(); } }
    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      Radicand.Draw(context);
      Degree?.Draw(context);
      context.SaveState();
      var translation = new PointF(Position.X + _radicalShift, Position.Y);
      context.Translate(translation);
      context.SetTextPosition(new PointF());
      _radicalGlyph.Draw(context);
      // Draw the VBOX
      // for the kern of, we don't need to draw anything.
      float heightFromTop = TopKern;
      // draw the horizontal line with the given thickness
      var lineStart = new PointF(_radicalGlyph.Width, Ascent - heightFromTop - LineThickness / 2);
      var lineEnd = new PointF(lineStart.X + Radicand.Width, lineStart.Y);
      context.DrawLine(lineStart.X, lineStart.Y, lineEnd.X, lineEnd.Y, LineThickness, TextColor);
      context.RestoreState();
    }
    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) {
      TextColor ??= textColor;
      _radicalGlyph.SetTextColorRecursive(TextColor);
      Radicand?.SetTextColorRecursive(textColor);
      Degree?.SetTextColorRecursive(textColor);
    }
    public Color? BackColor { get; set; }
    public override string ToString() => $@"\sqrt[{Degree}]{{{Radicand}}}";
  }
}
