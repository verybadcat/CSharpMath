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
  /// <summary>Corresponds to MTCTLineDisplay in iOSMath. Will need to
  /// figure out Core Text a bit in order to fill this out.</summary> 
  public class TextRunDisplay<TMathFont, TGlyph> : IDisplay<TGlyph>
      where TMathFont : MathFont<TGlyph> {
    public AttributedGlyphRun<TMathFont, TGlyph> Run { get; private set; }

    public TextRunDisplay(
      AttributedGlyphRun<TMathFont, TGlyph> run, 
      Range range, 
      TypesettingContext<TMathFont, TGlyph> context){
      var font = run.Font;
      Run = run;
      Range = range;
      
      var bounds = context.GlyphBoundsProvider.GetBoundingRectForGlyphs(font, run.Text);
      Width = bounds.Width;
      Ascent = Math.Max(0, bounds.YMax());
      Descent = Math.Max(0, -bounds.Y);
    }
    public RectangleF DisplayBounds
      => this.ComputeDisplayBounds();

    public void Draw(IGraphicsContext<TGlyph> context) {
      context.SaveState();
      var glyphs = Run.Text;
      var font = Run.Font;
      context.DrawGlyphsAtPoint(glyphs, font, this.Position);
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
