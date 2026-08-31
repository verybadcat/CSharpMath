using System.Drawing;
using System.Linq;
using CSharpMath.Atom;

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
      var rects = context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(
        font, Run.Glyphs, Run.GlyphInfos.Count).ToArray();
      Ascent = rects.IsEmpty() ? 0 : rects.Max(rect => rect.Bottom); // Convert to non-flipped naming here, 
      Descent = rects.IsEmpty() ? 0 : rects.Max(rect => -rect.Y);
      var advances = context.GlyphBoundsProvider.GetAdvancesForGlyphs(
        font, Run.Glyphs, Run.GlyphInfos.Count).Advances.ToArray();
      var x = 0f;
      var ink = RectangleF.Empty;
      for (var i = 0; i < Run.GlyphInfos.Count; i++) {
        var glyph = Run.GlyphInfos[i];
        var rect = rects[i];
        var positioned = new RectangleF(x + rect.X, rect.Y, rect.Width, rect.Height);
        if (!rect.IsEmpty) ink = ink.IsEmpty ? positioned : ink.Union(positioned);
        x += advances[i] + glyph.KernAfterGlyph;
      }
      InkBounds = ink;
    }
    public AttributedGlyphRun<TFont, TGlyph> Run { get; }
    internal RectangleF InkBounds { get; }

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
