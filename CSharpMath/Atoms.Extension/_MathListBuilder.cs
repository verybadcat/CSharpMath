using System;
using System.Globalization;
using System.Text;
using CSharpMath.Interfaces;
using Builder = CSharpMath.Atoms.MathListBuilder;

namespace CSharpMath.Atoms.Extension {
  internal static class _MathListBuilder {
    private static readonly MathAtom TeX = new Inner {
      //should be \textrm instead of \text
      InnerList = MathLists.FromString(@"\text{T\kern-.1667em\raisebox{-.5ex}{E}\kern-.125emX}")
        ?? throw new FormatException(@"A syntax error is present in the definition of \TeX.")
    };

    internal static MathAtom AtomForCommand(Builder b, string command) {
      switch (command) {
        case "kern":
        case "hskip":
          if (b._textMode) return b.ReadSpace();
          b.SetError($@"\{command} is not allowed in math mode");
          return null;
        case "mkern":
        case "mskip":
          if (!b._textMode) return b.ReadSpace();
          b.SetError($@"\{command} is not allowed in text mode");
          return null;
        case "raisebox":
          if (!b.ExpectCharacter('{')) { b.SetError("Expected {"); return null; }
          var space = b.ReadSpace();
          if(!b.ExpectCharacter('}')) { b.SetError("Expected }"); return null; }
          return new RaiseBox { Raise = space, InnerList = b.BuildInternal(true) };
        case "TeX":
          return TeX;
        default:
          return null;
      }
    }
    
    private static Space ReadSpace(this Builder b) {
      b.SkipSpaces();
      var sb = new StringBuilder();
      while (b.HasCharacters) {
        var ch = b.GetNextCharacter();
        if (char.IsDigit(ch) || ch == '.' || ch == '-' || ch == '+') {
          sb.Append(ch);
        } else {
          b.UnlookCharacter();
          break;
        }
      }
      var length = sb.ToString();
      if (length == string.Empty) {
        b.SetError("Expected length value");
        return null;
      }
      b.SkipSpaces();
      var unit = new char[2];
      for (int i = 0; i < 2 && b.HasCharacters; i++) {
        unit[i] = b.GetNextCharacter();
      }
      var (result, error) = Structures.Space.Create(length, new string(unit), b._textMode);
      if(result is null) {
        b.SetError(error);
        return null;
      }
      return new Space(result.Value);
    }
  }
}