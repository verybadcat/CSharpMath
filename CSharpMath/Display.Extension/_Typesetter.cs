using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Atoms.Extension;
using CSharpMath.Enumerations;
using CSharpMath.Interfaces;

namespace CSharpMath.Display.Extension {
  internal static class _Typesetter {
    internal static void CreateDisplayAtom<TFont, TGlyph>(Typesetter<TFont, TGlyph> t, I_ExtensionAtom atom)
      where TFont : MathFont<TGlyph> {
      switch (atom.AtomType) {
        case MathAtomType.RaiseBox:
          t.AddDisplayLine(false);
          var raiseBox = atom as RaiseBox;
          var raisedDisplay = Typesetter<TFont, TGlyph>.CreateLine(raiseBox.InnerList, t._font, t._context, t._style);
          var raisedPosition =  t._currentPosition;
          raisedPosition.Y += raiseBox.Raise.ActualLength(t._mathTable, t._font);
          raisedDisplay.Position = raisedPosition;
          t._currentPosition.X += raisedDisplay.Width;
          t._displayAtoms.Add(raisedDisplay);
          break;
      }
    }

    internal static int GetInterElementSpaceArrayIndexForType(MathAtomType atomType) {
      switch (atomType) {
        case MathAtomType.RaiseBox: return 0; //Same as Color
      }
      return -1; // If reach here, then WILL THROW
    }
  }
}
