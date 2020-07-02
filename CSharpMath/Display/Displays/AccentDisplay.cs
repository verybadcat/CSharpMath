using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  public class AccentDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph> where TFont : IFont<TGlyph> {
    public AccentDisplay(GlyphDisplay<TFont, TGlyph> accentGlyphDisplay, IDisplay<TFont, TGlyph> accentee) {
      Accent = accentGlyphDisplay;
      Accentee = accentee;
    }
    ///<summary>A display representing the inner list that is accented.
    ///Its position is relative to the parent and it is not treated as a sub-display.</summary>
    public IDisplay<TFont, TGlyph> Accentee { get; }
    ///<summary>A display representing the accent.
    ///Its position is relative to the current display.</summary>
    public GlyphDisplay<TFont, TGlyph> Accent { get; }

    public float Ascent => System.Math.Max(Accent.Ascent + Accent.Position.Y, Accentee.Ascent);

    public float Descent => System.Math.Max(Accent.Descent + Accent.Position.Y, Accentee.Descent);

    public float Width => System.Math.Max(Accent.Width + Accent.Position.X, Accentee.Width);

    public Range Range => Accentee.Range;

    public PointF Position { get => Accentee.Position; set => Accentee.Position = value; }
    
    public bool HasScript { get; set; }

    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      Accentee.Draw(context);
      context.SaveState();
      context.Translate(Position);
      context.SetTextPosition(new PointF());
      Accent.Draw(context);
      context.RestoreState();
    }

    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) {
      TextColor ??= textColor;
      Accentee.SetTextColorRecursive(textColor);
      Accent.SetTextColorRecursive(textColor);
    }
    public Color? BackColor { get; set; }

    public override string ToString() => $@"\accent{{{Accent}}}{{{Accentee}}}";
  }
}