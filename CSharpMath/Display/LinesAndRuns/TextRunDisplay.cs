using CSharpMath.Atoms;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using CSharpMath.Interfaces;
using Color = CSharpMath.Structures.Color;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display {
  /// <summary>Corresponds to MTCTLineDisplay in iOSMath.</summary> 
  public class TextRunDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph> where TFont : IFont<TGlyph> {
    public AttributedGlyphRun<TFont, TGlyph> Run { get; }

    public TextRunDisplay(
      AttributedGlyphRun<TFont, TGlyph> run, 
      Range range, 
      TypesettingContext<TFont, TGlyph> context){
      var font = run.Font;
      Run = run;
      Range = range;
      
      Width = context.GlyphBoundsProvider.GetTypographicWidth(font, run);
      _ComputeAscentDescent(context, font);
    }
    private void _ComputeAscentDescent(TypesettingContext<TFont, TGlyph> context, TFont font) {
      var rects = context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(font, Run.Glyphs.AsForEach(), Run.GlyphInfos.Count);
      var tops = rects.Select(rect => rect.Bottom); // Convert to non-flipped naming here, 
      var bottoms = rects.Select(rect => rect.Y);
      float ascent = 0;
      float descent = 0;
      foreach (var top in tops) {
        ascent = Math.Max(ascent, top);
      }
      foreach (var bottom in bottoms) {
        descent = Math.Max(descent, -bottom);
      }
      Ascent = ascent;
      Descent = descent;
    }
    public RectangleF DisplayBounds => this.ComputeDisplayBounds();

    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      context.SaveState();
      context.DrawGlyphRunWithOffset(Run, Position, TextColor);
      context.RestoreState();
    }
    public Range Range { get; set; }
    public float Width { get; set; }
    public float Ascent { get; set; }
    public float Descent { get; set; }
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
    public Color? TextColor { get; set; }

    public void SetTextColorRecursive(Color? textColor) =>
      TextColor = TextColor ?? textColor;

    public override string ToString() => Run.Text.ToString();
  }
}
