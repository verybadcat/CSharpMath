using CSharpMath.Atom;
using System.Drawing;
using System.Linq;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  /// <summary>Corresponds to MTCTLineDisplay in iOSMath.</summary> 
  public class TextRunDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph> where TFont : IFont<TGlyph> {
    public TextRunDisplay(
      AttributedGlyphRun<TFont, TGlyph> run, 
      Range range, 
      TypesettingContext<TFont, TGlyph> context) {
      var font = run.Font;
      Run = run;
      Range = range;
      Width = context.GlyphBoundsProvider.GetTypographicWidth(font, run);
      // Compute ascent and descent
      var rects =
        context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(font, Run.Glyphs, Run.GlyphInfos.Count);
      Ascent = rects.IsEmpty() ? 0 : rects.Max(rect => rect.Bottom); // Convert to non-flipped naming here, 
      Descent = rects.IsEmpty() ? 0 : rects.Max(rect => -rect.Y);
    }
    public AttributedGlyphRun<TFont, TGlyph> Run { get; }
    
    public Range Range { get; }
    public float Width { get; }
    public float Ascent { get; }
    public float Descent { get; }
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      context.SaveState();
      context.DrawGlyphRunWithOffset(Run, Position, TextColor);
      context.RestoreState();
    }
    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) => TextColor ??= textColor;
    public Color? BackColor { get; set; }
    public override string ToString() => Run.Text.ToString();
  }
}