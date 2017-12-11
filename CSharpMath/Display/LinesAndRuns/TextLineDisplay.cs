using CSharpMath.Atoms;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CSharpMath.Display.Text;

namespace CSharpMath.Display {
  public class TextLineDisplay<TMathFont, TGlyph> : IDisplay<TGlyph>
    where TMathFont: MathFont<TGlyph> {
    public TextLineDisplay(List<TextRunDisplay<TMathFont, TGlyph>> runs,
      IEnumerable<IMathAtom> atoms) {
      Runs = runs;
      Atoms = atoms.ToList();
    }
    // We don't implement count as it's not clear if it would refer to runs or atoms.
    public List<TextRunDisplay<TMathFont, TGlyph>> Runs { get; }
    public List<IMathAtom> Atoms { get; }
    public List<TGlyph> Text {
      get {
        List<TGlyph> r = new List<TGlyph>();
        foreach (var run in Runs) {
          r.AddRange(run.Run.Text);
        }
        return r;
      }
    }

    public RectangleF DisplayBounds
      => this.ComputeDisplayBounds();

    public void Draw(IGraphicsContext<TGlyph> context) {

    }
    public PointF Position { get; set; }

    public float Ascent => Runs.CollectionAscent();
    public float Descent => Runs.CollectionDescent();
    public float Width => Runs.CollectionWidth();
    public Range Range => RangeExtensions.Combine(Runs.Select(r => r.Range));
    public bool HasScript { get; set; }
  }
}

