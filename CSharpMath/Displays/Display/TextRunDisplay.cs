using CSharpMath.Atoms;
using CSharpMath.FrontEnd;
using Color = CSharpMath.Structures.Color;
using System.Drawing;
using System.Linq;

namespace CSharpMath.Displays.Display {
  /// <summary>Corresponds to MTCTLineDisplay in iOSMath.</summary> 
  public class TextRunDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph> where TFont : IFont<TGlyph> {
    public TextRunDisplay(
      AttributedGlyphRun<TFont, TGlyph> run, 
      Range range, 
      TypesettingContext<TFont, TGlyph> context){
      var font = run.Font;
      Run = run;
      Range = range;
      Width = context.GlyphBoundsProvider.GetTypographicWidth(font, run);
      // Compute ascent and descent
      var rects =
        context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(font, Run.Glyphs, Run.GlyphInfos.Count);
      Ascent = rects.Select(rect => rect.Bottom).Max(); // Convert to non-flipped naming here, 
      Descent = rects.Select(rect => -rect.Y).Max();
    }
    public AttributedGlyphRun<TFont, TGlyph> Run { get; }
    public RectangleF DisplayBounds => this.DisplayBounds();
    public Range Range { get; set; }
    public float Width { get; set; }
    public float Ascent { get; set; }
    public float Descent { get; set; }
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
    public Color? TextColor { get; set; }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      context.SaveState();
      context.DrawGlyphRunWithOffset(Run, Position, TextColor);
      context.RestoreState();
    }
    public void SetTextColorRecursive(Color? textColor) => TextColor ??= textColor;
    public override string ToString() => Run.Text.ToString();
  }
}