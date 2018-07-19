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
      var str = sb.ToString();
      if (str == string.Empty) {
        b.SetError("Expected length value");
        return null;
      }
      b.SkipSpaces();
      var chars = new char[2];
      for (int i = 0; i < 2 && b.HasCharacters; i++) {
        chars[i] = b.GetNextCharacter();
      }
      if (chars[1] == default) {
        b.SetError("Expected two-character length unit");
        return null;
      }

      if(!(float.TryParse(str, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out var points))) {
        b.SetError("Invalid length value");
        return null;
      }
      var unit = new string(chars);
      if (b._textMode) {
        switch (unit) {
          //https://en.wikibooks.org/wiki/LaTeX/Lengths
          case "pt": break;
          case "mm": points *= 7227f / 2540f; break;
          case "cm": points *= 7227f / 254f; break;
          case "in": points *= 72.27f; break;
          case "bp": points *= 1.00375f; break;
          case "pc": points *= 12f; break;
          case "dd": points *= 1238f / 1157f; break;
          case "cc": points *= 14856f / 1157f; break;
          case "nd": points *= 685f / 642f; break;
          case "nc": points *= 1370f / 107f; break;
          case "sp": points /= 65536f; break;

          case "em":
            return new Space(points * 18f, true);
          case "ex":
            return new Space(points * 9f, true);
          case "mu":
            b.SetError("The length unit mu is not allowed in text mode");
            return null;
          default:
            b.SetError($"Unsupported length unit {unit}");
            return null;
        }
        return new Space(points, false);
      } else {
        if (unit != "mu") {
          b.SetError("Only the length unit mu is allowed in math mode");
          return null;
        }
        return new Space(points, true);
      }
    }
  }
}