using CSharpMath.Atoms;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display {
  /// <summary>Corresponds to MTCTLineDisplay in iOSMath.</summary> 
  public class TextRunDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
      where TFont : MathFont<TGlyph> {
    public AttributedGlyphRun<TFont, TGlyph> Run { get; private set; }

    public TextRunDisplay(
      AttributedGlyphRun<TFont, TGlyph> run, 
      Range range, 
      TypesettingContext<TFont, TGlyph> context){
      var font = run.Font;
      Run = run;
      Range = range;
      
      var width = context.GlyphBoundsProvider.GetTypographicWidth(font, run);
      Width = (float)width;
      _ComputeAscentDescent(context, font);
    }
    private void _ComputeAscentDescent(TypesettingContext<TFont, TGlyph> context, TFont font) {
      var rects = context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(font, Run.Glyphs, Run.KernedGlyphs.Count());
      var tops = rects.Select(rect => rect.Bottom); // Convert to non-flipped naming here, 
      var bottoms = rects.Select(rect => rect.Y);
      float ascent = 0;
      float descent = 0;
      foreach (var top in tops) {
        ascent = Math.Max(ascent, top);
      }
      foreach (var bottom in bottoms) {
        descent = Math.Min(descent, bottom);
      }
      Ascent = ascent;
      Descent = descent;
    }
    public RectangleF DisplayBounds
      => this.ComputeDisplayBounds();

    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      context.SaveState();
      var text = Run.Text;
      var font = Run.Font;
      context.DrawGlyphRunWithOffset(Run, Position);
      context.RestoreState();
    }
    public Range Range { get; set; }
    public float Width { get; set; }
    public float Ascent { get; set; }
    public float Descent { get; set; }
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
  }
}
