using CSharpMath.Atoms;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CSharpMath.Display.Text;

namespace CSharpMath.Display {
  public class TextLineDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont: MathFont<TGlyph> {
    public TextLineDisplay(List<TextRunDisplay<TFont, TGlyph>> runs,
      IEnumerable<IMathAtom> atoms) {
      Runs = runs;
      Atoms = atoms.ToList();
    }
    // We don't implement count as it's not clear if it would refer to runs or atoms.
    public List<TextRunDisplay<TFont, TGlyph>> Runs { get; }
    public List<IMathAtom> Atoms { get; }
    public List<TGlyph> Text {
      get {
        List<TGlyph> r = new List<TGlyph>();
        foreach (var run in Runs) {
          r.AddRange(run.Run.KernedGlyphs.Select(g => g.Glyph).ToArray());
        }
        return r;
      }
    }

    public RectangleF DisplayBounds
      => this.ComputeDisplayBounds();

    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      context.SaveState();
      context.SetTextPosition(this.Position);
      foreach (var run in Runs) {
        run.Draw(context);
      }
      context.RestoreState();
    }
    public PointF Position { get; set; }

    public float Ascent => Runs.CollectionAscent();
    public float Descent => Runs.CollectionDescent();
    public float Width => Runs.CollectionWidth();
    public Range Range => RangeExtensions.Combine(Runs.Select(r => r.Range));
    public bool HasScript { get; set; }
  }
}

