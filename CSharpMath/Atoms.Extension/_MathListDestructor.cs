using System;
using System.Globalization;
using System.Text;
using CSharpMath.Interfaces;
using Builder = CSharpMath.Atoms.MathListBuilder;

namespace CSharpMath.Atoms.Extension {
  internal static class _MathListDestructor {
    internal static string MathAtomToString(I_ExtensionAtom atom) {
      switch (atom) {
        case RaiseBox r:
          return $@"\raisebox{{{r.Raise.Length}{(r.Raise.IsMu ? "mu" : "pt")}}}{{{Builder.MathListToString(r.InnerList)}}}";
        default:
          throw new InvalidCodePathException($"Atom implements {nameof(I_ExtensionAtom)} but its type is unknown?");
      }
    }
  }
}