using CSharpMath.Atoms;
using CSharpMath.Display.Text;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display {
  /// <summary>Corresponds to MTCTLineDisplay in iOSMath. Will need to
  /// figure out Core Text a bit in order to fill this out.</summary> 
  public class TextLineDisplay : DisplayBase {
    public TextLine Line { get; private set; }
    public AttributedString AttributedText { get; private set; }

    public IMathAtom[] Atoms { get; private set; }

    public TextLineDisplay(AttributedString text, 
      PointF position, Range range, Font font, IEnumerable<IMathAtom> atoms) {
      Position = position;
      AttributedText = text;
      Atoms = atoms.ToArray();
    }

  }
}
