using CSharpMath.Atoms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display {
  public class MathListDisplay: DisplayBase {
    public DisplayBase[] Displays { get; set; }
    public LinePosition MyLinePosition { get; set; }
    /// <summary> Recursively. While translating, we'll keep the iosMath name "setTextColor".</summary> 
    public void SetTextColor(Color textColor) {
      TextColor = textColor;
      foreach (var atom in Displays) {
        atom.TextColor = atom.LocalTextColor ?? textColor;
      }
    }
    /// <summary>For a subscript or superscript, this is the index in the
    /// parent list. For a regular list, it is int.MinValue.</summary>
    public int IndexInParent { get; set; }

    public MathListDisplay(DisplayBase[] displays, Range range ): base() {
      Displays = displays.ToArray();
      MyLinePosition = LinePosition.Regular;
      IndexInParent = int.MinValue;
      Range = range;
      _RecomputeDimensions();
    }

    private void _RecomputeDimensions() {
      float maxAscent = 0;
      float maxDescent = 0;
      float maxWidth = 0;
      foreach (var atom in Displays) {
        float ascent = Math.Max(0, atom.Position.Y + atom.Ascent);
        if (ascent > maxAscent) {
          maxAscent = ascent;
        }
        float descent = Math.Min(0, 0 - (atom.Position.Y - atom.Descent));
        if (descent > maxDescent) {
          maxDescent = descent;
        }
        float width = atom.Width + atom.Position.X;
        if (width > maxWidth) {
          maxWidth = width;
        }
      }
      Ascent = maxAscent;
      Descent = maxDescent;
      Width = maxWidth;
    }

    public override void Draw(IGraphicsContext context) {
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
