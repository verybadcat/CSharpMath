using CSharpMath.Atoms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display {
  /// <summary>
  /// Corresponds to MTMathListDisplay in iosMath.
  /// </summary>
  public class MathListDisplay: IDisplay {
    public IDisplay[] Displays { get; set; }
    public LinePosition MyLinePosition { get; set; }
    public Color TextColor { get; set; }
    public bool HasScript { get; set; }
    /// <summary> Recursively. While translating, we'll keep the iosMath name "setTextColor".</summary> 
    public void SetTextColor(Color textColor) {
      TextColor = textColor;
      foreach (var atom in Displays) {
        atom.TextColor = textColor;
      }
    }
    /// <summary>For a subscript or superscript, this is the index in the
    /// parent list. For a regular list, it is int.MinValue.</summary>
    public int IndexInParent { get; set; }

    public MathListDisplay(IDisplay[] displays): base() {
      Displays = displays.ToArray();
      MyLinePosition = LinePosition.Regular;
      IndexInParent = int.MinValue;
    }

    public float Ascent => Displays.CollectionAscent();
    public float Descent => Displays.CollectionDescent();
    public PointF Position { get; set; }
    public RectangleF DisplayBounds => this.ComputeDisplayBounds();
    public Range Range => RangeExtensions.Combine(Displays.Select(d => d.Range));
    public float Width {
      get {
        var x = Displays.Min(d => d.Position.X);
        var maxX = Displays.Max(d => d.Position.X + d.Width);
        return maxX - x;
      }
    }
    public void Draw(IGraphicsContext context) {
      context.SaveState();
      context.Translate(this.Position);
      context.SetTextPosition(new PointF());
      foreach (var displayAtom in Displays) {
        displayAtom.Draw(context);
      }
      context.RestoreState();
    }
  }
}
