using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  public static class MathListPreprocessor {

    public static List<IMathAtom> Preprocess(IMathList list) {
      // Note: Some of the preprocessing described by the TeX algorithm is done in the finalize method of MTMathList.
      // Specifically rules 5 & 6 in Appendix G are handled by finalize.
      // This function does not do a complete preprocessing as specified by TeX either. It removes any special atom types
      // that are not included in TeX and applies Rule 14 to merge ordinary characters.
      var r = new List<IMathAtom>();
      IMathAtom prevNode = null;
      foreach(IMathAtom atom in list) {
        if (atom.AtomType == MathAtomType.Variable || atom.AtomType == MathAtomType.Number) {
          // These are not a TeX type nodes. TeX does this during parsing the input.
          // switch to using the font specified in the atom
          var newFont = ChangeFont(atom.Nucleus, atom.FontStyle);
          atom.AtomType = MathAtomType.Ordinary;
          atom.Nucleus = newFont;
        }
        if (atom.AtomType == MathAtomType.UnaryOperator) {
          // TeX treats these as ordinary. So will we.
          atom.AtomType = MathAtomType.Ordinary;
        }
        if (atom.AtomType == MathAtomType.Ordinary) {

          if (prevNode!=null && prevNode.AtomType == MathAtomType.Ordinary && prevNode.Subscript == null && prevNode.Superscript == null) {
            prevNode.Fuse(atom);
            continue;
          }
        }
        prevNode = atom;
        r.Add(atom);
      }
      return r;
    }

    private static string ChangeFont(string str, FontStyle fontStyle) {
      // TODO: Font manipulations here. See MTTypesetter.m method changeFont in iosMath.
      return str;
    }
  }
}
