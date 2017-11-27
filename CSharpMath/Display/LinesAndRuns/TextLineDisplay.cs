using CSharpMath.Atoms;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display {
  public class TextLineDisplay : IDisplay {
    public TextLineDisplay(List<TextRunDisplay> runs,
      IEnumerable<IMathAtom> atoms) {
      Runs = runs;
      Atoms = atoms.ToList();
    }
    public List<TextRunDisplay> Runs { get; }
    public List<IMathAtom> Atoms { get; }
    public string Text {
      get {
        string r = "";
        foreach (var run in Runs) {
          r += run.Run.Text;
        }
        return r;
      }
    }

    public RectangleF DisplayBounds
      => this.OriginBoundsFromAscentDescentWidth();

    public void Draw(IGraphicsContext context) {

    }

    public float Ascent => throw new NotImplementedException();
    public float Descent => throw new NotImplementedException(); // TODO: runs probably need a location.
    public float Width => throw new NotImplementedException(); // again, probably need a location on runs
    public Range Range {
      get {
        if (Runs.IsEmpty()) {
          return Ranges.NotFound;
        }
        int start = int.MaxValue;
        int end = 0;
        foreach (var run in Runs) {
          start = Math.Min(start, run.Range.Location);
          end = Math.Max(end, run.Range.End);
        }
        return new Range(start, end - start);
      }
    }
  }
}
