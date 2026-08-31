using System.Drawing;
using System.Linq;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  /// <summary>Corresponds to MTCTLineDisplay in iOSMath.</summary> 
  public class TextRunDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>, IInkDisplay where TFont : IFont<TGlyph> {
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
      InkWidth = Width;
      if (font is IFontGlyphBounds<TGlyph> bounds) {
        var x = 0f;
        var glyphRects = bounds.GetBoundingRects(run.Glyphs).ToList();
        var advances = bounds.GetAdvances(run.Glyphs).ToList();
        for (var i = 0; i < glyphRects.Count; i++) {
          InkWidth = System.Math.Max(InkWidth, x + glyphRects[i].Right);
          x += (i < advances.Count ? advances[i] : 0) + (i < run.GlyphInfos.Count ? run.GlyphInfos[i].KernAfterGlyph : 0);
        }
      }
    }
    public AttributedGlyphRun<TFont, TGlyph> Run { get; }

    public Range Range { get; }
    public float Width { get; }
    public float InkWidth { get; }
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
